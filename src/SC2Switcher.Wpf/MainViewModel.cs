using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Sc2Switch2;

namespace Sc2Wpf;

public sealed class MainViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    bool china, busy, ready, inspecting, commitStarted, settingsBusy;
    AppliedConfiguration applied = new();
    string page="home";
    int stage=1;
    string tone="working";
    string region="EU";
    string globalTextLocale="enUS",globalSpeechLocale="enUS";
    string currentRegion, statusTitle="正在检查安装",statusDetail="读取两个版本的本地安装信息。";
    string statusDetailField;
    public MainViewModel(){UiText.Instance.PropertyChanged+=(_,_)=>Refresh();}
    public UiText Text=>UiText.Instance;
    public System.Windows.Media.FontFamily UiFontFamily=>UiTypography.ForLanguage(UiText.Language);
    public System.Windows.Media.FontFamily DisplayFontFamily=>UiTypography.DisplayForLanguage(UiText.Language);
    public System.Windows.Media.FontFamily DestinationFontFamily=>UiText.Language=="zh-CN"?UiFontFamily:DisplayFontFamily;
    public FontWeight DestinationWeight=>FontWeights.SemiBold;
    public double DestinationLineHeight=>Double.NaN;
    public string EditionLabel=>UiText.T("web.desktopEdition")+" / "+typeof(MainViewModel).Assembly.GetName().Version.ToString(3);
    public System.Windows.Markup.XmlLanguage TextLanguage=>UiTypography.CurrentLanguage;
    public System.Collections.Generic.IReadOnlyList<InterfaceLanguage> InterfaceLanguages=>UiTypography.Languages;
    public string UiLanguage {get=>UiText.Language;set {if(!busy&&!settingsBusy&&value!=null)UiText.SetLanguage(value);}}
    public bool CanNavigate=>!busy&&!inspecting&&!settingsBusy;
    public bool HomeActive=>page=="home";
    public bool SettingsActive=>page=="settings";
    public bool GuideActive=>page=="guide";
    public void SetPage(string value){page=value;Refresh();}
    public void SetSettingsBusy(bool value){settingsBusy=value;Refresh();}
    public string CnStatus { get; private set; }="等待检查";
    public string GlobalStatus { get; private set; }="等待检查";
    public string ChinaAvailability=>UiText.T(CnStatus=="安装检查通过"?"web.available":CnStatus=="等待检查"?"web.pendingCheck":"web.unavailable");
    public string GlobalAvailability=>UiText.T(GlobalStatus=="安装检查通过"?"web.available":GlobalStatus=="等待检查"?"web.pendingCheck":"web.unavailable");
    public bool ChinaAvailable=>CnStatus=="安装检查通过";
    public bool GlobalAvailable=>GlobalStatus=="安装检查通过";
    public Visibility RepairVisibility=>!ChinaAvailable||!GlobalAvailable?Visibility.Visible:Visibility.Collapsed;
    public Thickness ErrorRule=>tone=="error"?new Thickness(3,0,0,0):new Thickness(0);
    public string ErrorSurface=>tone=="error"?"#FFF1E9":"Transparent";
    public Thickness ErrorPadding=>tone=="error"?new Thickness(12,8,12,8):new Thickness(0);
    public string StatusTitleColor=>tone=="error"?"#962B23":"#181A18";
    public Visibility StatusDetailVisibility=>String.IsNullOrEmpty(statusDetail)?Visibility.Collapsed:Visibility.Visible;
    public string CnDot { get; private set; }="#929BA7";
    public string GlobalDot { get; private set; }="#929BA7";
    public string CurrentRegion=>UiText.T(currentRegion??"战网区域待确认");
    public string CurrentLoginRegion {get;private set;}
    public string StatusTitle=>UiText.T(statusTitle);
    public string StatusDetail=>String.IsNullOrEmpty(statusDetailField)?UiText.T(statusDetail):UiText.T(SettingsPageModel.FieldName(statusDetailField))+": "+UiText.T(statusDetail);
    public string StatusColor { get; private set; }="#63635E";
    public string StatusBackground { get; private set; }="#F1F0EF";
    public string StatusSymbol { get; private set; }="·";
    public bool IsChina {get=>china;set {if(value&&!busy){china=true;Refresh();}}}
    public bool IsGlobal {get=>!china;set {if(value&&!busy){china=false;Refresh();}}}
    public string Target=>china?"CN":"Global";
    public string Region=>region;
    public bool Europe {get=>region=="EU";set {if(value&&!busy){region="EU";Refresh();}}}
    public bool Americas {get=>region=="US";set {if(value&&!busy){region="US";Refresh();}}}
    public bool Asia {get=>region=="KR";set {if(value&&!busy){region="KR";Refresh();}}}
    public bool IsIdle=>!busy;
    public bool CanInspect=>!busy&&!inspecting;
    public bool CanEditRegion=>!busy&&!inspecting&&!china;
    public bool ConfigurationChanged=>!applied.Matches(CurrentLoginRegion,china?"CN":region,china?"zhCN":globalTextLocale,china?"zhCN":globalSpeechLocale);
    public bool CanSwitch=>!busy&&!inspecting&&ready&&ConfigurationChanged;
    public string CurrentCode=>CurrentLoginRegion??"—";
    public string CurrentLanguageCode=>applied.IsKnown?(applied.TextLocale==applied.SpeechLocale?applied.TextLocale:applied.TextLocale+" / "+applied.SpeechLocale):"—";
    public string TargetLanguage=>china?GameLocales.DisplayName("zhCN"):GlobalGameLanguage;
    public string TargetConfiguration=>UiText.Format("web.targetConfiguration",UiText.T(china?"国服":"外服"));
    public Visibility GlobalVisibility=>china?Visibility.Collapsed:Visibility.Visible;
    public Visibility ChinaVisibility=>china?Visibility.Visible:Visibility.Collapsed;
    public string ApplyLabel=>UiText.T(ConfigurationChanged?"web.apply":"web.reapply");
    public string ChangeSummary {
        get {
            if(applied.NeedsRecovery)return UiText.T("上次切换尚未恢复，已停止新的语言修改。");
            if(!ConfigurationChanged)return UiText.T("web.noChange");
            string targetRegion=china?"CN":region,text=china?"zhCN":globalTextLocale,speech=china?"zhCN":globalSpeechLocale;
            string targetLanguage=text==speech?text:text+" / "+speech;
            if(!applied.IsKnown)return UiText.Format("web.configurationChange",CurrentCode,CurrentLanguageCode,targetRegion,targetLanguage);
            if(CurrentLoginRegion==targetRegion)return UiText.Format("web.languageChange",CurrentLanguageCode,targetLanguage);
            if(applied.TextLocale==text&&applied.SpeechLocale==speech)return UiText.Format("web.regionChange",CurrentCode,targetRegion);
            return UiText.Format("web.configurationChange",CurrentCode,CurrentLanguageCode,targetRegion,targetLanguage);
        }
    }
    public void SetApplied(AppliedConfiguration value){applied=value;Refresh();}
    public bool IsCommitStarted=>commitStarted;
    public string BusyLabel=>UiText.T("正在切换");
    public Visibility SwitchVisibility=>busy?Visibility.Collapsed:Visibility.Visible;
    public Visibility BusyVisibility=>busy?Visibility.Visible:Visibility.Collapsed;
    public Visibility ProgressVisibility=>busy?Visibility.Visible:Visibility.Collapsed;
    public Visibility ErrorVisibility=>tone=="error"&&!busy?Visibility.Visible:Visibility.Collapsed;
    public string StepLabel=>UiText.Format("阶段 {0} / 3",stage);
    public string StepOneColor=>"#63635E";
    public string StepTwoColor=>stage>=2?"#63635E":"#DAD9D6";
    public string StepThreeColor=>stage>=3?"#63635E":"#DAD9D6";
    public string ActionLabel=>UiText.T(china?"切换到国服":"切换到外服");
    public string GlobalGameLanguage=>GameLocales.Describe(globalTextLocale,globalSpeechLocale);
    public string GlobalTargetName=>UiText.Format("切换目标：外服，{0}",GlobalGameLanguage);
    public string LanguageTitle=>UiText.Format("游戏语言：{0}",china?GameLocales.DisplayName("zhCN"):GlobalGameLanguage);
    public void SetGameLanguages(Profile global){globalTextLocale=global.TextLocale;globalSpeechLocale=global.SpeechLocale;Refresh();}
    public void Initialize(string loginRegion,string preferredRegion){region=preferredRegion;china=loginRegion=="CN";SetCurrent(loginRegion);Refresh();}
    public void SetCurrent(string value){CurrentLoginRegion=value;currentRegion=value switch {"CN"=>"国服 · 中国", "EU"=>"外服 · 欧洲", "US"=>"外服 · 美洲", "KR"=>"外服 · 亚洲", _=>"暂未识别"};Refresh();}
    public void SetInstallation(bool cn,bool ok){if(cn){CnStatus=ok?"安装检查通过":"需要检查安装";CnDot=ok?"#247456":"#AA5A18";}else{GlobalStatus=ok?"安装检查通过":"需要检查安装";GlobalDot=ok?"#247456":"#AA5A18";}Refresh();}
    public void SetReady(bool value){ready=value;Refresh();}
    public void SetBusy(bool value){busy=value;commitStarted=false;if(value)stage=1;Refresh();}
    public void SetPhase(SwitchPhase phase){
        commitStarted=phase==SwitchPhase.StartingBattleNet||phase==SwitchPhase.Completed;
        stage=commitStarted?3:phase==SwitchPhase.Checking?1:2;
        Refresh();
    }
    public void SetInspecting(bool value){inspecting=value;Refresh();}
    public void SetStage(int value){stage=Math.Clamp(value,1,3);Refresh();}
    public void Status(string title,string detail,string tone="ready",string field=null){
        this.tone=tone;
        statusTitle=title;statusDetail=detail;statusDetailField=field;
        StatusColor=tone=="error"?"#962B23":tone=="working"?"#181A18":"#315B45";
        StatusBackground=tone=="error"?"#FFF2E7":tone=="working"?"#F1F0EF":"#EAF5EF";
        StatusSymbol=tone=="error"?"!":tone=="working"?"·":"✓";Refresh();
    }
    void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));
}
