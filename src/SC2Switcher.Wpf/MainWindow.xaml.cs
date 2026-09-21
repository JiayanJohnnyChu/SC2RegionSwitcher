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
    Button sheetCloseButton;
    bool exportingDesignStates;
    IInputElement previousFocus;
    bool inspecting;
    SwitchPhase currentPhase;
    readonly string preferencesPath;
    public MainWindow(ConfigurationLoadResult loaded,ConfigurationStore configuration,string state,string report) {
        this.initialConfiguration=loaded;this.settings=loaded.Settings;this.configuration=configuration;this.state=state;this.report=report;
        preferencesPath=Path.Combine(state,"ui-preferences.json");
        platform=new NativePlatform(settings.BattleNetPath);
        InitializeComponent();Content=null;host.Children.Add(Root);Content=host;DataContext=model;model.Initialize(platform.ReadRegion(),settings.GlobalRegion);AdaptLayout();
        Loaded+=async(_,_)=>{try{await Inspect();if(initialConfiguration.NeedsSetup&&report==null)OpenSettings();if(report!=null)await Dispatcher.InvokeAsync(()=>WriteDiagnostics(report),DispatcherPriority.ApplicationIdle);}catch(Exception error){ShowError(error);}};
        SizeChanged+=(_,_)=>AdaptLayout();
        UiText.Instance.PropertyChanged+=LanguageChanged;
        Closed+=(_,_)=>UiText.Instance.PropertyChanged-=LanguageChanged;
        Closing+=(_,e)=>{if(!model.IsIdle||settingsPage?.IsSaving==true){e.Cancel=true;model.Status("切换尚未结束","请等待切换结束后再关闭窗口。","working");}else if(settingsPage!=null&&!settingsPage.CanLeave())e.Cancel=true;};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape&&sheet!=null){CloseSheet();e.Handled=true;}if(e.Key==Key.F12&&report!=null){WriteDiagnostics(Path.Combine(Path.GetDirectoryName(report),(settingsPage!=null?"settings":sheet!=null?"reference":model.Target)+(ActualWidth<620?"-narrow":"")+".json"));e.Handled=true;}};
    }
    void AdaptLayout(){
        bool compact=(ActualWidth>0?ActualWidth:Width)<620;
        double margin=compact?22:32;
        Resources["PageHeaderPadding"]=new Thickness(margin,19,margin,19);
        Resources["PageBodyMargin"]=new Thickness(margin,20,margin,8);
        Resources["PageFooterPadding"]=new Thickness(margin,15,margin,20);
        Resources["PageTitleSize"]=compact?26.0:30.0;
        Header.Padding=new Thickness(margin,compact?12:16,margin,compact?12:16);
        BrandDescriptor.Visibility=compact?Visibility.Collapsed:Visibility.Visible;
        Workspace.Margin=new Thickness(margin,compact?18:26,margin,compact?16:26);
        Intro.Margin=new Thickness(0,0,0,compact?18:20);
        HeroTitle.FontSize=compact?26:30;
        ChinaCard.Padding=GlobalCard.Padding=new Thickness(compact?18:22,compact?12:18,compact?18:22,compact?12:18);
        ChinaCard.MinHeight=GlobalCard.MinHeight=compact?110:126;
        RegionRow.Margin=new Thickness(0,compact?16:22,0,0);
        DockContent.Margin=new Thickness(margin,compact?14:19,margin,compact?16:22);
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
        if(exportingDesignStates||!model.CanSwitch||inspecting)return;
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
        if(exportingDesignStates)throw new InvalidOperationException("Design snapshots cannot save installation settings.");
        var saved=await Task.Run(()=>configuration.Save(candidate));
        settings=saved;platform=new NativePlatform(settings.BattleNetPath);initialConfiguration.NeedsSetup=false;
        await Inspect();return ConfigurationValidator.Clone(settings);
    }
    void ShowSheet(string title,string intro,List<(string,string)> sections){
        if(sheet!=null)return;
        previousFocus=Keyboard.FocusedElement;Root.IsEnabled=false;Root.Visibility=Visibility.Hidden;
        sheet=new Border{Background=Background};
        KeyboardNavigation.SetTabNavigation(sheet,KeyboardNavigationMode.Cycle);
        var grid=new Grid();grid.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});grid.RowDefinitions.Add(new RowDefinition{Height=new GridLength(1,GridUnitType.Star)});grid.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});
        var heading=new Grid();heading.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});heading.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
        var pageTitle=new TextBlock{Text=UiText.T(title),Style=(Style)FindResource("PageTitle")};pageTitle.SetResourceReference(TextBlock.FontSizeProperty,"PageTitleSize");heading.Children.Add(pageTitle);
        var category=new TextBlock{Text=UiText.T("参考信息"),Style=(Style)FindResource("Eyebrow"),VerticalAlignment=VerticalAlignment.Center,Margin=new Thickness(20,0,0,0)};Grid.SetColumn(category,1);heading.Children.Add(category);
        var sheetHeader=new Border{Child=heading,BorderBrush=(Brush)FindResource("Line"),BorderThickness=new Thickness(0,0,0,1)};sheetHeader.SetResourceReference(Border.PaddingProperty,"PageHeaderPadding");grid.Children.Add(sheetHeader);
        var stack=new StackPanel();stack.SetResourceReference(FrameworkElement.MarginProperty,"PageBodyMargin");stack.Children.Add(new TextBlock{Text=UiText.T(intro),Foreground=(Brush)FindResource("Muted"),FontSize=13,Margin=new Thickness(0,0,0,24)});
        foreach(var section in sections){
            var group=new StackPanel();group.Children.Add(new TextBlock{Text=UiText.T(section.Item1),FontSize=13,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,7)});
            group.Children.Add(new TextBox{Text=UiText.T(section.Item2),IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Background=Brushes.Transparent,BorderThickness=new Thickness(0),Padding=new Thickness(0),Foreground=(Brush)FindResource("Muted"),FontSize=13,VerticalScrollBarVisibility=ScrollBarVisibility.Disabled});
            stack.Children.Add(new Border{Child=group,BorderBrush=(Brush)FindResource("Line"),BorderThickness=new Thickness(0,0,0,1),Padding=new Thickness(0,0,0,18),Margin=new Thickness(0,0,0,18)});
        }
        var scroll=new ScrollViewer{Content=stack,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled};Grid.SetRow(scroll,1);grid.Children.Add(scroll);
        sheetCloseButton=new Button{Content=UiText.T("返回切换"),Style=(Style)FindResource("QuietButton"),HorizontalAlignment=HorizontalAlignment.Left,MinWidth=70,Margin=new Thickness(-10,0,0,0)};sheetCloseButton.Click+=(_,_)=>CloseSheet();
        var footer=new Border{Child=sheetCloseButton,BorderBrush=(Brush)FindResource("Line"),BorderThickness=new Thickness(0,1,0,0)};footer.SetResourceReference(Border.PaddingProperty,"PageFooterPadding");Grid.SetRow(footer,2);grid.Children.Add(footer);sheet.Child=grid;host.Children.Add(sheet);sheetCloseButton.Focus();
    }
    void CloseSheet(){if(sheet==null)return;if(settingsPage!=null&&!settingsPage.CanLeave())return;host.Children.Remove(sheet);sheet=null;settingsPage=null;sheetCloseButton=null;Root.Visibility=Visibility.Visible;Root.IsEnabled=true;if(previousFocus!=null)Keyboard.Focus(previousFocus);}
}
