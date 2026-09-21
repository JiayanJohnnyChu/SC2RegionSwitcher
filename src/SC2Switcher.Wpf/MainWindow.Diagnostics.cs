using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sc2Switch2;

namespace Sc2Wpf;

public partial class MainWindow {
    [DllImport("user32.dll")] static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool AreDpiAwarenessContextsEqual(IntPtr a,IntPtr b);

    static bool Fits(FrameworkElement element,FrameworkElement viewport) {
        if(element==null||!element.IsVisible)return false;
        Point p=element.TranslatePoint(new Point(),viewport);
        return p.X>=-0.5&&p.Y>=-0.5&&p.X+element.ActualWidth<=viewport.ActualWidth+0.5&&p.Y+element.ActualHeight<=viewport.ActualHeight+0.5;
    }

    static string BrushValue(Brush brush)=>(brush as SolidColorBrush)?.Color.ToString();
    static object DestinationAppearance(RadioButton button) {
        var tile=button.Template.FindName("Tile",button) as Border;
        var text=((Panel)button.Content).Children.OfType<TextBlock>().ToArray();
        return new {Selected=button.IsChecked,Enabled=button.IsEnabled,Background=BrushValue(tile?.Background),
            Foreground=BrushValue(button.Foreground),LabelForeground=BrushValue(text[0].Foreground),
            DetailForeground=BrushValue(text[^1].Foreground),LabelOpacity=text[0].Opacity,DetailOpacity=text[^1].Opacity,TileOpacity=tile?.Opacity};
    }

