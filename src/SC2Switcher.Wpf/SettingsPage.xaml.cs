using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sc2Switch2;

namespace Sc2Wpf;

public sealed class SettingsPageModel:INotifyPropertyChanged {
    Settings baseline;
    string battle,china,global,variables,messageKey,detail,errorField,notice;
    bool saving,error;
    public event PropertyChangedEventHandler PropertyChanged;
    public SettingsPageModel(Settings settings,string notice){this.notice=notice;Accept(settings);}
    public UiText Text=>UiText.Instance;
    public string UiLanguage{get=>UiText.Language;set{if(!saving)UiText.SetLanguage(value);}}
    public string BattleNetDirectory{get=>battle;set{battle=value;Edited();}}
    public string ChinaDirectory{get=>china;set{china=value;Edited();}}
    public string GlobalDirectory{get=>global;set{global=value;Edited();}}
    public string VariablesFile{get=>variables;set{variables=value;Edited();}}
    static string Parent(string path){try{return String.IsNullOrWhiteSpace(path)?"":Path.GetDirectoryName(path);}catch{return "";}}
    public bool IsSaving=>saving;
    public bool CanEdit=>!saving;
    public bool HasChanges=>battle!=Parent(baseline.BattleNetPath)||china!=baseline.CN.GamePath||global!=baseline.Global.GamePath||variables!=baseline.VariablesPath;
    public Visibility DiscardVisibility=>HasChanges?Visibility.Visible:Visibility.Collapsed;
    public string SaveLabel=>UiText.T(saving?"正在保存…":"保存并检查");
    public string Notice=>UiText.T(notice);
    public Visibility NoticeVisibility=>String.IsNullOrEmpty(notice)?Visibility.Collapsed:Visibility.Visible;
    public string Message=>String.IsNullOrEmpty(errorField)?UiText.T(messageKey):UiText.T(FieldName(errorField))+": "+detail;
    public string MessageColor=>error?"#975019":saving?"#2855DC":"#247456";
    public Visibility MessageVisibility=>String.IsNullOrEmpty(Message)?Visibility.Collapsed:Visibility.Visible;
    public static string FieldName(string name)=>name switch{"BattleNetDirectory"=>"战网目录","ChinaDirectory"=>"国服游戏目录","GlobalDirectory"=>"外服游戏目录","VariablesFile"=>"共享游戏设置文件",_=>"安装目录"};
    public Settings Candidate()=>ConfigurationValidator.FromDirectories(baseline,battle,china,global,variables);
    public void SetSaving(bool value){saving=value;Refresh();}
    public void Status(string key,bool isError=false){messageKey=key;detail=null;errorField=null;error=isError;Refresh();}
    public void Failure(Exception exception){errorField=(exception as SettingsValidationException)?.Field??"Configuration";detail=exception.Message;messageKey=null;error=true;Refresh();}
    public void Accept(Settings settings){baseline=ConfigurationValidator.Clone(settings);battle=Parent(settings.BattleNetPath);china=settings.CN.GamePath;global=settings.Global.GamePath;variables=settings.VariablesPath;Refresh();}
    public void Reset(){Accept(baseline);Status(null);}
    public void ClearNotice(){notice=null;Refresh();}
    void Edited(){Status(null);}
    public void Refresh()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));
}
public partial class SettingsPage:UserControl {
    readonly SettingsPageModel model;
    readonly Func<Settings,Task<Settings>> save;
    public event Action CloseRequested;
    public bool IsSaving=>model.IsSaving;
    public SettingsPage(Settings settings,string notice,Func<Settings,Task<Settings>> save){
        InitializeComponent();this.save=save;model=new SettingsPageModel(settings,notice);DataContext=model;
        Loaded+=(_,_)=>UiText.Instance.PropertyChanged+=LanguageChanged;
        Unloaded+=(_,_)=>UiText.Instance.PropertyChanged-=LanguageChanged;
    }
    void LanguageChanged(object sender,PropertyChangedEventArgs e)=>model.Refresh();
    public void ShowMessage(string key,bool error=false)=>model.Status(key,error);
    public bool CanLeave(){if(model.IsSaving)return false;if(model.HasChanges){model.Status("目录修改尚未保存。请先保存或放弃修改。",true);return false;}return true;}
    void Back_Click(object sender,RoutedEventArgs e){if(CanLeave())CloseRequested?.Invoke();}
    void Discard_Click(object sender,RoutedEventArgs e){model.Reset();CloseRequested?.Invoke();}
    async void Save_Click(object sender,RoutedEventArgs e){
        if(model.IsSaving)return;
        model.SetSaving(true);model.Status("正在检查安装目录与设置文件…");
        try {
            var saved=await save(model.Candidate());model.Accept(saved);model.ClearNotice();model.Status("目录已保存，安装文件检查通过。");
        }catch(Exception error){model.Failure(error);if(error is SettingsValidationException validation)FocusField(validation.Field);}
        finally{model.SetSaving(false);}
    }
    void FocusField(string field){
        var box=field switch{"BattleNetDirectory"=>BattleField,"ChinaDirectory"=>ChinaField,"GlobalDirectory"=>GlobalField,"VariablesFile"=>VariablesField,_=>null};
        if(box==null)return;if(field=="VariablesFile")Advanced.IsExpanded=true;
        Dispatcher.BeginInvoke(new Action(()=>{box.BringIntoView();box.Focus();box.SelectAll();}));
    }
    void BrowseFolder_Click(object sender,RoutedEventArgs e){
        string field=(string)((Button)sender).Tag;
        string current=field switch{"BattleNetDirectory"=>model.BattleNetDirectory,"ChinaDirectory"=>model.ChinaDirectory,_=>model.GlobalDirectory};
        try {
            var dialog=new OpenFolderDialog{Title=UiText.T(SettingsPageModel.FieldName(field)),Multiselect=false};
            if(Directory.Exists(current))dialog.InitialDirectory=current;
            if(dialog.ShowDialog(Window.GetWindow(this))!=true)return;
            if(field=="BattleNetDirectory")model.BattleNetDirectory=dialog.FolderName;else if(field=="ChinaDirectory")model.ChinaDirectory=dialog.FolderName;else model.GlobalDirectory=dialog.FolderName;
        }catch(Exception error){model.Failure(error);}
    }
    void BrowseVariables_Click(object sender,RoutedEventArgs e){
        try {
            var dialog=new OpenFileDialog{Title=UiText.T("共享游戏设置文件"),Filter="Variables.txt|Variables.txt",CheckFileExists=true,Multiselect=false};
            if(File.Exists(model.VariablesFile)){dialog.FileName=model.VariablesFile;dialog.InitialDirectory=Path.GetDirectoryName(model.VariablesFile);}
            if(dialog.ShowDialog(Window.GetWindow(this))==true)model.VariablesFile=dialog.FileName;
        }catch(Exception error){model.Failure(error);}
    }
}
