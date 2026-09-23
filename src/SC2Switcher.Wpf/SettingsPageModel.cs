using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Sc2Switch2;

namespace Sc2Wpf;

public sealed class SettingsPageModel:INotifyPropertyChanged {
    Settings baseline;
    string battle,china,global,variables,textLocale,speechLocale,messageKey,detail,errorField,notice;
    string detectionKey="尚未检测外服语言。",detectionError;
    bool saving,error,detecting;
    int detectionGeneration;
    int installationGeneration;
    string globalVersion,chinaVersion,checkedGlobal,checkedChina;
    string globalCheckError,chinaCheckError;
    readonly Func<string,Task<IReadOnlyList<string>>> discover;
    readonly Func<Profile,bool,BuildInfo> inspect;
    public event PropertyChangedEventHandler PropertyChanged;
    public SettingsPageModel(Settings settings,string notice,Func<string,Task<IReadOnlyList<string>>> discover=null,Func<Profile,bool,BuildInfo> inspect=null){
        this.notice=notice;
        this.discover=discover??(path=>Task.Run(()=>ConfigurationValidator.DiscoverGlobalLanguages(path)));
        this.inspect=inspect??BuildInfo.Load;
        Accept(settings);
    }
    public UiText Text=>UiText.Instance;
    public IReadOnlyList<InterfaceLanguage> InterfaceLanguages=>UiTypography.Languages;
    public System.Windows.Media.FontFamily DisplayFontFamily=>UiTypography.DisplayForLanguage(UiText.Language);
    public string GlobalVersion=>global==checkedGlobal?globalVersion??"—":"—";
    public string ChinaVersion=>china==checkedChina?chinaVersion??"—":"—";
    public string GlobalCheckLabel=>CheckLabel(global,checkedGlobal,globalCheckError);
    public string ChinaCheckLabel=>CheckLabel(china,checkedChina,chinaCheckError);
    static string CheckLabel(string path,string inspected,string error)=>UiText.T(path!=inspected?"web.pendingCheck":error!=null?"安装检查未通过":"web.checked");
    public string GlobalCheckColor=>global==checkedGlobal?(globalCheckError==null?"#315B45":"#962B23"):"#555B51";
    public string ChinaCheckColor=>china==checkedChina?(chinaCheckError==null?"#315B45":"#962B23"):"#555B51";
    public string GlobalCheckNote=>global!=checkedGlobal?UiText.T("web.draftPathHint"):globalCheckError??"";
    public string ChinaCheckNote=>china!=checkedChina?UiText.T("web.draftPathHint"):chinaCheckError??"";
    public string MixedLanguageSummary=>textLocale!=speechLocale?GameLanguageSummary:"";
    public string GlobalInstallationStatus=>InstallationStatus(global,checkedGlobal,globalVersion,globalCheckError);
    public string ChinaInstallationStatus=>InstallationStatus(china,checkedChina,chinaVersion,chinaCheckError);
    static string InstallationStatus(string path,string inspected,string version,string error)=>path!=inspected?UiText.T("web.pendingCheck"):
        error!=null?error:UiText.T("web.version")+" "+version+" · "+UiText.T("web.checked");
    public async Task CheckInstallationsAsync(){
        int generation=++installationGeneration;
        var snapshot=ConfigurationValidator.Clone(baseline);
        snapshot.Global.GamePath=global;snapshot.CN.GamePath=china;
        snapshot.Global.TextLocale=textLocale;snapshot.Global.SpeechLocale=speechLocale;
        (string Version,string Error) Read(Profile profile,bool cn){
            try{return(inspect(profile,cn).Version,null);}
            catch(Exception error){return(null,error.Message);}
        }
        var result=await Task.Run(()=>(Global:Read(snapshot.Global,false),China:Read(snapshot.CN,true)));
        if(generation!=installationGeneration)return;
        checkedGlobal=snapshot.Global.GamePath;checkedChina=snapshot.CN.GamePath;
        globalVersion=result.Global.Version;globalCheckError=result.Global.Error;
        chinaVersion=result.China.Version;chinaCheckError=result.China.Error;Refresh();
    }
    public string UiLanguage{get=>UiText.Language;set{if(!saving&&value!=null)UiText.SetLanguage(value);}}
    public string BattleNetDirectory{get=>battle;set{if(saving)return;battle=value;Edited();}}
    public string ChinaDirectory{get=>china;set{if(saving)return;china=value;Edited();}}
    public string GlobalDirectory{get=>global;set{if(saving||global==value)return;global=value;InvalidateLanguages();Edited();}}
    public string VariablesFile{get=>variables;set{if(saving)return;variables=value;Edited();}}
    public IReadOnlyList<GameLanguageOption> AvailableLanguages {get;private set;}=Array.Empty<GameLanguageOption>();
    public string GlobalLanguage {
        get=>textLocale==speechLocale&&AvailableLanguages.Any(x=>x.Code==textLocale)?textLocale:null;
        set{if(!CanSelectLanguage||value==null||!AvailableLanguages.Any(x=>x.Code==value))return;textLocale=speechLocale=value;Edited();}
    }
    public string GameLanguageSummary=>UiText.Format("语言设置：{0}",GameLocales.Describe(textLocale,speechLocale));
    public string DetectionMessage=>detecting?UiText.T("正在检测外服语言…"):
        detectionError!=null?UiText.Format("无法检测外服语言：{0}",detectionError):
        AvailableLanguages.Count>0?(textLocale!=speechLocale?UiText.T("保留现有文字与语音组合；选择语言将同时替换两者。"):
        GlobalLanguage==null?UiText.T("原语言不在可选列表中。请选择已安装语言，或在战网中补充资源。"):
        UiText.Format("检测到 {0} 种文字与语音齐全的语言。",AvailableLanguages.Count)):UiText.T(detectionKey);
    public bool IsDetecting=>detecting;
    public bool CanSelectLanguage=>!saving&&!detecting&&AvailableLanguages.Count>0;
    public bool CanDetectLanguage=>!saving&&!detecting;
    public bool CanSave=>!saving&&!detecting;
    static string Parent(string path){try{return String.IsNullOrWhiteSpace(path)?"":Path.GetDirectoryName(path);}catch{return "";}}
    public bool IsSaving=>saving;
    public bool CanEdit=>!saving;
    public bool HasChanges=>battle!=Parent(baseline.BattleNetPath)||china!=baseline.CN.GamePath||global!=baseline.Global.GamePath||variables!=baseline.VariablesPath||textLocale!=baseline.Global.TextLocale||speechLocale!=baseline.Global.SpeechLocale;
    public Visibility DiscardVisibility=>HasChanges?Visibility.Visible:Visibility.Collapsed;
    public string SaveLabel=>UiText.T(saving?"正在保存…":"保存并检查");
    public string Notice=>UiText.T(notice);
    public Visibility NoticeVisibility=>String.IsNullOrEmpty(notice)?Visibility.Collapsed:Visibility.Visible;
    public string Message=>String.IsNullOrEmpty(errorField)?UiText.T(messageKey):UiText.T(FieldName(errorField))+": "+UiText.T(detail);
    public string MessageColor=>error?"#975019":saving?"#63635E":"#247456";
    public Visibility MessageVisibility=>String.IsNullOrEmpty(Message)?Visibility.Collapsed:Visibility.Visible;
    public static string FieldName(string name)=>name switch{"BattleNetDirectory"=>"战网目录","ChinaDirectory"=>"国服游戏目录","GlobalDirectory"=>"外服游戏目录","VariablesFile"=>"共享游戏设置文件","GlobalLanguage"=>"外服游戏语言（实验性）",_=>"安装配置"};
    public Settings Candidate(){
        var candidate=ConfigurationValidator.FromDirectories(baseline,battle,china,global,variables);
        candidate.Global.TextLocale=textLocale;candidate.Global.SpeechLocale=speechLocale;
        return candidate;
    }
    public void SetSaving(bool value){saving=value;Refresh();}
    public void Status(string key,bool isError=false){messageKey=key;detail=null;errorField=null;error=isError;Refresh();}
    public void Failure(Exception exception){errorField=(exception as SettingsValidationException)?.Field??"Configuration";detail=UiText.MessageKey(exception.Message);messageKey=null;error=true;Refresh();}
    public void Accept(Settings settings){
        bool changedPath=global!=settings.Global.GamePath;
        baseline=ConfigurationValidator.Clone(settings);battle=Parent(settings.BattleNetPath);china=settings.CN.GamePath;global=settings.Global.GamePath;variables=settings.VariablesPath;
        textLocale=settings.Global.TextLocale;speechLocale=settings.Global.SpeechLocale;
        if(changedPath)InvalidateLanguages();Refresh();
    }
    public void Reset(){Accept(baseline);Status(null);}
    public void ClearNotice(){notice=null;Refresh();}
    void Edited(){Status(null);}
    void InvalidateLanguages(){detectionGeneration++;detecting=false;AvailableLanguages=Array.Empty<GameLanguageOption>();detectionError=null;detectionKey="尚未检测外服语言。";}
    public void CancelDetection(){InvalidateLanguages();Refresh();}
    public async Task DetectLanguagesAsync(){
        if(saving)return;
        int generation=++detectionGeneration;
        string path=global;
        detecting=true;detectionError=null;AvailableLanguages=Array.Empty<GameLanguageOption>();Refresh();
        try {
            var languages=await discover(path);
            if(generation!=detectionGeneration||path!=global)return;
            AvailableLanguages=languages.Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).Select(x=>new GameLanguageOption(x)).ToArray();
            detectionKey="安装清单中没有文字与语音齐全的语言。请在战网中安装所需资源。";
        }catch(Exception exception){if(generation==detectionGeneration&&path==global)detectionError=exception.Message;}
        finally{if(generation==detectionGeneration){detecting=false;Refresh();}}
    }
    public void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));
}
