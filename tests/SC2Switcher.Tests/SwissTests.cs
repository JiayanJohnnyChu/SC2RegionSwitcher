using System;
using System.IO;
using System.Linq;
using System.Text;
using Sc2Switch2;
using Sc2Wpf;

partial class Tests {
    static void SwissTests() {
        Test("unchanged applied region text and speech disable switching",()=>{
            var vm=new MainViewModel();vm.Initialize("EU","EU");vm.SetReady(true);
            vm.SetApplied(new(){TextLocale="enUS",SpeechLocale="enUS"});
            Assert(!vm.CanSwitch,"Unchanged configuration allowed a restart");
            vm.Americas=true;Assert(vm.CanSwitch,"Region change blocked");vm.Europe=true;Assert(!vm.CanSwitch,"Returning to current region did not disable");
            vm.IsChina=true;Assert(vm.CanSwitch,"China change blocked");vm.IsGlobal=true;
            vm.SetGameLanguages(new Profile{TextLocale="enUS",SpeechLocale="koKR"});Assert(vm.CanSwitch,"Speech-only change blocked");
            vm.SetApplied(new(){TextLocale="enUS",SpeechLocale="koKR"});Assert(!vm.CanSwitch,"Successful change did not disable restart");
            vm.SetApplied(new(){TextLocale="enUS",SpeechLocale="koKR",NeedsRecovery=true});Assert(vm.CanSwitch,"Recovery was trapped behind unchanged gate");
            vm.SetApplied(new());Assert(vm.CanSwitch,"Unknown record incorrectly considered unchanged");
        });
        Test("applied language observation reads strict encodings without writes",()=>{
            string folder=TestPath("applied-observation"),file=Path.Combine(folder,"Variables.txt");
            foreach(Encoding encoding in new Encoding[]{new UTF8Encoding(false),new UTF8Encoding(true),new UnicodeEncoding(false,true),new UnicodeEncoding(true,true)}){
                File.WriteAllText(file,"localeidassets=koKR\r\nlocaleiddata=enUS\r\n",encoding);string hash=Files.HashFile(file);
                var applied=AppliedConfiguration.Read(file,folder);
                Assert(applied.IsKnown&&applied.TextLocale=="enUS"&&applied.SpeechLocale=="koKR"&&Files.HashFile(file)==hash,"Read failed or changed bytes");
            }
            File.WriteAllText(file,"localeiddata=enUS\nlocaleiddata=frFR\nlocaleidassets=enUS\n");Assert(!AppliedConfiguration.Read(file,folder).IsKnown,"Duplicate record considered known");
            File.WriteAllText(file,"localeiddata=oops\nlocaleidassets=enUS\n");Assert(!AppliedConfiguration.Read(file,folder).IsKnown,"Invalid locale considered known");
            File.WriteAllText(Path.Combine(folder,"pending-language.json"),"{}");Assert(AppliedConfiguration.Read(file,folder).NeedsRecovery,"Pending recovery ignored");
            Assert(!AppliedConfiguration.Read(Path.Combine(folder,"missing.txt"),folder).IsKnown,"Missing file considered known");
        });
        Test("all eleven interface languages persist independently of game configuration",()=>{
            string prefs=Path.Combine(TestPath("swiss-locales"),"ui-preferences.json");
            var vm=new MainViewModel();vm.Initialize("EU","EU");vm.SetApplied(new(){TextLocale="enUS",SpeechLocale="enUS"});vm.SetReady(true);
            Assert(UiTypography.Languages.Count==11,"Locale missing");
            foreach(var locale in UiTypography.Languages){
                Assert(UiText.KeysFor(locale.Code).OrderBy(x=>x).SequenceEqual(UiText.Keys.OrderBy(x=>x)),"Catalogue key set differs: "+locale.Code);
                UiPreferences.Save(prefs,locale.Code);Assert(UiPreferences.Load(prefs)==locale.Code,"Locale preference lost");
                vm.UiLanguage=locale.Code;Assert(!vm.CanSwitch&&vm.CurrentLoginRegion=="EU","Interface changed applied configuration");
                foreach(string key in UiText.Keys)Assert(Placeholders(UiText.Get(locale.Code,key))==Placeholders(UiText.Get("en-US",key)),"Placeholder mismatch: "+locale.Code+" "+key);
                Assert(!UiText.T("web.taskTitle").StartsWith("web."),"Raw key displayed");
            }
            UiText.SetLanguage("zh-CN");
        });
    }
}
