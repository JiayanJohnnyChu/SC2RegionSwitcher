using System;
using System.Collections.Generic;
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

public partial class SwissWindow {
    string designLanguageState="available";
    Task<IReadOnlyList<string>> DiscoverDesignLanguages(string path) => designLanguageState switch {
        "missing"=>Task.FromException<IReadOnlyList<string>>(new IOException(UiText.T("目录不存在或无法访问。"))),
        "empty"=>Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>()),
        _=>Task.FromResult<IReadOnlyList<string>>(new[]{"deDE","enUS","esES","koKR","zhTW"})
    };
    [DllImport("user32.dll")] static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool AreDpiAwarenessContextsEqual(IntPtr a,IntPtr b);

    static bool Fits(FrameworkElement element,FrameworkElement viewport) {
        if(element==null||!element.IsVisible)return false;
        Point p=element.TranslatePoint(new Point(),viewport);
        return p.X>=-0.5&&p.Y>=-0.5&&p.X+element.ActualWidth<=viewport.ActualWidth+0.5&&p.Y+element.ActualHeight<=viewport.ActualHeight+0.5;
    }

    static string BrushValue(Brush brush)=>(brush as SolidColorBrush)?.Color.ToString();
    static object[] RenderedFonts(Visual visual){
        var fonts=new List<object>();
        void Read(Drawing drawing){
            if(drawing is GlyphRunDrawing glyph){var face=glyph.GlyphRun.GlyphTypeface;fonts.Add(new{File=Path.GetFileName(face.FontUri.ToString()),Weight=face.Weight.ToOpenTypeWeight(),Simulation=face.StyleSimulations.ToString(),MissingGlyph=glyph.GlyphRun.GlyphIndices.Contains((ushort)0)});}
            if(drawing is DrawingGroup group)foreach(var child in group.Children)Read(child);
        }
        void Visit(Visual item){var drawing=VisualTreeHelper.GetDrawing(item);if(drawing!=null)Read(drawing);for(int i=0;i<VisualTreeHelper.GetChildrenCount(item);i++)if(VisualTreeHelper.GetChild(item,i) is Visual child)Visit(child);}
        Visit(visual);return fonts.ToArray();
    }
    static object DestinationAppearance(RadioButton button) {
        var tile=button.Template.FindName("Tile",button) as Border;
        var choice=(InstallationChoice)button;
        return new {Selected=button.IsChecked,Enabled=button.IsEnabled,RegionLabel=choice.RegionCode,Label=choice.Label,Background=BrushValue(tile?.Background),Foreground=BrushValue(button.Foreground),TileOpacity=tile?.Opacity};
    }

    void WriteDiagnostics(string path) {
        UpdateLayout();
        var dpi=VisualTreeHelper.GetDpi(this);
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(Shell.ActualWidth*dpi.DpiScaleX),(int)Math.Ceiling(Shell.ActualHeight*dpi.DpiScaleY),dpi.PixelsPerInchX,dpi.PixelsPerInchY,PixelFormats.Pbgra32);
        bitmap.Render(Shell);
        FrameworkElement primary=settingsPage!=null?(FrameworkElement)settingsPage.FindName("SaveButton"):
            sheetCloseButton!=null?sheetCloseButton:model.IsIdle?(FrameworkElement)SwitchButton:BusyIndicator;
        bool reachable=Fits(primary,host);
        bool? targetsVisible=sheet==null?Fits(ChinaCard,MainScroll)&&Fits(GlobalCard,MainScroll)&&Fits(RegionRow,MainScroll):null;
        var formScroll=settingsPage?.FindName("FormScroll") as ScrollViewer;
        var languagePicker=settingsPage?.FindName("GameLanguagePicker") as ComboBox;
        var pageModel=settingsPage?.DataContext as SettingsPageModel;
        Json.Write(path,new {
            Framework="WPF",Design="Swiss S-06",Version=typeof(SwissWindow).Assembly.GetName().Version.ToString(3),
            SyntheticState=exportingDesignStates,Page=settingsPage!=null?"settings":sheet!=null?"reference":"switcher",
            UiLanguage=UiText.Language,WindowTitle=Title,WindowVisible=IsVisible,WindowState=WindowState.ToString(),UserCancellationAvailable=false,CommitStarted=model.IsCommitStarted,
            Runtime=Environment.Version.ToString(),DpiX=dpi.PixelsPerInchX,DpiY=dpi.PixelsPerInchY,
            PerMonitorV2=AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(new WindowInteropHelper(this).Handle),new IntPtr(-4)),
            UseLayoutRounding,SnapsToDevicePixels,Width=ActualWidth,Height=ActualHeight,ClientWidth=Shell.ActualWidth,ClientHeight=Shell.ActualHeight,Selected=model.Target,
            Ready=model.CanSwitch,PrimaryEnabled=SwitchButton.IsEnabled,CurrentRegion=model.CurrentRegion,Status=model.StatusTitle,
            CurrentLoginRegion=model.CurrentLoginRegion,
            GlobalGameLanguage=model.GlobalGameLanguage,
            RepairField=repairField,FocusedControl=(System.Windows.Input.Keyboard.FocusedElement as FrameworkElement)?.Name,
            StatusLayout=new {MarkerWidth=StatusMarker.ActualWidth,MarkerBorder=StatusMarker.BorderThickness.Left,TitleSize=StatusHeading.FontSize,BodyLineHeight=Double.IsNaN(StatusBody.LineHeight)?(double?)null:StatusBody.LineHeight,ActionsRow=Grid.GetRow(RecoveryActions),ActionColumns=RecoveryActions.ColumnDefinitions.Select(c=>c.ActualWidth).ToArray(),RepairRow=Grid.GetRow(RepairPathButton),CheckRow=Grid.GetRow(CheckButton),DetailsRow=Grid.GetRow(ErrorDetailsButton),Background=BrushValue(Readiness.Background)},
            TitleFonts=RenderedFonts(HeroTitle),ContinuousFonts=RenderedFonts(GameLanguageValue),
            GameLanguages=pageModel==null?null:new {Options=pageModel.AvailableLanguages.Select(x=>x.Code).ToArray(),Selected=pageModel.GlobalLanguage,Summary=pageModel.GameLanguageSummary,Message=pageModel.DetectionMessage,Enabled=languagePicker.IsEnabled,PickerFullyVisible=Fits(languagePicker,formScroll),HasChanges=pageModel.HasChanges,IsSaving=pageModel.IsSaving},
            Palette=new {Paper=BrushValue(Shell.Background),CurrentMarker=BrushValue(CurrentMarker.BorderBrush),
                China=DestinationAppearance(ChinaCard),Global=DestinationAppearance(GlobalCard),Action=BrushValue(SwitchButton.Background)},
            ScrollHeight=sheet==null?MainScroll.ScrollableHeight:formScroll?.ScrollableHeight,
            PrimaryActionFullyVisible=reachable,TargetsAndRegionsFullyVisible=targetsVisible,
            Versions=builds.Select(x=>x?.Version).ToArray()
        });
        // Home parameters and action deliberately share one scroll surface.
        // Keyboard/scroll reachability is checked by the matrix, not initial visibility.
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream=File.Create(Path.ChangeExtension(path,"png"));encoder.Save(stream);
    }

    // Existing diagnostics only: --matrix requires its own --data-dir.
    // Fixtures affect presentation, never the switching engine or user installations.
    public async Task ExportDesignStates(string directory) {
        while(inspecting)await Task.Delay(10);
        exportingDesignStates=true;
        Json.Write(Path.Combine(directory,"font-catalog.json"),Fonts.GetFontFamilies(new Uri("pack://application:,,,/SC2Switcher.Wpf;component/"),"./Fonts/").Select(f=>new{f.Source,Names=f.FamilyNames.Values.ToArray(),Faces=f.GetTypefaces().Select(t=>new{Weight=t.Weight.ToOpenTypeWeight(),Names=t.FaceNames.Values.ToArray()}).ToArray()}).ToArray());
        initialConfiguration.NeedsSetup=false;
        settings=ConfigurationValidator.Clone(settings);
        settings.BattleNetPath=@"C:\Program Files (x86)\Battle.net\Battle.net.exe";
        settings.CN.GamePath=@"C:\Games\StarCraft II China";
        settings.Global.GamePath=@"C:\Games\StarCraft II Global";
        settings.VariablesPath=@"C:\Game settings\StarCraft II\Variables.txt";
        builds[0]=new BuildInfo{Version="5.0.0.00000",Branch="cn"};
        builds[1]=new BuildInfo{Version="5.0.0.00000",Branch="eu"};
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--interactions-only")>=0){
            await ExportInteractionChecks(directory);Close();return;
        }
        if(Array.IndexOf(Environment.GetCommandLineArgs(),"--status-only")>=0){
            await ExportStatusStudy(directory);Close();return;
        }
        async Task Snapshot(string name) {
            await Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);
            await Task.Delay(80);
            WriteDiagnostics(Path.Combine(directory,UiText.Language+"-"+name+".json"));
        }
        var arguments=Environment.GetCommandLineArgs();int languageIndex=Array.IndexOf(arguments,"--matrix-languages");
        var languages=languageIndex>=0?arguments[languageIndex+1].Split(','):UiTypography.Languages.Select(x=>x.Code).ToArray();
        if(languages.Any(x=>!UiText.IsSupported(x)))throw new ArgumentException("Unsupported matrix language.");
        foreach(string language in languages) {
            UiText.SetLanguage(language);
            settings.Global.TextLocale=settings.Global.SpeechLocale="enUS";
            Width=1000;Height=820;
            model.Initialize("EU","EU");model.SetGameLanguages(settings.Global);model.SetReady(true);
            model.SetInstallation(true,true);model.SetInstallation(false,true);
            model.SetApplied(new AppliedConfiguration{TextLocale="enUS",SpeechLocale="enUS"});
            model.Status("web.installationsReady","");
            await Snapshot("main-global");
            if(SwitchButton.IsEnabled)throw new InvalidOperationException("Unchanged action was enabled in the actual control.");
            model.SetCurrent("CN");await Snapshot("main-current-china");model.SetCurrent("EU");
            model.IsChina=true;await Snapshot("main-china");model.IsGlobal=true;
            Width=520;Height=560;await Snapshot("main-compact");
            SwitchButton.BringIntoView();await Snapshot("main-compact-action");
            if(!Fits(SwitchButton,MainScroll))throw new InvalidOperationException("Primary action is not reachable by scrolling.");
            MainScroll.ScrollToTop();
            Width=360;Height=740;await Snapshot("main-small");SwitchButton.BringIntoView();await Snapshot("main-small-action");
            if(!Fits(SwitchButton,MainScroll))throw new InvalidOperationException("Small-window action is not reachable.");
            Width=520;Height=560;MainScroll.ScrollToTop();
            model.IsChina=true;await Snapshot("main-china-compact");model.IsGlobal=true;
            model.SetBusy(true);model.SetStage(2);
            model.Status("正在设置游戏语言","正在设置目标语言并保存原始配置备份。","working");await Snapshot("busy-language");
            if(HeaderLanguage.IsEnabled||Navigation.IsEnabled||ChinaCard.IsEnabled||GlobalCard.IsEnabled)throw new InvalidOperationException("Busy presentation left mutable controls enabled.");
            model.SetPhase(SwitchPhase.StartingBattleNet);
            model.Status("正在打开目标战网","正在核对战网区域。完成后恢复操作。","working");await Snapshot("busy-confirm");
            model.SetBusy(false);
            model.Status("操作未完成","检测到游戏文件变化或监测中断。需等待更新、安装或修复结束后重新检查。","error");await Snapshot("error-compact");
            Width=760;Height=620;
            StatusDetails_Click(this,new RoutedEventArgs());await Snapshot("error-details");CloseSheet();
            Width=360;Height=740;StatusDetails_Click(this,new RoutedEventArgs());await Snapshot("error-details-small");CloseSheet();Width=760;Height=620;
            model.Status("已打开外服战网","区域配置已核对；账号登录与游戏启动需在战网中完成。");await Snapshot("success");
            OpenSettings();await Snapshot("settings");
            var pageModel=(SettingsPageModel)settingsPage.DataContext;
            pageModel.GlobalDirectory=@"C:\Games\StarCraft II Global - edited";
            Home_Click(this,new RoutedEventArgs());
            if(settingsPage==null||!model.SettingsActive)throw new InvalidOperationException("Navigation discarded unsaved changes.");
            await Snapshot("settings-unsaved");
            pageModel.Reset();
            Width=520;Height=560;await Snapshot("settings-compact");
            ((Expander)settingsPage.FindName("Advanced")).IsExpanded=true;
            ((FrameworkElement)settingsPage.FindName("VariablesField")).BringIntoView();await Snapshot("settings-advanced");
            foreach(bool compact in new[]{false,true}) {
                Width=compact?520:760;Height=compact?560:620;
                string suffix=compact?"-compact":"";
                designLanguageState="available";await pageModel.DetectLanguagesAsync();
                await Dispatcher.InvokeAsync(()=>settingsPage.UpdateLayout(),DispatcherPriority.ApplicationIdle);
                ((FrameworkElement)settingsPage.FindName("GameLanguageSection")).BringIntoView();
                await Snapshot("languages-available"+suffix);
                ((ComboBox)settingsPage.FindName("GameLanguagePicker")).SelectedValue="esES";
                await Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);
                if(!pageModel.HasChanges||pageModel.Candidate().Global.TextLocale!="esES")throw new InvalidOperationException("Language picker binding did not update the draft.");
                settingsPage.CanLeave();await Snapshot("languages-unsaved"+suffix);
                pageModel.SetSaving(true);await Snapshot("languages-busy"+suffix);pageModel.SetSaving(false);pageModel.Reset();
                designLanguageState="empty";await pageModel.DetectLanguagesAsync();await Snapshot("languages-empty"+suffix);
                designLanguageState="missing";await pageModel.DetectLanguagesAsync();await Snapshot("languages-missing"+suffix);
            }
            designLanguageState="available";
            CloseSheet();Width=760;Height=620;
            settings.Global.TextLocale=settings.Global.SpeechLocale="esES";model.SetGameLanguages(settings.Global);
            await Snapshot("main-global-language");Width=520;Height=560;await Snapshot("main-global-language-compact");Width=760;Height=620;
            Details_Click(this,new RoutedEventArgs());await Snapshot("installations");CloseSheet();
            Help_Click(this,new RoutedEventArgs());await Snapshot("help");CloseSheet();
        }
        Close();
    }
    async Task ExportStatusStudy(string directory){
        async Task SizeClient(double width,double height){
            Width=width+(ActualWidth-Shell.ActualWidth);Height=height+(ActualHeight-Shell.ActualHeight);
            await Dispatcher.InvokeAsync(()=>UpdateLayout(),DispatcherPriority.ApplicationIdle);
        }
        foreach(var item in new[]{("en-US",1280d,800d),("fr-FR",520d,740d),("zh-CN",360d,740d)}){
            UiText.SetLanguage(item.Item1);await SizeClient(item.Item2,item.Item3);
            model.Initialize("EU","EU");model.SetGameLanguages(settings.Global);model.SetReady(false);
            model.SetApplied(new AppliedConfiguration{TextLocale="enUS",SpeechLocale="enUS"});
            model.SetInstallation(true,true);model.SetInstallation(false,false);
            model.Status("web.installationBlocked",UiText.Format("web.pathRecovery",@"D:\Games\StarCraft II"),"error");
            await Dispatcher.InvokeAsync(()=>UpdateLayout(),DispatcherPriority.ApplicationIdle);
            if(item.Item1=="zh-CN")SwitchButton.BringIntoView();else MainScroll.ScrollToTop();
            await Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);
            WriteDiagnostics(Path.Combine(directory,"status-"+item.Item1+"-error.json"));
        }
        StatusDetails_Click(this,new RoutedEventArgs());
        await Dispatcher.InvokeAsync(()=>UpdateLayout(),DispatcherPriority.ApplicationIdle);
        WriteDiagnostics(Path.Combine(directory,"status-zh-CN-details.json"));CloseSheet();
        UiText.SetLanguage("en-US");await SizeClient(1120,800);
        model.SetInstallation(false,true);model.SetReady(true);model.Status("web.installationsReady","");MainScroll.ScrollToTop();
        await Dispatcher.InvokeAsync(()=>UpdateLayout(),DispatcherPriority.ApplicationIdle);
        WriteDiagnostics(Path.Combine(directory,"status-en-US-ready.json"));
    }
}
