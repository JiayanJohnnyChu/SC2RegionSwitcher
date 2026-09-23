using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Sc2Switch2;

namespace Sc2Wpf;

public partial class SwissWindow {
    // Targeted control/event checks. The existing --matrix isolation and engine
    // guard apply; the save delegate below only returns an in-memory draft.
    async Task ExportInteractionChecks(string directory) {
        var checks=new List<object>();
        int failures=0;
        void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        async Task Drain()=>await Dispatcher.InvokeAsync(()=>UpdateLayout(),DispatcherPriority.ApplicationIdle);
        async Task Until(Func<bool> condition){
            for(int i=0;i<200&&!condition();i++){await Task.Delay(10);await Drain();}
            Require(condition(),"Timed out waiting for the control operation.");
        }
        async Task Check(string name,Func<Task> action){
            try{await action();checks.Add(new{Name=name,Passed=true,Error=(string)null});}
            catch(Exception error){failures++;checks.Add(new{Name=name,Passed=false,Error=error.Message});}
            finally{
                if(settingsPage?.DataContext is SettingsPageModel editor){editor.SetSaving(false);editor.Reset();}
                CloseSheet();UiText.SetLanguage("en-US");await Drain();
            }
        }
        UiText.SetLanguage("en-US");model.SetReady(true);model.SetInstallation(true,true);model.SetInstallation(false,true);
        await Check("navigation selection opens pages and protects unsaved edits",async()=>{
            void Select(int index)=>((ISelectionItemProvider)new RadioButtonAutomationPeer((RadioButton)Navigation.Children[index]).GetPattern(PatternInterface.SelectionItem)).Select();
            Select(1);await Drain();Require(settingsPage!=null&&model.SettingsActive,"Selecting Settings only changed the tab appearance.");
            var editor=(SettingsPageModel)settingsPage.DataContext;editor.GlobalDirectory+=" - draft";
            Select(0);await Drain();Require(settingsPage!=null&&model.SettingsActive&&((RadioButton)Navigation.Children[1]).IsChecked==true,"Navigation lost an unsaved draft or marked the wrong tab.");
            var discard=(Button)((WrapPanel)settingsPage.FindName("DockActions")).Children[1];
            ((IInvokeProvider)new ButtonAutomationPeer(discard).GetPattern(PatternInterface.Invoke)).Invoke();await Drain();
            Require(settingsPage==null&&model.HomeActive,"Discard did not return to the switch page.");
            Select(2);await Drain();Require(sheet?.Child is SwissReferenceView&&model.GuideActive,"Selecting Guide did not open it.");
            ((IInvokeProvider)new ButtonAutomationPeer(sheetCloseButton).GetPattern(PatternInterface.Invoke)).Invoke();await Drain();
            Require(sheet==null&&model.HomeActive,"Guide Back did not return to the switch page.");
        });
        await Check("closed settings cannot unlock the saving page",async()=>{
            OpenSettings();await Drain();var oldEditor=(SettingsPageModel)settingsPage.DataContext;
            CloseSheet();OpenSettings();await Drain();var currentEditor=(SettingsPageModel)settingsPage.DataContext;
            currentEditor.SetSaving(true);await Drain();
            Require(!HeaderLanguage.IsEnabled&&!Navigation.IsEnabled,"Saving did not lock the header.");
            oldEditor.Refresh();await Drain(); // Same notification as a late installation-check completion.
            Require(!HeaderLanguage.IsEnabled&&!Navigation.IsEnabled,"A closed editor unlocked navigation during another page's save.");
        });
        await Check("error details follow the current interface language",async()=>{
            model.Status("操作未完成","检测到游戏文件变化或监测中断。需等待更新、安装或修复结束后重新检查。","error");
            OpenReference(false);await Drain();
            HeaderLanguage.SetCurrentValue(ComboBox.SelectedValueProperty,"zh-CN");await Drain();
            var reference=(SwissReferenceView)sheet.Child;
            Require(((TextBlock)reference.FindName("Introduction")).Text==model.StatusTitle,"Error introduction retained the previous interface language.");
            Require(((TextBox)reference.FindName("ErrorContent")).Text==model.StatusDetail,"Application-owned error detail retained the previous interface language.");
        });
        await Check("header and settings language selectors stay synchronized",async()=>{
            OpenSettings();await Drain();var editor=(SettingsPageModel)settingsPage.DataContext;
            var picker=(ComboBox)settingsPage.FindName("LanguagePicker");
            HeaderLanguage.SetCurrentValue(ComboBox.SelectedValueProperty,"zh-CN");await Drain();
            Require((string)picker.SelectedValue=="zh-CN","Settings language did not follow the header.");
            picker.SetCurrentValue(ComboBox.SelectedValueProperty,"fr-FR");await Drain();
            Require((string)HeaderLanguage.SelectedValue=="fr-FR"&&UiPreferences.Load(preferencesPath)=="fr-FR","Header or saved preference did not follow Settings.");
            Require(!editor.HasChanges&&editor.Candidate().Global.TextLocale==settings.Global.TextLocale,"Interface selection modified the game draft.");
        });
        await Check("path focus loss preserves the first save action",async()=>{
            int discoveries=0,saves=0;
            var pendingDiscovery=new TaskCompletionSource<IReadOnlyList<string>>();
            var completion=new TaskCompletionSource<Settings>();
            var page=new SwissSettingsView(settings,null,candidate=>{saves++;return completion.Task;},
                _=>++discoveries==1?Task.FromResult<IReadOnlyList<string>>(new[]{"enUS","esES"}):pendingDiscovery.Task,
                (_,cn)=>builds[cn?0:1]);
            Root.Visibility=Visibility.Hidden;host.Children.Add(page);
            try{
                await Drain();var editor=(SettingsPageModel)page.DataContext;
                await Until(()=>!editor.IsDetecting);
                var field=(TextBox)page.FindName("GlobalField");var button=(Button)page.FindName("SaveButton");
                field.SetCurrentValue(TextBox.TextProperty,settings.Global.GamePath+" - draft");await Drain();
                // Deliver the real WPF focus event before the button invocation,
                // with a held discovery to make the event-order race deterministic.
                field.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice,0,field,button){RoutedEvent=Keyboard.LostKeyboardFocusEvent});
                await Drain();Require(button.IsEnabled,"Path focus loss disabled Save before its first invocation.");
                ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
                await Until(()=>saves==1);
                Require(editor.IsSaving&&!field.IsEnabled&&!button.IsEnabled,"Save did not lock the editable controls.");
                completion.SetResult(editor.Candidate());await Until(()=>!editor.IsSaving);
                Require(!editor.HasChanges&&page.CanLeave(),"Successful save retained a dirty draft.");
                Require(discoveries==2&&editor.IsDetecting,"The saved path did not refresh its available languages.");
                pendingDiscovery.SetResult(new[]{"enUS","esES"});await Until(()=>!editor.IsDetecting);
                Require(((ComboBox)page.FindName("GameLanguagePicker")).IsEnabled&&editor.GlobalLanguage=="enUS","The saved path left the language picker unavailable.");
            }finally{
                pendingDiscovery.TrySetResult(new[]{"enUS"});completion.TrySetResult(ConfigurationValidator.Clone(settings));
                host.Children.Remove(page);Root.Visibility=Visibility.Visible;
            }
        });
        Json.Write(Path.Combine(directory,"interaction-results.json"),new{Checks=checks,Failures=failures,SimulatedEvents=true,ExternalInput=false,RealSave=false,EngineInvoked=false});
        if(failures>0)throw new InvalidOperationException($"{failures} targeted interaction checks failed; see interaction-results.json.");
    }
}
