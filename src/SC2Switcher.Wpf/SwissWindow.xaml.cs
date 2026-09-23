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
public partial class SwissWindow : Window {
    Settings settings;
    readonly ConfigurationStore configuration;
    readonly ConfigurationLoadResult initialConfiguration;
    SwissSettingsView settingsPage;
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
    string repairField;
    SwitchPhase currentPhase;
    readonly string preferencesPath;
    public SwissWindow(ConfigurationLoadResult loaded,ConfigurationStore configuration,string state,string report) {
        this.initialConfiguration=loaded;this.settings=loaded.Settings;this.configuration=configuration;this.state=state;this.report=report;
        preferencesPath=Path.Combine(state,"ui-preferences.json");
        platform=new NativePlatform(settings.BattleNetPath);
        InitializeComponent();PageHost.Children.Remove(Root);host.Children.Add(Root);PageHost.Children.Add(host);DataContext=model;model.Initialize(platform.ReadRegion(),settings.GlobalRegion);model.SetGameLanguages(settings.Global);AdaptLayout();
        model.PropertyChanged+=(_,_)=>AdaptStatusLayout();
        Loaded+=async(_,_)=>{try{await Inspect();if(initialConfiguration.NeedsSetup&&report==null)OpenSettings();if(report!=null)await Dispatcher.InvokeAsync(()=>WriteDiagnostics(report),DispatcherPriority.ApplicationIdle);}catch(Exception error){ShowError(error);}};
        SizeChanged+=(_,_)=>AdaptLayout();
        UiText.Instance.PropertyChanged+=LanguageChanged;
        Closed+=(_,_)=>UiText.Instance.PropertyChanged-=LanguageChanged;
        Closing+=(_,e)=>{if(!model.IsIdle||settingsPage?.IsSaving==true){e.Cancel=true;model.Status("切换尚未结束","请等待切换结束后再关闭窗口。","working");}else if(settingsPage!=null&&!settingsPage.CanLeave())e.Cancel=true;};
        PreviewKeyDown+=(_,e)=>{if(e.Key==Key.Escape&&sheet!=null){CloseSheet();e.Handled=true;}if(e.Key==Key.F12&&report!=null){WriteDiagnostics(Path.Combine(Path.GetDirectoryName(report),(settingsPage!=null?"settings":sheet!=null?"reference":model.Target)+(ActualWidth<620?"-narrow":"")+".json"));e.Handled=true;}};
    }
    void AdaptLayout(){
        double width=Shell.ActualWidth>0?Shell.ActualWidth:Width-16;
        double height=Shell.ActualHeight>0?Shell.ActualHeight:Height-40;
        bool tiny=width<=420,narrow=width<=600,compact=width<=900||height<=700,rows=width<=700;
        double gutter=tiny?14:narrow?18:compact?24:40,gap=tiny?0:narrow?18:compact?24:36;
        Header.Padding=new Thickness(gutter,6,gutter,6);Header.MinHeight=narrow?48:compact?52:60;
        HeaderLanguage.Width=narrow?114:132;HeaderLanguage.MinHeight=narrow?32:34;
        BrandSC2.FontSize=narrow?22:26;BrandDescriptor.FontSize=tiny?11:narrow?12:14;
        Navigation.Margin=new Thickness(gutter,0,gutter,0);Navigation.Width=Math.Max(0,Math.Min(760,width-gutter*2));Navigation.HorizontalAlignment=HorizontalAlignment.Left;
        foreach(RadioButton item in Navigation.Children){item.MinHeight=compact?40:44;item.FontSize=narrow?12:13;}
        Workspace.Margin=new Thickness(gutter,narrow?(height>=740?20:12):compact?16:24,gutter,compact?10:14);
        Intro.Margin=new Thickness(0,0,0,narrow?(height>=740?20:8):compact?14:20);
        Intro.ColumnDefinitions[1].Width=new GridLength(tiny?0:narrow?18:compact?24:32);
        Intro.ColumnDefinitions[2].MinWidth=narrow?0:compact?198:220;
        Intro.ColumnDefinitions[2].Width=tiny?new GridLength(0):new GridLength(narrow?1:compact?.7:.58,GridUnitType.Star);
        Grid.SetRow(CurrentMarker,tiny?1:0);Grid.SetColumn(CurrentMarker,tiny?0:2);Grid.SetColumnSpan(CurrentMarker,tiny?3:1);
        CurrentMarker.BorderThickness=tiny?new Thickness(0):new Thickness(1,0,0,0);CurrentMarker.Margin=new Thickness(0,tiny?12:0,0,0);
        CurrentMarker.Padding=new Thickness(narrow?12:compact?14:18,3,0,0);CurrentCode.FontSize=compact?18:21;
        if(tiny)CurrentMarker.Padding=new Thickness(0);
        HeroTitle.FontSize=tiny?27:narrow?26:compact?30:36;
        Destinations.ColumnDefinitions[1].Width=rows?new GridLength(0):new GridLength(1,GridUnitType.Star);
        Grid.SetColumn(ChinaCard,rows?0:1);Grid.SetRow(ChinaCard,rows?1:0);
        ChinaCard.BorderThickness=rows?new Thickness(0,1,0,0):new Thickness(1,0,0,0);
        foreach(var card in new[]{ChinaCard,GlobalCard}){
            card.IsCompact=rows;card.MinHeight=rows?84:compact?104:144;
            card.Padding=rows?new Thickness(16,12,16,12):compact?new Thickness(16,10,16,10):new Thickness(22,15,22,15);
            card.LabelSize=rows?Math.Clamp(width*.056,25,32):compact?38:52;
        }
        ConfigurationPanel.Padding=new Thickness(0,narrow?9:compact?10:13,0,narrow?10:compact?12:16);
        ConfigurationTitle.FontSize=narrow?13:14;ConfigurationTitle.Margin=new Thickness(0,0,0,narrow?7:compact?8:10);
        RegionRow.ColumnDefinitions[0].Width=new GridLength(narrow&&!tiny?1.45:1,GridUnitType.Star);RegionRow.ColumnDefinitions[1].Width=new GridLength(gap);
        RegionRow.ColumnDefinitions[2].Width=tiny?new GridLength(0):new GridLength(1,GridUnitType.Star);
        Grid.SetRow(GameLanguagePanel,tiny?1:0);Grid.SetColumn(GameLanguagePanel,tiny?0:2);GameLanguagePanel.Margin=new Thickness(0,tiny?12:0,0,0);
        foreach(var regionContent in new[]{RegionEUContent,RegionUSContent,RegionKRContent}){
            regionContent.Orientation=narrow&&!tiny?Orientation.Vertical:Orientation.Horizontal;
            ((TextBlock)regionContent.Children[1]).Margin=new Thickness(narrow&&!tiny?0:8,0,0,0);
        }
        GameLanguageValue.FontSize=narrow?13:14;
        Readiness.Margin=new Thickness(0,narrow?(height>=740?20:8):compact?14:20,0,0);
        ActionDock.Padding=new Thickness(gutter,narrow?12:compact?14:20,gutter,narrow?12:compact?14:20);ActionDock.MinHeight=compact?90:96;
        DockContent.ColumnDefinitions[1].Width=new GridLength(gap);SwitchButton.FontSize=compact?14:15;SwitchButton.MinHeight=narrow?52:compact?50:56;
        DockContent.ColumnDefinitions[2].Width=tiny?new GridLength(0):new GridLength(1,GridUnitType.Star);
        foreach(FrameworkElement action in new FrameworkElement[]{SwitchButton,BusyIndicator}){Grid.SetRow(action,tiny?1:0);Grid.SetColumn(action,tiny?0:2);action.Margin=new Thickness(0,tiny?12:0,0,0);}
        ChangeSummary.FontSize=compact?15:16;RestartNote.FontSize=compact?12:13;
        Colophon.Padding=new Thickness(gutter,0,gutter,0);Colophon.MinHeight=compact?32:36;
        AdaptStatusLayout();
    }
    void AdaptStatusLayout(){
        double width=Shell.ActualWidth>0?Shell.ActualWidth:Width-16;
        bool error=model.ErrorVisibility==Visibility.Visible,stacked=error&&width<=1000,phone=error&&width<=480;
        bool repair=model.RepairVisibility==Visibility.Visible;
        // These are the S-06 CSS grid breakpoints, not a substitute alert layout.
        StatusLayout.ColumnDefinitions[0].Width=new GridLength(1,GridUnitType.Star);
        StatusLayout.ColumnDefinitions[1].Width=new GridLength(stacked?0:error?36:18);
        StatusLayout.ColumnDefinitions[2].Width=stacked?new GridLength(0):error?new GridLength(1,GridUnitType.Star):GridLength.Auto;
        Grid.SetColumnSpan(StatusMessage,stacked?3:1);
        Grid.SetRow(RecoveryActions,stacked?1:0);Grid.SetColumn(RecoveryActions,stacked?0:2);Grid.SetColumnSpan(RecoveryActions,stacked?3:1);
        RecoveryActions.Margin=stacked?new Thickness(30,12,0,0):new Thickness(0);
        RecoveryActions.HorizontalAlignment=stacked?HorizontalAlignment.Stretch:error?HorizontalAlignment.Left:HorizontalAlignment.Right;
        Readiness.Padding=new Thickness(0,0,0,error?4:0);
        StatusMessage.ColumnDefinitions[0].Width=new GridLength(error?18:13);
        StatusMessage.ColumnDefinitions[1].Width=new GridLength(error?12:7);
        StatusMarker.Width=error?18:13;StatusMarker.Height=error?18:19.5;
        StatusMarker.Margin=new Thickness(0,error?3:1,0,0);StatusMarker.BorderThickness=new Thickness(error?1:0);
        StatusSymbolText.FontSize=error?12:13;
        StatusHeading.FontSize=error?15:13;StatusHeading.FontWeight=error?FontWeights.SemiBold:FontWeights.Normal;
        StatusHeading.LineHeight=error?24:Double.NaN;
        StatusHeading.LineStackingStrategy=error?LineStackingStrategy.BlockLineHeight:LineStackingStrategy.MaxHeight;
        StatusBody.Margin=new Thickness(0,error?5:4,0,0);StatusBody.LineHeight=error?21:Double.NaN;
        StatusBody.LineStackingStrategy=error?LineStackingStrategy.BlockLineHeight:LineStackingStrategy.MaxHeight;
        StatusBody.MaxWidth=error?64*new FormattedText("0",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,
            new Typeface(model.UiFontFamily,FontStyles.Normal,FontWeights.Normal,FontStretches.Normal),13,Brushes.Black,VisualTreeHelper.GetDpi(this).PixelsPerDip).Width:Double.PositiveInfinity;
        foreach(var button in new[]{RepairPathButton,CheckButton,ErrorDetailsButton}){
            button.Style=(Style)FindResource(error?(button==RepairPathButton?"StatusRepairButton":"StatusActionButton"):"QuietButton");
            button.FontSize=error&&width<=600?12:13;button.Margin=new Thickness(0);
            Grid.SetRow(button,0);Grid.SetColumnSpan(button,1);
        }
        if(phone){
            RecoveryActions.ColumnDefinitions[0].Width=new GridLength(1,GridUnitType.Star);
            RecoveryActions.ColumnDefinitions[1].Width=new GridLength(20);
            RecoveryActions.ColumnDefinitions[2].Width=new GridLength(1,GridUnitType.Star);
            RecoveryActions.ColumnDefinitions[3].Width=RecoveryActions.ColumnDefinitions[4].Width=new GridLength(0);
            Grid.SetColumnSpan(RepairPathButton,5);Grid.SetColumn(CheckButton,0);Grid.SetColumn(ErrorDetailsButton,2);
            foreach(var button in new[]{CheckButton,ErrorDetailsButton}){Grid.SetRow(button,repair?1:0);button.Margin=new Thickness(0,repair?4:0,0,0);}
        }else{
            RecoveryActions.ColumnDefinitions[0].Width=RecoveryActions.ColumnDefinitions[2].Width=RecoveryActions.ColumnDefinitions[4].Width=GridLength.Auto;
            double gap=error?(stacked?24:20):18;
            RecoveryActions.ColumnDefinitions[1].Width=new GridLength(repair?gap:0);
            RecoveryActions.ColumnDefinitions[3].Width=new GridLength(error?gap:0);
            Grid.SetColumn(CheckButton,2);Grid.SetColumn(ErrorDetailsButton,4);
        }
    }
    async Task Inspect(){
        if(!model.IsIdle||inspecting)return;
        inspecting=true;model.SetInspecting(true);model.SetReady(false);model.Status("正在检查安装","读取两个版本的安装信息。","working");
        try{
            var snapshot=ConfigurationValidator.Clone(settings);
            var result=await Task.Run(()=>{
                var errors=new List<string>();
                string field=null;
                builds[0]=null;builds[1]=null;
                try {ConfigurationValidator.Validate(snapshot);}
                catch(Exception error){
                    if(error is SettingsValidationException validation)field=validation.Field;
                    errors.Add(UiText.MessageKey(error.Message));
                }
                // Show each installation's actual result even when the other one fails.
                try{builds[0]=BuildInfo.Load(snapshot.CN,true);}catch{ }
                try{builds[1]=BuildInfo.Load(snapshot.Global,false);}catch{ }
                return (Errors:errors,Field:field);
            });
            repairField=result.Field;
            model.SetInstallation(true,builds[0]!=null);model.SetInstallation(false,builds[1]!=null);model.SetCurrent(platform.ReadRegion());ReadAppliedConfiguration();
            if(result.Errors.Count>0){model.Status("安装检查未通过",String.Join("\n",result.Errors),"error",result.Field);return;}
            if(platform.GameRunning())model.Status("检测到游戏正在运行","需退出游戏和地图编辑器后重新检查。","error");
            else {model.SetReady(true);model.Status("web.installationsReady","");}
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
        finally{model.SetBusy(false);model.SetCurrent(platform.ReadRegion());ReadAppliedConfiguration();if(acquired)mutex.ReleaseMutex();}
    }
    void ReadAppliedConfiguration()=>model.SetApplied(AppliedConfiguration.Read(settings.VariablesPath,state));
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
    void ShowError(Exception error){model.Status("操作未完成",UiText.MessageKey(error.Message),"error");try{Json.Write(Path.Combine(state,"last-error.json"),new {Time=DateTime.Now.ToString("o"),Message=error.Message});}catch{}}
    void StatusDetails_Click(object sender,RoutedEventArgs e)=>OpenReference(false);
    void Details_Click(object sender,RoutedEventArgs e)=>OpenSettings();
    void Navigation_Checked(object sender,RoutedEventArgs e){
        // SelectionItem automation and keyboard selection need the same guarded
        // navigation as a click. Model refreshes of the active tab do not navigate.
        switch(((RadioButton)sender).Tag){
            case "home" when !model.HomeActive:Home_Click(sender,e);break;
            case "settings" when !model.SettingsActive:Settings_Click(sender,e);break;
            case "guide" when !model.GuideActive:Help_Click(sender,e);break;
        }
    }
    void Home_Click(object sender,RoutedEventArgs e){CloseSheet();model.SetPage(sheet==null?"home":"settings");}
    void Help_Click(object sender,RoutedEventArgs e){
        if(!model.CanNavigate||model.GuideActive)return;
        CloseSheet();if(sheet!=null){model.SetPage("settings");return;}OpenReference(true);
    }
    void OpenReference(bool guide){
        if(sheet!=null)return;
        previousFocus=Keyboard.FocusedElement;Root.IsEnabled=false;Root.Visibility=Visibility.Hidden;
        var page=new SwissReferenceView(guide,()=>model.StatusTitle,()=>model.StatusDetail,()=>model.GlobalGameLanguage);
        page.BackRequested+=CloseSheet;page.EditRequested+=()=>{CloseSheet();OpenRepairSettings();};
        sheet=new Border{Background=Background,Child=page};host.Children.Add(sheet);
        KeyboardNavigation.SetTabNavigation(sheet,KeyboardNavigationMode.Cycle);
        sheetCloseButton=(Button)page.FindName("BackButton");sheetCloseButton.Focus();model.SetPage(guide?"guide":"home");
    }
    void GameLanguage_Click(object sender,RoutedEventArgs e){OpenSettings();settingsPage?.FocusField("GlobalLanguage");}
    void RepairPath_Click(object sender,RoutedEventArgs e)=>OpenRepairSettings();
    void OpenRepairSettings(){OpenSettings();settingsPage?.FocusField(repairField??(model.GlobalAvailable?"ChinaDirectory":"GlobalDirectory"));}
    void Settings_Click(object sender,RoutedEventArgs e)=>OpenSettings();
    void OpenSettings(){
        if(settingsPage!=null)return;
        if(!model.CanNavigate||inspecting)return;
        CloseSheet();if(sheet!=null)return;model.SetPage("settings");
        previousFocus=Keyboard.FocusedElement;Root.IsEnabled=false;Root.Visibility=Visibility.Hidden;
        sheet=new Border{Background=Background};
        KeyboardNavigation.SetTabNavigation(sheet,KeyboardNavigationMode.Cycle);
        settingsPage=new SwissSettingsView(settings,initialConfiguration.NeedsSetup?initialConfiguration.NoticeKey:null,SaveDirectories,exportingDesignStates?DiscoverDesignLanguages:null,exportingDesignStates?((_,cn)=>builds[cn?0:1]):null);
        settingsPage.CloseRequested+=CloseSheet;
        var editor=(SettingsPageModel)settingsPage.DataContext;
        editor.PropertyChanged+=SettingsStateChanged;
        sheet.Child=settingsPage;host.Children.Add(sheet);
    }
    async Task<Settings> SaveDirectories(Settings candidate){
        if(exportingDesignStates)throw new InvalidOperationException("Design snapshots cannot save installation settings.");
        var saved=await Task.Run(()=>configuration.Save(candidate));
        settings=saved;platform=new NativePlatform(settings.BattleNetPath);initialConfiguration.NeedsSetup=false;model.SetGameLanguages(settings.Global);
        await Inspect();return ConfigurationValidator.Clone(settings);
    }
    void SettingsStateChanged(object sender,System.ComponentModel.PropertyChangedEventArgs e){
        if(ReferenceEquals(settingsPage?.DataContext,sender))model.SetSettingsBusy(((SettingsPageModel)sender).IsSaving);
    }
    void CloseSheet(){
        if(sheet==null)return;
        if(settingsPage!=null&&!settingsPage.CanLeave())return;
        if(settingsPage?.DataContext is SettingsPageModel editor)editor.PropertyChanged-=SettingsStateChanged;
        host.Children.Remove(sheet);sheet=null;settingsPage=null;sheetCloseButton=null;Root.Visibility=Visibility.Visible;Root.IsEnabled=true;
        model.SetSettingsBusy(false);model.SetPage("home");if(previousFocus!=null)Keyboard.Focus(previousFocus);
    }
}
