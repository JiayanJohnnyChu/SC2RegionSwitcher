using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sc2Switch2;
using Sc2Wpf;

partial class Tests {
    static string LocaleTags(params string[] locales)=>String.Join(":",locales.SelectMany(x=>new[]{"Windows "+x+" text?","Windows "+x+" speech?"}));
    static string TaggedManifest(string tags,string branch="eu")=>"Tags|Version|Active|Branch\r\n"+tags+"|5.0.0.12345|1|"+branch+"\r\n";
    static void SetLanguages(Settings value,params string[] locales)=>File.WriteAllText(Path.Combine(value.Global.GamePath,".build.info"),TaggedManifest(LocaleTags(locales)));
    static Settings LanguageFixture(string name){var value=ValidSettings(TestPath("languages-"+name));SetLanguages(value,"enUS","koKR","zhTW");return value;}
    static void Detect(SettingsPageModel model)=>model.DetectLanguagesAsync().GetAwaiter().GetResult();
    static void LanguageTests(){
        Test("real-format Windows tags allow intervening installation qualifiers",()=>{
            var b=BuildInfo.Parse(TaggedManifest("Windows code KR? acct-XXX? geoip-YY? enUS speech?:Windows code KR? acct-XXX? geoip-YY? enUS text?:Windows code KR? zhTW text?:Windows code KR? zhTW speech?"));
            Assert(b.AvailableLocales.SequenceEqual(new[]{"enUS","zhTW"}),"Installation qualifiers hid available language pairs");
            var china=BuildInfo.Parse(TaggedManifest("Windows code CN? acct-XXX? geoip-YY? zhCN speech?:Windows code CN? acct-XXX? geoip-YY? zhCN text?","cn"));
            Assert(china.AvailableLocales.SequenceEqual(new[]{"zhCN"}),"China qualifiers hid Simplified Chinese");
        });
        Test("manifest language discovery intersects sorted Windows text and speech",()=>{
            var b=BuildInfo.Parse(TaggedManifest(LocaleTags("zhTW","enUS","koKR","enUS")+":Windows deDE text:Windows frFR speech"));
            Assert(b.AvailableLocales.SequenceEqual(new[]{"enUS","koKR","zhTW"}),"Intersection or ordering differs");
            Assert(b.TextLocales.Contains("deDE")&&!b.SpeechLocales.Contains("deDE")&&b.SpeechLocales.Contains("frFR"),"Resource types collapsed");
        });
        Test("manifest language discovery ignores malformed foreign-platform and partial tags",()=>{
            var b=BuildInfo.Parse(TaggedManifest("Mac enUS text?:Mac enUS speech?:Windows enUS text?junk:Windows enus speech?:OtherWindows koKR text:Windows koKR speech:Windows zhTW text-extra:Windows zhTW speech:"+LocaleTags("zzZZ")));
            Assert(b.AvailableLocales.SequenceEqual(new[]{"zzZZ"}),"Invalid or non-Windows tag accepted");
            Assert(GameLocales.DisplayName("zzZZ")=="zzZZ","Unknown valid locale should display its code");
            Assert(GameLocales.DisplayName("zhTW").Contains("zhTW"),"Known locale code missing from label");
        });
        Test("language discovery only considers the unique active build",()=>{
            string active=TaggedManifest(LocaleTags("koKR"));
            var b=BuildInfo.Parse(active+LocaleTags("enUS")+"|5.0.0.12344|0|eu\r\n");
            Assert(b.AvailableLocales.SequenceEqual(new[]{"koKR"}),"Inactive resources appeared");
            Block(()=>BuildInfo.Parse(active+LocaleTags("enUS")+"|5.0.0.12346|1|eu\r\n"));
        });
        Test("language discovery works when the previously selected language is absent",()=>{
            var s=LanguageFixture("removed-english");SetLanguages(s,"zhTW","koKR");
            Assert(ConfigurationValidator.DiscoverGlobalLanguages(s.Global.GamePath).SequenceEqual(new[]{"koKR","zhTW"}),"Discovery requires old language");
            Block(()=>ConfigurationValidator.Validate(s));
            var m=new SettingsPageModel(s,null);Detect(m);Assert(m.GlobalLanguage==null&&!m.HasChanges,"Discovery silently replaced saved English");
            m.GlobalLanguage="zhTW";var valid=ConfigurationValidator.Validate(m.Candidate());Assert(valid.Global.SpeechLocale=="zhTW","New selection not validated");
        });
        Test("language discovery retains installation completion branch and manifest checks",()=>{
            var s=LanguageFixture("structure");string patch=Path.Combine(s.Global.GamePath,".patch.result"),manifest=Path.Combine(s.Global.GamePath,".build.info");
            File.WriteAllText(patch,"1");Block(()=>ConfigurationValidator.DiscoverGlobalLanguages(s.Global.GamePath));
            File.Delete(patch);Block(()=>ConfigurationValidator.DiscoverGlobalLanguages(s.Global.GamePath));File.WriteAllText(patch,"0");
            File.WriteAllText(manifest,TaggedManifest(LocaleTags("zhTW"),"cn"));Block(()=>ConfigurationValidator.DiscoverGlobalLanguages(s.Global.GamePath));
            File.WriteAllText(manifest,"Tags|Version\nenUS|1");Block(()=>ConfigurationValidator.DiscoverGlobalLanguages(s.Global.GamePath));
            File.Delete(manifest);Block(()=>ConfigurationValidator.DiscoverGlobalLanguages(s.Global.GamePath));
        });
        Test("language selection remains a draft and discard restores both locale fields",()=>{
            var s=LanguageFixture("draft");var m=new SettingsPageModel(s,null);string hash=Files.HashFile(s.VariablesPath);
            Detect(m);Assert(!m.HasChanges&&m.GlobalLanguage=="enUS","Discovery changed configuration");
            m.GlobalLanguage="koKR";var candidate=m.Candidate();
            Assert(m.HasChanges&&candidate.Global.TextLocale=="koKR"&&candidate.Global.SpeechLocale=="koKR","Selection did not update both fields");
            Assert(s.Global.TextLocale=="enUS"&&Files.HashFile(s.VariablesPath)==hash,"Draft mutated original or game settings");
            m.GlobalLanguage=null;Assert(m.Candidate().Global.TextLocale=="koKR","Binding deselection erased preference");
            m.Reset();Assert(!m.HasChanges&&m.GlobalLanguage=="enUS","Discard failed");
        });
        Test("language preferences persist using schema 2 without changing game or China",()=>{
            var s=LanguageFixture("persist");string folder=Path.Combine(Path.GetDirectoryName(s.VariablesPath),"state");
            var store=Store(folder);store.Load();store.Save(s);byte[] before=File.ReadAllBytes(s.VariablesPath);
            var m=new SettingsPageModel(s,null);Detect(m);m.GlobalLanguage="zhTW";var saved=store.Save(m.Candidate());m.Accept(saved);
            var reopened=Store(folder).Load();Assert(!reopened.NeedsSetup&&reopened.Settings.SchemaVersion==2&&reopened.Settings.Global.TextLocale=="zhTW"&&reopened.Settings.Global.SpeechLocale=="zhTW","Restart lost locale pair");
            Assert(saved.CN.TextLocale=="zhCN"&&saved.CN.SpeechLocale=="zhCN"&&File.ReadAllBytes(s.VariablesPath).SequenceEqual(before)&&!m.HasChanges,"Save touched game or China");
            Assert(Directory.GetFiles(Path.Combine(folder,"ConfigurationBackups")).Length==1,"Replacement was not backed up");
        });
        Test("existing mixed-language profile survives unrelated edits and explicit choice unifies it",()=>{
            var s=LanguageFixture("mixed");s.Global.TextLocale="enUS";s.Global.SpeechLocale="koKR";
            var m=new SettingsPageModel(s,null);Detect(m);m.VariablesFile=s.VariablesPath;
            var candidate=ConfigurationValidator.Validate(m.Candidate());
            Assert(m.GlobalLanguage==null&&!m.HasChanges&&candidate.Global.TextLocale=="enUS"&&candidate.Global.SpeechLocale=="koKR","Mixed legacy pair overwritten");
            m.GlobalLanguage="zhTW";Assert(m.Candidate().Global.TextLocale=="zhTW"&&m.Candidate().Global.SpeechLocale=="zhTW","Explicit choice not unified");
        });
        Test("interface language and login region changes preserve game language preference",()=>{
            var s=LanguageFixture("independence");s.Global.TextLocale=s.Global.SpeechLocale="koKR";
            var m=new SettingsPageModel(s,null);Detect(m);m.UiLanguage="en-US";m.UiLanguage="zh-CN";
            var vm=new MainViewModel();vm.Initialize("EU","EU");vm.SetGameLanguages(m.Candidate().Global);string label=vm.GlobalGameLanguage;vm.Asia=true;vm.UiLanguage="en-US";
            Assert(vm.Region=="KR"&&vm.CurrentLoginRegion=="EU"&&vm.GlobalGameLanguage==label&&m.Candidate().Global.TextLocale=="koKR"&&!m.HasChanges,"Independent preference changed language");
            Assert(vm.GlobalTargetName.Contains("koKR")&&vm.LanguageTitle.Contains("koKR"),"Main labels retain English assumption");UiText.SetLanguage("zh-CN");
        });
        Test("saving locks language directory and interface edits",()=>{
            var s=LanguageFixture("busy");var m=new SettingsPageModel(s,null);Detect(m);m.SetSaving(true);string ui=m.UiLanguage;
            m.GlobalLanguage="koKR";m.GlobalDirectory="changed";m.UiLanguage=ui=="en-US"?"zh-CN":"en-US";Detect(m);
            Assert(!m.CanEdit&&!m.CanSave&&!m.CanSelectLanguage&&!m.CanDetectLanguage&&!m.HasChanges&&m.UiLanguage==ui,"Busy model accepted edit");m.SetSaving(false);
            Assert(m.CanSave&&m.CanSelectLanguage,"Controls remained locked");
        });
        Test("disconnected installation retains saved preference and configuration bytes",()=>{
            var s=LanguageFixture("disconnected");s.Global.TextLocale=s.Global.SpeechLocale="zhTW";
            string stateRoot=Path.Combine(Path.GetDirectoryName(s.VariablesPath),"state");var store=Store(stateRoot);store.Load();store.Save(s);string configHash=Files.HashFile(store.ConfigPath),gameHash=Files.HashFile(s.VariablesPath);
            File.Move(Path.Combine(s.Global.GamePath,".build.info"),Path.Combine(s.Global.GamePath,".build.info.offline"));
            var loaded=Store(stateRoot).Load();var m=new SettingsPageModel(loaded.Settings,null);Detect(m);
            Assert(loaded.NeedsSetup&&!m.CanSelectLanguage&&!m.HasChanges&&m.Candidate().Global.TextLocale=="zhTW"&&m.GameLanguageSummary.Contains("zhTW"),"Disconnected state discarded preference");
            Block(()=>store.Save(m.Candidate()));Assert(Files.HashFile(store.ConfigPath)==configHash&&Files.HashFile(s.VariablesPath)==gameHash,"Disconnected check wrote files");
        });
        Test("no complete language resource leaves the picker disabled",()=>{
            var s=LanguageFixture("partial");File.WriteAllText(Path.Combine(s.Global.GamePath,".build.info"),TaggedManifest("Windows enUS text?:Windows koKR speech?"));
            var m=new SettingsPageModel(s,null);Detect(m);Assert(m.AvailableLanguages.Count==0&&!m.CanSelectLanguage&&!m.HasChanges&&m.Candidate().Global.TextLocale=="enUS","Partial resources offered or default changed");
        });
        Test("path edits immediately invalidate options and ignore an older asynchronous result",()=>{
            var s=LanguageFixture("async");var first=new TaskCompletionSource<IReadOnlyList<string>>();var second=new TaskCompletionSource<IReadOnlyList<string>>();int calls=0;
            var m=new SettingsPageModel(s,null,_=>++calls==1?first.Task:second.Task);
            Task old=m.DetectLanguagesAsync();Assert(m.IsDetecting&&!m.CanSave,"Detection not exposed");
            m.GlobalDirectory=Path.Combine(s.Global.GamePath,"new");Assert(!m.CanSelectLanguage&&m.AvailableLanguages.Count==0,"Path edit retained options");
            Task current=m.DetectLanguagesAsync();second.SetResult(new[]{"zhTW"});current.GetAwaiter().GetResult();first.SetResult(new[]{"enUS"});old.GetAwaiter().GetResult();
            Assert(m.AvailableLanguages.Single().Code=="zhTW"&&m.GlobalLanguage==null&&m.Candidate().Global.TextLocale=="enUS","Stale result or automatic selection applied");
        });
        Test("older same-path failure and closed-page detection cannot replace current state",()=>{
            var s=LanguageFixture("async-failure");var first=new TaskCompletionSource<IReadOnlyList<string>>();int calls=0;
            var m=new SettingsPageModel(s,null,_=>++calls==1?first.Task:Task.FromResult<IReadOnlyList<string>>(new[]{"koKR"}));
            Task old=m.DetectLanguagesAsync();Detect(m);string message=m.DetectionMessage;first.SetException(new IOException("old failure"));old.GetAwaiter().GetResult();
            Assert(m.AvailableLanguages.Single().Code=="koKR"&&m.DetectionMessage==message,"Stale failure replaced state");
            var pending=new TaskCompletionSource<IReadOnlyList<string>>();var closed=new SettingsPageModel(s,null,_=>pending.Task);Task task=closed.DetectLanguagesAsync();closed.CancelDetection();pending.SetResult(new[]{"enUS"});task.GetAwaiter().GetResult();
            Assert(closed.AvailableLanguages.Count==0&&!closed.IsDetecting,"Unloaded model accepted result");
        });
        Test("removed resources are rejected at save without altering configuration or game",()=>{
            var s=LanguageFixture("remove-before-save");string stateRoot=Path.Combine(Path.GetDirectoryName(s.VariablesPath),"state");var store=Store(stateRoot);store.Load();store.Save(s);
            string configHash=Files.HashFile(store.ConfigPath),gameHash=Files.HashFile(s.VariablesPath);var m=new SettingsPageModel(s,null);Detect(m);m.GlobalLanguage="koKR";SetLanguages(s,"enUS");
            Block(()=>store.Save(m.Candidate()));Assert(Files.HashFile(store.ConfigPath)==configHash&&Files.HashFile(s.VariablesPath)==gameHash,"Failed validation wrote files");
        });
        Test("pending recovery blocks locale changes but still permits login-region changes",()=>{
            var s=LanguageFixture("pending-locale");string stateRoot=Path.Combine(Path.GetDirectoryName(s.VariablesPath),"state");var store=Store(stateRoot);store.Load();store.Save(s);
            File.WriteAllText(Path.Combine(stateRoot,"pending-language.json"),"{}");string gameHash=Files.HashFile(s.VariablesPath);
            var changed=CopySettings(s);changed.Global.TextLocale=changed.Global.SpeechLocale="koKR";BlockField("GlobalLanguage",()=>store.Save(changed));
            changed=CopySettings(s);changed.GlobalRegion="KR";store.Save(changed);var loaded=Store(stateRoot).Load().Settings;
            Assert(loaded.GlobalRegion=="KR"&&loaded.Global.TextLocale=="enUS"&&loaded.Global.SpeechLocale=="enUS"&&Files.HashFile(s.VariablesPath)==gameHash&&File.Exists(Path.Combine(stateRoot,"pending-language.json")),"Pending transaction was changed");
        });
        Test("removed selected language blocks switching before launcher or game writes",()=>{
            var s=LanguageFixture("remove-before-switch");s.Global.TextLocale=s.Global.SpeechLocale="koKR";ConfigurationValidator.Validate(s);SetLanguages(s,"enUS");
            string hash=Files.HashFile(s.VariablesPath);var p=new FakePlatform();Block(()=>new Engine(s,Path.Combine(root,"missing-language-state"),p).Switch("Global",_=>{},CancellationToken.None).GetAwaiter().GetResult());
            Assert(p.Starts==0&&p.Exits==0&&Files.HashFile(s.VariablesPath)==hash,"Missing language caused side effects");
        });
        foreach(string locale in new[]{"enUS","zhTW","koKR"}) {
            Test("simulated China to "+locale+" and back preserves bytes and commits backups",()=>{
                var s=LanguageFixture("roundtrip-"+locale);s.BattleNetPath=typeof(Tests).Assembly.Location;s.Global.TextLocale=s.Global.SpeechLocale=locale;
                var encoding=new UnicodeEncoding(false,true,true);byte[] original=encoding.GetPreamble().Concat(encoding.GetBytes("localeidassets=zhCN\r\nlocaleiddata=zhCN\r\nvolume=0.6\r\ncustom=保留\r\n")).ToArray();File.WriteAllBytes(s.VariablesPath,original);
                string stateRoot=Path.Combine(root,"roundtrip-state-"+locale);var p=new FakePlatform{Region="CN"};var engine=new Engine(s,stateRoot,p);
                var result=engine.Switch("Global",_=>{},CancellationToken.None).GetAwaiter().GetResult();
                Assert(File.ReadAllBytes(s.VariablesPath).SequenceEqual(Language.Change(original,locale,locale))&&File.ReadAllBytes(result.Backup).SequenceEqual(original)&&result.TextLocale==locale&&result.SpeechLocale==locale,"Target locale or backup mismatch");
                engine.Switch("CN",_=>{},CancellationToken.None).GetAwaiter().GetResult();Assert(File.ReadAllBytes(s.VariablesPath).SequenceEqual(original)&&p.Starts==2&&!File.Exists(Path.Combine(stateRoot,"pending-language.json")),"Roundtrip failed");
            });
        }
        Test("non-English launch failure rolls back and interrupted transaction recovers",()=>{
            var s=LanguageFixture("rollback");s.Global.TextLocale=s.Global.SpeechLocale="zhTW";string hash=Files.HashFile(s.VariablesPath),stateRoot=Path.Combine(root,"multilanguage-rollback");
            Block(()=>new Engine(s,stateRoot,new FakePlatform{StartFails=true}).Switch("Global",_=>{},CancellationToken.None).GetAwaiter().GetResult());
            Assert(Files.HashFile(s.VariablesPath)==hash&&!File.Exists(Path.Combine(stateRoot,"pending-language.json")),"Non-English rollback failed");
            new LanguageTransaction(stateRoot,s.VariablesPath).Apply("Global",s.Global,()=>{});
            LanguageTransaction.Recover(stateRoot,s.VariablesPath,()=>{});Assert(Files.HashFile(s.VariablesPath)==hash,"Non-English recovery failed");
        });
    }
}