    void WriteDiagnostics(string path) {
        UpdateLayout();
        var dpi=VisualTreeHelper.GetDpi(this);
        FrameworkElement primary=settingsPage!=null?(FrameworkElement)settingsPage.FindName("SaveButton"):
            sheetCloseButton!=null?sheetCloseButton:model.IsIdle?(FrameworkElement)SwitchButton:BusyIndicator;
        bool reachable=Fits(primary,host);
        bool? targetsVisible=sheet==null?Fits(ChinaCard,MainScroll)&&Fits(GlobalCard,MainScroll)&&Fits(RegionRow,MainScroll):null;
        var formScroll=settingsPage?.FindName("FormScroll") as ScrollViewer;
        Json.Write(path,new {
            Framework="WPF",Design="Swiss modernism / English first / Radix regions",Version=typeof(MainWindow).Assembly.GetName().Version.ToString(3),
            SyntheticState=exportingDesignStates,Page=settingsPage!=null?"settings":sheet!=null?"reference":"switcher",
            UiLanguage=UiText.Language,WindowTitle=Title,WindowVisible=IsVisible,WindowState=WindowState.ToString(),UserCancellationAvailable=false,CommitStarted=model.IsCommitStarted,
            Runtime=Environment.Version.ToString(),DpiX=dpi.PixelsPerInchX,DpiY=dpi.PixelsPerInchY,
            PerMonitorV2=AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(new WindowInteropHelper(this).Handle),new IntPtr(-4)),
            UseLayoutRounding,SnapsToDevicePixels,Width=ActualWidth,Height=ActualHeight,Selected=model.Target,
            Ready=model.CanSwitch,CurrentRegion=model.CurrentRegion,Status=model.StatusTitle,
            CurrentLoginRegion=model.CurrentLoginRegion,
            Palette=new {Paper=BrushValue(Root.Background),CurrentMarker=BrushValue(CurrentMarker.BorderBrush),
                China=DestinationAppearance(ChinaCard),Global=DestinationAppearance(GlobalCard),Action=BrushValue(SwitchButton.Background)},
            ScrollHeight=sheet==null?MainScroll.ScrollableHeight:formScroll?.ScrollableHeight,
            PrimaryActionFullyVisible=reachable,TargetsAndRegionsFullyVisible=targetsVisible,
            Versions=builds.Select(x=>x?.Version).ToArray()
        });
        if(!reachable)throw new InvalidOperationException("The primary action is not fully visible.");
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(host.ActualWidth*dpi.DpiScaleX),(int)Math.Ceiling(host.ActualHeight*dpi.DpiScaleY),dpi.PixelsPerInchX,dpi.PixelsPerInchY,PixelFormats.Pbgra32);
        bitmap.Render(host);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream=File.Create(Path.ChangeExtension(path,"png"));encoder.Save(stream);
    }

    // Existing diagnostics only: --matrix requires its own --data-dir.
    // Fixtures affect presentation, never the switching engine or user installations.
    public async Task ExportDesignStates(string directory) {
        while(inspecting)await Task.Delay(10);
        exportingDesignStates=true;
        initialConfiguration.NeedsSetup=false;
        settings=ConfigurationValidator.Clone(settings);
        settings.BattleNetPath=@"C:\Program Files (x86)\Battle.net\Battle.net.exe";
        settings.CN.GamePath=@"C:\Games\StarCraft II China";
        settings.Global.GamePath=@"C:\Games\StarCraft II Global";
        settings.VariablesPath=@"C:\Game settings\StarCraft II\Variables.txt";
        builds[0]=new BuildInfo{Version="5.0.0.00000",Branch="cn"};
        builds[1]=new BuildInfo{Version="5.0.0.00000",Branch="eu"};
        async Task Snapshot(string name) {
            await Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);
            await Task.Delay(80);
            WriteDiagnostics(Path.Combine(directory,UiText.Language+"-"+name+".json"));
        }
        foreach(string language in new[]{"en-US","zh-CN"}) {
            UiText.SetLanguage(language);
            Width=760;Height=620;
            model.Initialize("EU","EU");model.SetReady(true);
            model.Status("本地安装检查通过","切换将重启战网，需先退出游戏并完成更新。");
            await Snapshot("main-global");
            model.SetCurrent("CN");await Snapshot("main-current-china");model.SetCurrent("EU");
            model.IsChina=true;await Snapshot("main-china");model.IsGlobal=true;
            Width=520;Height=560;await Snapshot("main-compact");
            model.IsChina=true;await Snapshot("main-china-compact");model.IsGlobal=true;
            model.SetBusy(true);model.SetStage(2);
            model.Status("正在设置游戏语言","正在设置目标语言并保存原始配置备份。","working");await Snapshot("busy-language");
            model.SetPhase(SwitchPhase.StartingBattleNet);
            model.Status("正在打开目标战网","正在核对战网区域。完成后恢复操作。","working");await Snapshot("busy-confirm");
            model.SetBusy(false);
            model.Status("操作未完成","检测到游戏文件变化或监测中断。需等待更新、安装或修复结束后重新检查。","error");await Snapshot("error-compact");
            Width=760;Height=620;
            StatusDetails_Click(this,new RoutedEventArgs());await Snapshot("error-details");CloseSheet();
            model.Status("已打开外服战网","区域配置已核对；账号登录与游戏启动需在战网中完成。");await Snapshot("success");
            OpenSettings();await Snapshot("settings");
            var pageModel=(SettingsPageModel)settingsPage.DataContext;
            pageModel.GlobalDirectory=@"C:\Games\StarCraft II Global - edited";
            settingsPage.CanLeave();await Snapshot("settings-unsaved");
            pageModel.Reset();
            Width=520;Height=560;await Snapshot("settings-compact");
            ((Expander)settingsPage.FindName("Advanced")).IsExpanded=true;
            ((FrameworkElement)settingsPage.FindName("VariablesField")).BringIntoView();await Snapshot("settings-advanced");
            CloseSheet();Width=760;Height=620;
            Details_Click(this,new RoutedEventArgs());await Snapshot("installations");CloseSheet();
            Help_Click(this,new RoutedEventArgs());await Snapshot("help");CloseSheet();
        }
        Close();
    }
}
