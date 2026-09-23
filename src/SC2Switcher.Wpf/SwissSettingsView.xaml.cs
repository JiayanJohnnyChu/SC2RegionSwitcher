using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sc2Switch2;

namespace Sc2Wpf;

public partial class SwissSettingsView:UserControl {
    readonly SettingsPageModel model;
    readonly Func<Settings,Task<Settings>> save;
    public event Action CloseRequested;
    public bool IsSaving=>model.IsSaving;
    public SwissSettingsView(Settings settings,string notice,Func<Settings,Task<Settings>> save,Func<string,Task<IReadOnlyList<string>>> discover=null,Func<Profile,bool,BuildInfo> inspect=null){
        InitializeComponent();this.save=save;model=new SettingsPageModel(settings,notice,discover,inspect);DataContext=model;SizeChanged+=(_,_)=>AdaptLayout();
        Loaded+=async(_,_)=>{UiText.Instance.PropertyChanged+=LanguageChanged;await model.DetectLanguagesAsync();await model.CheckInstallationsAsync();};
        Unloaded+=(_,_)=>{UiText.Instance.PropertyChanged-=LanguageChanged;model.CancelDetection();};
    }
    void AdaptLayout(){
        bool narrow=ActualWidth<=600,compact=ActualWidth<=900;
        double gutter=narrow?18:compact?24:40;
        double scrollbarSpace=FormScroll.ComputedVerticalScrollBarVisibility==Visibility.Visible&&FormScroll.ViewportWidth>0
            ?Math.Max(0,FormScroll.ActualWidth-FormScroll.ViewportWidth):0;
        FormContent.Margin=new Thickness(gutter,compact?24:28,Math.Max(0,gutter-scrollbarSpace),compact?24:28);
        SettingsDock.Padding=new Thickness(gutter,compact?14:20,gutter,compact?6:12);
        SettingsTitle.FontSize=narrow?30:36;
        bool shortWindow=(Window.GetWindow(this)?.ActualHeight??800)<=740;
        SettingsCategory.Visibility=narrow&&shortWindow?Visibility.Collapsed:Visibility.Visible;
        SettingsTitle.Margin=new Thickness(0,narrow&&shortWindow?0:12,0,12);
        PageHeading.Margin=new Thickness(0,0,0,narrow?20:28);
        InstallationsNumber.Visibility=InstallationsHeading.Visibility=narrow?Visibility.Collapsed:Visibility.Visible;
        InstallationsSectionTitle.ColumnDefinitions[0].Width=new GridLength(narrow?0:34);
        foreach(var item in new[]{(InstallationsSection,InstallationsSectionTitle,InstallationsSectionFields),(LauncherSection,LauncherSectionTitle,LauncherSectionFields),(InterfaceSection,InterfaceSectionTitle,InterfaceSectionFields)}){
            item.Item1.ColumnDefinitions[1].Width=new GridLength(narrow?0:32);
            item.Item1.ColumnDefinitions[2].Width=narrow?new GridLength(0):new GridLength(2,GridUnitType.Star);
            Grid.SetRow(item.Item3,narrow?1:0);Grid.SetColumn(item.Item3,narrow?0:2);
            item.Item3.Margin=new Thickness(0,narrow?18:0,0,0);
            item.Item1.Margin=new Thickness(0,narrow?22:24,0,narrow?22:24);
        }
        SaveButton.MinWidth=narrow?0:190;SaveButton.MaxWidth=narrow?Math.Max(150,(ActualWidth-gutter*2)*.48):320;
    }
    void FormScroll_ScrollChanged(object sender,ScrollChangedEventArgs e){
        if(ReferenceEquals(e.OriginalSource,FormScroll)&&e.ViewportWidthChange!=0)AdaptLayout();
    }
    void LanguageChanged(object sender,PropertyChangedEventArgs e)=>model.Refresh();
    public void ShowMessage(string key,bool error=false)=>model.Status(key,error);
    public bool CanLeave(){if(model.IsSaving)return false;if(model.HasChanges){model.Status("配置修改尚未保存。请先保存或放弃修改。",true);return false;}return true;}
    void Back_Click(object sender,RoutedEventArgs e){if(CanLeave())CloseRequested?.Invoke();}
    void Discard_Click(object sender,RoutedEventArgs e){model.Reset();CloseRequested?.Invoke();}
    async void Save_Click(object sender,RoutedEventArgs e){
        if(!model.CanSave)return;
        bool savedSuccessfully=false;
        model.SetSaving(true);model.Status("正在检查安装目录与设置文件…");
        try {
            var saved=await save(model.Candidate());model.Accept(saved);await model.CheckInstallationsAsync();model.ClearNotice();model.Status("配置已保存，安装文件检查通过。");
            savedSuccessfully=true;
        }catch(Exception error){model.Failure(error);if(error is SettingsValidationException validation)FocusField(validation.Field);}
        finally{model.SetSaving(false);}
        // A direct path-to-save action defers discovery until validation succeeds.
        if(savedSuccessfully)await model.DetectLanguagesAsync();
    }
    public void FocusField(string field){
        Control box=field switch{"BattleNetDirectory"=>BattleField,"ChinaDirectory"=>ChinaField,"GlobalDirectory"=>GlobalField,"VariablesFile"=>VariablesField,"GlobalLanguage"=>GameLanguagePicker,_=>null};
        if(box==null)return;if(field=="VariablesFile")Advanced.IsExpanded=true;
        // New pages must finish loading and layout before scrolling/focusing a
        // field; the default dispatcher priority can run before that happens.
        Dispatcher.BeginInvoke(new Action(()=>{
            if(!IsLoaded)return;
            UpdateLayout();box.BringIntoView();box.Focus();if(box is TextBox textBox)textBox.SelectAll();
        }),System.Windows.Threading.DispatcherPriority.ContextIdle);
    }
    async void DetectLanguages_Click(object sender,RoutedEventArgs e)=>await model.DetectLanguagesAsync();
    async void GlobalDirectory_LostKeyboardFocus(object sender,System.Windows.Input.KeyboardFocusChangedEventArgs e){
        // A click focuses Save before invoking it. Do not disable that button
        // between these events; saving performs full path/language validation.
        if(ReferenceEquals(e.NewFocus,SaveButton))return;
        await model.DetectLanguagesAsync();
    }
    async void BrowseFolder_Click(object sender,RoutedEventArgs e){
        string field=(string)((Button)sender).Tag;
        string current=field switch{"BattleNetDirectory"=>model.BattleNetDirectory,"ChinaDirectory"=>model.ChinaDirectory,_=>model.GlobalDirectory};
        try {
            var dialog=new OpenFolderDialog{Title=UiText.T(SettingsPageModel.FieldName(field)),Multiselect=false};
            if(Directory.Exists(current))dialog.InitialDirectory=current;
            if(dialog.ShowDialog(Window.GetWindow(this))!=true)return;
            if(field=="BattleNetDirectory")model.BattleNetDirectory=dialog.FolderName;else if(field=="ChinaDirectory")model.ChinaDirectory=dialog.FolderName;else model.GlobalDirectory=dialog.FolderName;
            if(field=="GlobalDirectory")await model.DetectLanguagesAsync();
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
