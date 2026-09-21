using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Automation;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sc2Switch2;

namespace Sc2Wpf;
public partial class MainWindow : Window {
    Settings settings;
    readonly ConfigurationStore configuration;
    readonly ConfigurationLoadResult initialConfiguration;
    SettingsPage settingsPage;
    readonly string state,report;
    NativePlatform platform;
    readonly MainViewModel model=new();
    readonly BuildInfo[] builds=new BuildInfo[2];
    readonly Grid host=new();
    Border sheet;
    IInputElement previousFocus;
    bool inspecting;
    SwitchPhase currentPhase;
    readonly string preferencesPath;
    public MainWindow(ConfigurationLoadResult loaded,ConfigurationStore configuration,string state,string report) {
        this.initialConfiguration=loaded;this.settings=loaded.Settings;this.configuration=configuration;this.state=state;this.report=report;
        preferencesPath=Path.Combine(state,"ui-preferences.json");
        platform=new NativePlatform(settings.BattleNetPath);
        InitializeComponent();Content=null;host.Children.Add(Root);Content=host;DataContext=model;model.Initialize(platform.ReadRegion(),settings.GlobalRegion);
        Loaded+=async(_,_)=>{try{await Inspect();if(initialConfiguration.NeedsSetup&&report==null)OpenSettings();if(report!=null)await Dispatcher.InvokeAsync(()=>WriteDiagnostics(report),DispatcherPriority.ApplicationIdle);}catch(Exception error){ShowError(error);}};
        SizeChanged+=(_,_)=>AdaptLayout();
        UiText.Instance.PropertyChanged+=LanguageChanged;
        Closed+=(_,_)=>UiText.Instance.PropertyChanged-=LanguageChanged;
        Closing+=(_,e)=>{if(!model.IsIdle||settingsPage?.IsSaving==true){e.Cancel=true;model.Status("切换尚未结束","请等待切换结束后再关闭窗口。","working");}else if(settingsPage!=null&&!settingsPage.CanLeave())e.Cancel=true;};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape&&sheet!=null){CloseSheet();e.Handled=true;}if(e.Key==Key.F12&&report!=null&&sheet==null){WriteDiagnostics(Path.Combine(Path.GetDirectoryName(report),model.Target+(ActualWidth<740?"-narrow":"")+".json"));e.Handled=true;}};
    }
    void AdaptLayout(){
        double margin=ActualWidth<620?24:32;
        Workspace.Margin=new Thickness(margin,25,margin,22);
        DockContent.Margin=new Thickness(margin,19,margin,22);
    }
    async Task Inspect(){
        if(!model.IsIdle||inspecting)return;
        inspecting=true;model.SetInspecting(true);model.SetReady(false);model.Status("正在检查安装","读取两个版本的安装信息。","working");
        try{
            var snapshot=ConfigurationValidator.Clone(settings);
            var result=await Task.Run(()=>{
                var errors=new List<string>();
                builds[0]=null;builds[1]=null;
                try {ConfigurationValidator.Validate(snapshot);builds[0]=BuildInfo.Load(snapshot.CN,true);builds[1]=BuildInfo.Load(snapshot.Global,false);}
                catch(Exception error){errors.Add(error is SettingsValidationException validation?UiText.T(SettingsPageModel.FieldName(validation.Field))+": "+error.Message:error.Message);}
                return errors;
            });
            model.SetInstallation(true,builds[0]!=null);model.SetInstallation(false,builds[1]!=null);model.SetCurrent(platform.ReadRegion());
            if(result.Count>0){model.Status("安装检查未通过",String.Join("\n",result),"error");return;}
            if(platform.GameRunning())model.Status("检测到游戏正在运行","需退出游戏和地图编辑器后重新检查。","error");
            else {model.SetReady(true);model.Status("本地安装检查通过","切换将重启战网，需先退出游戏并完成更新。");}
        }finally{inspecting=false;model.SetInspecting(false);}
    }
    async void Check_Click(object sender,RoutedEventArgs e){try{await Inspect();}catch(Exception error){ShowError(error);}}
    async void Switch_Click(object sender,RoutedEventArgs e){await Switch();}
    async Task Switch(){
        if(!model.CanSwitch||inspecting)return;
        bool acquired=false;
        using var mutex=new Mutex(false,"Local\\SC2DualRegionSwitcher");
        try{
            try{acquired=mutex.WaitOne(0);}catch(AbandonedMutexException){acquired=true;}
            if(!acquired)throw new IOException(UiText.T("另一个切换窗口正在操作，请稍后重试。"));
            // Keep the mutex on the dispatcher thread across awaits: Mutex ownership is thread-affine.
            model.SetBusy(true);
            var candidate=ConfigurationValidator.Clone(settings);candidate.GlobalRegion=model.Region;
            settings=configuration.Save(candidate);
            var result=await new Engine(settings,state,platform).Switch(model.Target,Progress,CancellationToken.None,PhaseChanged);
            model.SetCurrent(result.Region);
            model.Status(result.Target=="CN"?"已打开国服战网":"已打开外服战网","区域配置已核对；账号登录与游戏启动需在战网中完成。");
        }catch(Exception error){ShowError(error);}
        finally{model.SetBusy(false);model.SetCurrent(platform.ReadRegion());if(acquired)mutex.ReleaseMutex();}
    }
    void PhaseChanged(SwitchPhase phase){currentPhase=phase;model.SetPhase(phase);}
    void Progress(string text){
        string title=currentPhase switch {
            SwitchPhase.StoppingBattleNet=>"正在退出战网",
            SwitchPhase.SettingLanguage=>"正在设置游戏语言",
            SwitchPhase.StartingBattleNet or SwitchPhase.Completed=>"正在打开目标战网",
            _=>"正在检查切换条件"
        };
        model.Status(title,text,"working");
    }
    void LanguageChanged(object sender,System.ComponentModel.PropertyChangedEventArgs e){
        try {UiPreferences.Save(preferencesPath,UiText.Language);}
        catch(Exception error) when(error is IOException||error is UnauthorizedAccessException){
            model.Status("操作未完成","无法保存界面语言偏好。本次界面语言已应用，下次启动可能恢复为原设置。","error");
            settingsPage?.ShowMessage("无法保存界面语言偏好。本次界面语言已应用，下次启动可能恢复为原设置。",true);
        }
    }
    void ShowError(Exception error){model.Status("操作未完成",error.Message,"error");try{Json.Write(Path.Combine(state,"last-error.json"),new {Time=DateTime.Now.ToString("o"),Message=error.Message});}catch{}}
    void StatusDetails_Click(object sender,RoutedEventArgs e)=>ShowSheet(model.StatusTitle,"以下列出错误信息及处理建议。",new(){
        ("错误信息",model.StatusDetail),
        ("使用条件","游戏及地图编辑器需处于关闭状态。战网下载或更新完成后，可执行“重新检查”。"),
        ("进一步检查","在“安装详情”中核对两套游戏目录。进一步排查时，应保留原始错误信息及备份文件。")});
    void Details_Click(object sender,RoutedEventArgs e){
        var sections=new List<(string,string)>{
            ("国服 · 简体中文",(builds[0]?.Version??UiText.T("安装未通过检查"))+"\n"+settings.CN.GamePath),
            ("外服 · English",(builds[1]?.Version??UiText.T("安装未通过检查"))+"\n"+settings.Global.GamePath),
            ("Battle.net",settings.BattleNetPath),
            ("共享游戏设置",settings.VariablesPath)};
        ShowSheet("安装详情","以下信息来源于本地安装目录及配置文件。",sections);
    }
    void Help_Click(object sender,RoutedEventArgs e)=>ShowSheet("使用帮助","切换流程、使用条件及状态说明。",new(){
        ("1. 切换前提","星际争霸 II 和地图编辑器需处于关闭状态。战网下载或更新任务需已完成。"),
        ("2. 目标配置","国服使用简体中文，外服使用英文。外服登录区域可设为欧洲、美洲或亚洲；游戏服务器需在战网中另行选择。"),
        ("3. 登录与启动","切换过程会重新启动战网。账号登录及游戏启动在战网中完成。"),
        ("切换期间","切换期间会锁定目标和界面语言，并阻止关闭窗口。选错目标时，请等待结束后再切换。"),
        ("界面语言","界面语言与游戏语言相互独立。界面语言的修改会立即生效并保存，不会修改两套游戏的语言设置。"),
        ("状态含义","“战网当前配置”读取自本地配置文件，不表示账号已登录。“本地安装检查通过”不表示已完成账号认证或在线连接验证。")});
    void Settings_Click(object sender,RoutedEventArgs e)=>OpenSettings();
    void OpenSettings(){
        if(sheet!=null||!model.CanInspect||inspecting)return;
        previousFocus=Keyboard.FocusedElement;Root.IsEnabled=false;Root.Visibility=Visibility.Hidden;
        sheet=new Border{Background=Background};
        KeyboardNavigation.SetTabNavigation(sheet,KeyboardNavigationMode.Cycle);
        settingsPage=new SettingsPage(settings,initialConfiguration.NeedsSetup?initialConfiguration.NoticeKey:null,SaveDirectories);
        settingsPage.CloseRequested+=CloseSheet;
        sheet.Child=settingsPage;host.Children.Add(sheet);
    }
    async Task<Settings> SaveDirectories(Settings candidate){
        var saved=await Task.Run(()=>configuration.Save(candidate));
        settings=saved;platform=new NativePlatform(settings.BattleNetPath);initialConfiguration.NeedsSetup=false;
        await Inspect();return ConfigurationValidator.Clone(settings);
    }
    void ShowSheet(string title,string intro,List<(string,string)> sections){
        if(sheet!=null)return;
        previousFocus=Keyboard.FocusedElement;Root.IsEnabled=false;Root.Visibility=Visibility.Hidden;
        sheet=new Border{Background=Background,Padding=new Thickness(40,32,40,28)};
        KeyboardNavigation.SetTabNavigation(sheet,KeyboardNavigationMode.Cycle);
        var grid=new Grid();grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});grid.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
        var stack=new StackPanel();stack.Children.Add(new TextBlock{Text=UiText.T(title),FontSize=26,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8)});stack.Children.Add(new TextBlock{Text=UiText.T(intro),Foreground=(Brush)FindResource("Muted"),Margin=new Thickness(0,0,0,24)});
        foreach(var section in sections){
            stack.Children.Add(new TextBlock{Text=UiText.T(section.Item1),FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,8)});
            stack.Children.Add(new TextBox{Text=UiText.T(section.Item2),IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Background=Brushes.Transparent,BorderThickness=new Thickness(0),Padding=new Thickness(0),Foreground=(Brush)FindResource("Muted"),FontSize=13,Margin=new Thickness(0,0,0,22),VerticalScrollBarVisibility=ScrollBarVisibility.Disabled});
        }
        grid.Children.Add(new ScrollViewer{Content=stack,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled});
        var close=new Button{Content=UiText.T("返回切换"),Style=(Style)FindResource("PrimaryButton"),HorizontalAlignment=HorizontalAlignment.Right,MinWidth=140,Margin=new Thickness(0,18,0,0)};close.Click+=(_,_)=>CloseSheet();Grid.SetRow(close,1);grid.Children.Add(close);sheet.Child=grid;host.Children.Add(sheet);close.Focus();
    }
    void CloseSheet(){if(sheet==null)return;if(settingsPage!=null&&!settingsPage.CanLeave())return;host.Children.Remove(sheet);sheet=null;settingsPage=null;Root.Visibility=Visibility.Visible;Root.IsEnabled=true;if(previousFocus!=null)Keyboard.Focus(previousFocus);}
    [DllImport("user32.dll")]static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hwnd);
    [DllImport("user32.dll")]static extern bool AreDpiAwarenessContextsEqual(IntPtr a,IntPtr b);
    void WriteDiagnostics(string path){
        var dpi=VisualTreeHelper.GetDpi(this);
        var primary=model.IsIdle?(FrameworkElement)SwitchButton:BusyIndicator;
        Point location=primary.TranslatePoint(new Point(),Root);
        bool reachable=location.Y>=0&&location.Y+primary.ActualHeight<=Root.ActualHeight&&location.X>=0&&location.X+primary.ActualWidth<=Root.ActualWidth;
        Json.Write(path,new {Framework="WPF",Design="Round 2",Version=typeof(MainWindow).Assembly.GetName().Version.ToString(3),UiLanguage=UiText.Language,UserCancellationAvailable=false,CommitStarted=model.IsCommitStarted,Runtime=Environment.Version.ToString(),DpiX=dpi.PixelsPerInchX,DpiY=dpi.PixelsPerInchY,PerMonitorV2=AreDpiAwarenessContextsEqual(GetWindowDpiAwarenessContext(new WindowInteropHelper(this).Handle),new IntPtr(-4)),UseLayoutRounding,SnapsToDevicePixels,Width=ActualWidth,Height=ActualHeight,Selected=model.Target,Ready=model.CanSwitch,CurrentRegion=model.CurrentRegion,Status=model.StatusTitle,ScrollHeight=MainScroll.ScrollableHeight,PrimaryActionFullyVisible=reachable,PrimaryActionTop=location.Y,PrimaryActionBottom=location.Y+primary.ActualHeight,ContentHeight=Root.ActualHeight,Versions=builds.Select(x=>x?.Version).ToArray()});
        if(!reachable)throw new InvalidOperationException("主操作未完整显示。");
        double width=Root.ActualWidth+Root.Margin.Left+Root.Margin.Right,height=Root.ActualHeight+Root.Margin.Top+Root.Margin.Bottom;
        var bitmap=new RenderTargetBitmap((int)Math.Ceiling(width*dpi.DpiScaleX),(int)Math.Ceiling(height*dpi.DpiScaleY),dpi.PixelsPerInchX,dpi.PixelsPerInchY,PixelFormats.Pbgra32);bitmap.Render(Root);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var stream=File.Create(Path.ChangeExtension(path,"png"));encoder.Save(stream);
    }
    // Isolated visual regression mode. It changes only this window's presentation state.
    public async Task ExportDesignStates(string directory){
        while(inspecting)await Task.Delay(10);
        await Inspect();
        async Task Snapshot(string name){await Dispatcher.InvokeAsync(()=>{},DispatcherPriority.ApplicationIdle);UpdateLayout();WriteDiagnostics(Path.Combine(directory,name+".json"));}
        await Snapshot("默认");model.IsChina=true;await Snapshot("国服目标");model.IsGlobal=true;
        Width=520;Height=560;await Snapshot("最小窗口");
        model.SetBusy(true);model.SetStage(2);model.Status("正在设置游戏语言","正在设置目标语言并保存原始配置备份。","working");await Snapshot("切换中-状态预览");
        model.SetPhase(SwitchPhase.StartingBattleNet);model.Status("正在打开目标战网","正在核对战网区域。完成后恢复操作。","working");await Snapshot("核对区域-状态预览");model.SetBusy(false);
        model.Status("操作未完成","检测到游戏文件变化或监测中断。需等待更新、安装或修复结束后重新检查。","error");await Snapshot("错误-状态预览");
        Width=740;Height=678;model.Status("已打开外服战网","区域配置已核对；账号登录与游戏启动需在战网中完成。");await Snapshot("完成-状态预览");
        Close();
    }
}
