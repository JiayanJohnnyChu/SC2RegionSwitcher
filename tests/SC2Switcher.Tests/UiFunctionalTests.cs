using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Sc2Switch2;
using Sc2Wpf;

partial class Tests {
    static void UiFunctionalTests() {
        Test("latest installation inspection wins over an older failure",()=>{
            var settings=ValidSettings(TestPath("ui-inspection-race"));
            using var entered=new ManualResetEventSlim();
            using var release=new ManualResetEventSlim();
            int globalCalls=0;
            var model=new SettingsPageModel(settings,null,inspect:(profile,cn)=>{
                if(cn)return new BuildInfo{Version="3.0.0.1"};
                if(Interlocked.Increment(ref globalCalls)==1){
                    entered.Set();
                    if(!release.Wait(TimeSpan.FromSeconds(10)))throw new TimeoutException("First inspection was not released");
                    throw new IOException("obsolete inspection failure");
                }
                return new BuildInfo{Version="3.0.0.2"};
            });
            Task first=model.CheckInstallationsAsync();
            try {
                Assert(entered.Wait(TimeSpan.FromSeconds(10)),"First inspection did not start");
                model.CheckInstallationsAsync().GetAwaiter().GetResult();
            } finally {
                release.Set();
                first.GetAwaiter().GetResult();
            }
            Assert(globalCalls==2&&model.GlobalVersion=="3.0.0.2"&&model.ChinaVersion=="3.0.0.1","Older inspection replaced the latest versions");
            Assert(model.GlobalCheckNote.Length==0&&model.GlobalCheckColor=="#315B45","Older inspection failure replaced a successful check");
        });
        Test("installation check results follow the current directory draft",()=>{
            var settings=ValidSettings(TestPath("ui-check-draft"));
            string other=Path.Combine(Path.GetDirectoryName(settings.Global.GamePath),"OtherGlobal");
            var model=new SettingsPageModel(settings,null,inspect:(profile,cn)=>{
                if(!cn&&profile.GamePath==other)throw new IOException("replacement installation is incomplete");
                return new BuildInfo{Version=cn?"1.0.0.1":"2.0.0.1"};
            });
            model.CheckInstallationsAsync().GetAwaiter().GetResult();
            Assert(model.GlobalVersion=="2.0.0.1"&&model.ChinaVersion=="1.0.0.1","Initial installation versions were not exposed");
            model.GlobalDirectory=other;
            Assert(model.HasChanges&&model.DiscardVisibility==Visibility.Visible&&model.GlobalVersion=="—"&&model.GlobalCheckLabel==UiText.T("web.pendingCheck"),"Changed path retained an old inspection result");
            Assert(model.ChinaVersion=="1.0.0.1","Global path edit discarded the independent China check");
            model.CheckInstallationsAsync().GetAwaiter().GetResult();
            Assert(model.GlobalVersion=="—"&&model.GlobalCheckNote.Contains("replacement installation is incomplete")&&model.GlobalCheckColor=="#962B23","New path inspection failure was hidden");
            model.Reset();
            Assert(!model.HasChanges&&model.DiscardVisibility==Visibility.Collapsed&&model.GlobalDirectory==settings.Global.GamePath&&model.GlobalCheckLabel==UiText.T("web.pendingCheck"),"Discard retained the replacement path or its check result");
        });
        Test("settings draft and discard preserve a mixed language baseline",()=>{
            var settings=ValidSettings(TestPath("ui-mixed-draft"));
            settings.Global.SpeechLocale="koKR";
            string originalVariables=Files.HashFile(settings.VariablesPath);
            var model=new SettingsPageModel(settings,null);
            model.BattleNetDirectory=Path.Combine(Path.GetDirectoryName(settings.BattleNetPath),"OtherBattle");
            model.ChinaDirectory=Path.Combine(Path.GetDirectoryName(settings.CN.GamePath),"OtherChina");
            model.VariablesFile=Path.Combine(Path.GetDirectoryName(settings.VariablesPath),"OtherVariables.txt");
            var candidate=model.Candidate();
            Assert(model.HasChanges&&candidate.Global.TextLocale=="enUS"&&candidate.Global.SpeechLocale=="koKR","Unrelated draft changed the mixed game languages");
            Assert(candidate.BattleNetPath!=settings.BattleNetPath&&candidate.CN.GamePath!=settings.CN.GamePath&&candidate.VariablesPath!=settings.VariablesPath,"Draft did not include all edited paths");
            Assert(settings.Global.SpeechLocale=="koKR"&&Files.HashFile(settings.VariablesPath)==originalVariables,"Draft modified saved settings or game bytes");
            model.Reset();
            var restored=model.Candidate();
            Assert(!model.HasChanges&&restored.BattleNetPath==settings.BattleNetPath&&restored.CN.GamePath==settings.CN.GamePath&&restored.VariablesPath==settings.VariablesPath,"Discard did not restore all directory fields");
            Assert(restored.Global.TextLocale=="enUS"&&restored.Global.SpeechLocale=="koKR"&&Files.HashFile(settings.VariablesPath)==originalVariables,"Discard modified the mixed language pair or game bytes");
        });
        Test("home navigation and actions reflect inspection and settings activity",()=>{
            UiText.SetLanguage("zh-CN");
            var model=new MainViewModel();model.Initialize("EU","EU");model.SetReady(true);
            model.SetApplied(new(){TextLocale="enUS",SpeechLocale="enUS"});
            Assert(model.CanNavigate&&model.CanInspect&&model.CanEditRegion&&!model.CanSwitch,"Idle action state is inconsistent");
            model.SetPage("settings");model.SetSettingsBusy(true);
            string language=model.UiLanguage;
            model.UiLanguage="en-US";
            Assert(model.SettingsActive&&!model.HomeActive&&!model.CanNavigate&&model.UiLanguage==language,"Saving settings left navigation or interface language enabled");
            model.SetSettingsBusy(false);model.SetPage("home");model.SetInspecting(true);
            Assert(model.HomeActive&&!model.CanNavigate&&!model.CanInspect&&!model.CanEditRegion&&!model.CanSwitch,"Inspection left competing actions enabled");
            model.SetInspecting(false);model.Americas=true;
            Assert(model.CanNavigate&&model.CanInspect&&model.CanEditRegion&&model.CanSwitch,"Finished inspection did not restore a changed-region action");
            model.SetBusy(true);
            Assert(!model.CanNavigate&&!model.CanInspect&&!model.CanEditRegion&&!model.CanSwitch&&model.BusyVisibility==Visibility.Visible,"Switching left navigation or actions enabled");
            model.SetBusy(false);model.Europe=true;
            Assert(model.CanNavigate&&model.CanInspect&&!model.CanSwitch&&model.BusyVisibility==Visibility.Collapsed,"Returning to the applied region did not close the action gate");
        });
        Test("validation errors translate after an interface language change without rewriting external details",()=>{
            UiText.SetLanguage("en-US");
            var settings=ValidSettings(TestPath("ui-error-language"));
            File.Delete(settings.BattleNetPath);
            SettingsValidationException failure=null;
            try{ConfigurationValidator.Validate(settings);}
            catch(SettingsValidationException error){failure=error;}
            Assert(failure!=null&&failure.Field=="BattleNetDirectory","Fixture did not produce a Battle.net path validation error");
            const string detailKey="所选目录中没有 Battle.net.exe。请选择战网的安装文件夹。";
            string englishDetail=UiText.Get("en-US",detailKey),chineseDetail=UiText.Get("zh-CN",detailKey);
            Assert(englishDetail!=chineseDetail&&failure.Message==englishDetail,"Fixture did not produce the catalog's English error");
            var editor=new SettingsPageModel(settings,null);
            var home=new MainViewModel();
            editor.Failure(failure);
            home.Status("安装检查未通过",UiText.MessageKey(failure.Message),"error",failure.Field);
            Assert(editor.Message=="Battle.net folder: "+englishDetail&&home.StatusDetail==editor.Message,"English error lost its field or detail");
            UiText.SetLanguage("zh-CN");
            Assert(editor.Message=="战网目录: "+chineseDetail&&home.StatusDetail==editor.Message,"Saved validation error did not translate both field and detail");
            const string external="External diagnostic 42";
            editor.Failure(new SettingsValidationException("BattleNetDirectory",external));
            home.Status("安装检查未通过",UiText.MessageKey(external),"error","BattleNetDirectory");
            UiText.SetLanguage("en-US");
            Assert(editor.Message=="Battle.net folder: "+external&&home.StatusDetail==editor.Message,"Unknown external detail was rewritten in English");
            UiText.SetLanguage("zh-CN");
            Assert(editor.Message=="战网目录: "+external&&home.StatusDetail==editor.Message,"Unknown external detail was rewritten in Chinese");
        });
    }
}
