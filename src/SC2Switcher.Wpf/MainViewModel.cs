using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Sc2Switch2;

namespace Sc2Wpf;

public sealed class MainViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    bool china, busy, ready, inspecting, commitStarted;
    int stage=1;
    string tone="working";
    string region="EU";
    string currentRegion, statusTitle="正在检查安装",statusDetail="读取两个版本的本地安装信息。";
    public MainViewModel(){UiText.Instance.PropertyChanged+=(_,_)=>Refresh();}
    public UiText Text=>UiText.Instance;
    public string UiFontFamily=>UiText.Language=="zh-CN"?"Microsoft YaHei UI":"Segoe UI";
    public string UiLanguage {get=>UiText.Language;set {if(!busy)UiText.SetLanguage(value);}}
    public string CnStatus { get; private set; }="等待检查";
    public string GlobalStatus { get; private set; }="等待检查";
    public string CnDot { get; private set; }="#929BA7";
    public string GlobalDot { get; private set; }="#929BA7";
    public string CurrentRegion=>UiText.T(currentRegion??"战网区域待确认");
    public string CurrentLoginRegion {get;private set;}
    public string StatusTitle=>UiText.T(statusTitle);
    public string StatusDetail=>UiText.T(statusDetail);
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
    public bool CanEditRegion=>!busy&&!china;
    public bool CanSwitch=>!busy&&!inspecting&&ready;
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
    public string LanguageTitle=>china?"游戏语言：简体中文":"游戏语言：English";
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
    public void Status(string title,string detail,string tone="ready"){
        this.tone=tone;
        statusTitle=title;statusDetail=detail;
        StatusColor=tone=="error"?"#975019":tone=="working"?"#63635E":"#247456";
        StatusBackground=tone=="error"?"#FFF2E7":tone=="working"?"#F1F0EF":"#EAF5EF";
        StatusSymbol=tone=="error"?"!":tone=="working"?"·":"✓";Refresh();
    }
    void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));
}
