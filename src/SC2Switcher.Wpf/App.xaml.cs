using System;
using System.IO;
using System.Windows;
using Sc2Switch2;

namespace Sc2Wpf;
public partial class App : Application {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);
        try {
            var root=AppContext.BaseDirectory;
            var state=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SC2RegionSwitcherV2");
            int dataIndex=Array.IndexOf(e.Args,"--data-dir");
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
            var window=new MainWindow(loaded,configuration,state,report);
            if(report!=null&&Array.IndexOf(e.Args,"--compact")>=0){window.Width=520;window.Height=560;}
            if(report!=null&&Array.IndexOf(e.Args,"--matrix")>=0)window.ContentRendered+=async(_,_)=>{try{await window.ExportDesignStates(Path.GetDirectoryName(report));}catch(Exception error){Json.Write(Path.Combine(Path.GetDirectoryName(report),"layout-error.json"),new{error.Message});Shutdown(1);}};
            MainWindow=window;window.Show();
        } catch(Exception error) {
            MessageBox.Show(error.Message,UiText.T("双服切换 · 无法打开"),MessageBoxButton.OK,MessageBoxImage.Information);Shutdown(1);
        }
    }
}
