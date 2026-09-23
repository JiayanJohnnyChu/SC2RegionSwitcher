using System;
using System.IO;
using System.Windows;
using Sc2Switch2;

namespace Sc2Wpf;
public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        try {
            int fontIndex=Array.IndexOf(e.Args,"--font-report");
            if(fontIndex>=0){FontDiagnostics.Write(Path.GetFullPath(e.Args[fontIndex+1]));Shutdown();return;}
            var root=AppContext.BaseDirectory;
            var state=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SC2RegionSwitcherV2");
            int dataIndex=Array.IndexOf(e.Args,"--data-dir");
            if(Array.IndexOf(e.Args,"--matrix")>=0&&dataIndex<0)throw new ArgumentException("--matrix requires an isolated --data-dir.");
            if(dataIndex>=0){if(dataIndex+1>=e.Args.Length||!Path.IsPathFullyQualified(e.Args[dataIndex+1]))throw new ArgumentException("--data-dir requires an absolute path.");state=Path.GetFullPath(e.Args[dataIndex+1]);}
            var preferences=Path.Combine(state,"ui-preferences.json");
            UiText.SetLanguage(UiPreferences.Load(preferences));
            int languageIndex=Array.IndexOf(e.Args,"--language");
            if(languageIndex>=0&&languageIndex+1<e.Args.Length)UiText.SetLanguage(e.Args[languageIndex+1]);
            var configuration=new ConfigurationStore(state,Path.Combine(root,"profiles.json"));
            var loaded=configuration.Load();var settings=loaded.Settings;
            if(e.Args.Length>=2 && e.Args[0]=="--inspect"){
                Json.Write(Path.GetFullPath(e.Args[1]),new {Builds=new Engine(settings,state,new NativePlatform(settings.BattleNetPath)).Inspect(),Runtime=Environment.Version.ToString(),Region=new NativePlatform(settings.BattleNetPath).ReadRegion()});Shutdown();return;
            }
            string report=e.Args.Length>=2&&e.Args[0]=="--ui-report"?Path.GetFullPath(e.Args[1]):null;
            if(report!=null){
                Directory.CreateDirectory(Path.GetDirectoryName(report));
                System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level=System.Diagnostics.SourceLevels.Error;
                System.Diagnostics.PresentationTraceSources.DataBindingSource.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(Path.Combine(Path.GetDirectoryName(report),"bindings.log")));
                System.Diagnostics.Trace.AutoFlush=true;
            }
            var window=new SwissWindow(loaded,configuration,state,report);
            if(report!=null&&Array.IndexOf(e.Args,"--compact")>=0){window.Width=520;window.Height=560;}
            if(report!=null&&Array.IndexOf(e.Args,"--matrix")>=0){
                window.ShowActivated=false;window.ShowInTaskbar=false;
                // Start from Loaded so validation does not depend on foreground
                // input or a compositor frame while the unattended desktop is locked.
                window.Loaded+=async(_,_)=>{try{await window.ExportDesignStates(Path.GetDirectoryName(report));}catch(Exception error){Json.Write(Path.Combine(Path.GetDirectoryName(report),"layout-error.json"),new{error.Message});Shutdown(1);}};
            }
            MainWindow=window;window.Show();
        } catch(Exception error) {
            // Unattended UI validation must never wait on a modal dialog.
            if(Array.IndexOf(e.Args,"--matrix")>=0||Array.IndexOf(e.Args,"--ui-report")>=0){
                int index=Array.IndexOf(e.Args,"--ui-report");
                if(index>=0&&index+1<e.Args.Length){try{Json.Write(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(e.Args[index+1])),"startup-error.json"),new{error.Message,Detail=error.ToString()});}catch{}}
                Shutdown(1);return;
            }
            MessageBox.Show(error.Message,UiText.T("双服切换 · 无法打开"),MessageBoxButton.OK,MessageBoxImage.Information);Shutdown(1);
        }
    }
}
