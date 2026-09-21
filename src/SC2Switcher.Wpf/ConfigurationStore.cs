using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;

namespace Sc2Switch2;

public sealed class SettingsValidationException : IOException {
    public string Field {get;}
    public SettingsValidationException(string field,string message):base(message){Field=field;}
}
public sealed class ConfigurationLoadResult {
    public Settings Settings {get;set;}
    public bool NeedsSetup {get;set;}
    public string NoticeKey {get;set;}
    public bool Migrated {get;set;}
}
public static class ConfigurationValidator {
    public static Settings Defaults() {
        var candidates=new[]{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)};
        string battle=candidates.Where(x=>!String.IsNullOrEmpty(x)).Select(x=>Path.Combine(x,"Battle.net","Battle.net.exe")).FirstOrDefault(File.Exists)??"";
        return new Settings{SchemaVersion=2,BattleNetPath=battle,VariablesPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"StarCraft II","Variables.txt"),GlobalRegion="EU",CN=new Profile{Name="国服",GamePath="",TextLocale="zhCN",SpeechLocale="zhCN"},Global=new Profile{Name="外服",GamePath="",TextLocale="enUS",SpeechLocale="enUS"}};
    }
    public static Settings Clone(Settings s)=>new Settings{SchemaVersion=s.SchemaVersion,BattleNetPath=s.BattleNetPath,VariablesPath=s.VariablesPath,GlobalRegion=s.GlobalRegion,CN=Copy(s.CN),Global=Copy(s.Global)};
    static Profile Copy(Profile p)=>p==null?null:new Profile{Name=p.Name,GamePath=p.GamePath,TextLocale=p.TextLocale,SpeechLocale=p.SpeechLocale};
    public static Settings FromDirectories(Settings original,string battleNetDirectory,string chinaDirectory,string globalDirectory,string variablesFile) {
        var s=Clone(original);
        s.BattleNetPath=Path.Combine(LocalPath(battleNetDirectory,"BattleNetDirectory",true),"Battle.net.exe");
        s.CN.GamePath=LocalPath(chinaDirectory,"ChinaDirectory",true);
        s.Global.GamePath=LocalPath(globalDirectory,"GlobalDirectory",true);
        s.VariablesPath=LocalPath(variablesFile,"VariablesFile",false);
        return s;
    }
    static string LocalPath(string value,string field,bool directory) {
        try {
            value=value?.Trim().Trim('"');
            if(String.IsNullOrWhiteSpace(value)||!Path.IsPathFullyQualified(value))throw new IOException(UiText.T("请输入完整的本机路径。"));
            string full=Path.GetFullPath(value),root=Path.GetPathRoot(full);
            if(root.Length!=3||!Char.IsLetter(root[0])||root[1]!=':')throw new IOException(UiText.T("请选择本机磁盘上的路径。"));
            if(directory&&String.Equals(full.TrimEnd('\\'),root.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase))throw new IOException(UiText.T("请选择实际安装文件夹，不要选择磁盘根目录。"));
            return directory?full.TrimEnd('\\','/'):full;
        }catch(Exception error) when(error is IOException||error is ArgumentException||error is NotSupportedException){throw new SettingsValidationException(field,error.Message);}
    }
    static void Check(string field,Action action) {
        try{action();}catch(SettingsValidationException){throw;}catch(Exception error) when(error is IOException||error is InvalidDataException||error is UnauthorizedAccessException||error is ArgumentException||error is NotSupportedException){throw new SettingsValidationException(field,error.Message);}
    }
    public static Settings Validate(Settings original) {
        if(original==null||original.SchemaVersion!=2||original.CN==null||original.Global==null)throw new SettingsValidationException("Configuration",UiText.T("安装配置格式无效。请重新选择目录。"));
        var s=Clone(original);
        s.BattleNetPath=LocalPath(s.BattleNetPath,"BattleNetDirectory",false);
        s.CN.GamePath=LocalPath(s.CN.GamePath,"ChinaDirectory",true);
        s.Global.GamePath=LocalPath(s.Global.GamePath,"GlobalDirectory",true);
        s.VariablesPath=LocalPath(s.VariablesPath,"VariablesFile",false);
        if(!new[]{"EU","US","KR"}.Contains(s.GlobalRegion))throw new SettingsValidationException("Configuration",UiText.T("外服登录区域无效。"));
        Check("GlobalDirectory",()=>Paths.RequireSeparate(s.CN.GamePath,s.Global.GamePath));
        Check("BattleNetDirectory",()=>{
            if(!String.Equals(Path.GetFileName(s.BattleNetPath),"Battle.net.exe",StringComparison.OrdinalIgnoreCase)||!File.Exists(s.BattleNetPath))throw new IOException(UiText.T("所选目录中没有 Battle.net.exe。请选择战网的安装文件夹。"));
            Paths.RejectLinks(s.BattleNetPath);
        });
        Check("ChinaDirectory",()=>{if(!Directory.Exists(s.CN.GamePath))throw new IOException(UiText.T("目录不存在或无法访问。"));BuildInfo.Load(s.CN,true);});
        Check("GlobalDirectory",()=>{if(!Directory.Exists(s.Global.GamePath))throw new IOException(UiText.T("目录不存在或无法访问。"));BuildInfo.Load(s.Global,false);});
        Check("VariablesFile",()=>{
            if(!String.Equals(Path.GetFileName(s.VariablesPath),"Variables.txt",StringComparison.OrdinalIgnoreCase)||!File.Exists(s.VariablesPath))throw new IOException(UiText.T("找不到 Variables.txt。请先启动并退出游戏，或选择已有的游戏设置文件。"));
            Paths.RejectLinks(s.VariablesPath);
            if(new FileInfo(s.VariablesPath).Length>4*1024*1024)throw new IOException(UiText.T("设置文件过大，无法作为游戏语言配置读取。"));
            var bytes=File.ReadAllBytes(s.VariablesPath);
            Language.Change(bytes,s.CN.TextLocale,s.CN.SpeechLocale);
            Language.Change(bytes,s.Global.TextLocale,s.Global.SpeechLocale);
        });
        return s;
    }
}
public sealed class ConfigurationStore {
    readonly string stateRoot,legacyPath,mutexName;
    string expectedHash;
    Settings lastSettings;
    public string ConfigPath {get;}
    public ConfigurationStore(string stateRoot,string legacyPath=null,string mutexName="Local\\SC2DualRegionSwitcher") {
        this.stateRoot=Path.GetFullPath(stateRoot);this.legacyPath=legacyPath;this.mutexName=mutexName;ConfigPath=Path.Combine(this.stateRoot,"profiles.json");
    }
    static string Stamp(string path)=>File.Exists(path)?Files.HashFile(path):null;
    public ConfigurationLoadResult Load() {
        var defaults=ConfigurationValidator.Defaults();
        lastSettings=null;
        try{expectedHash=Stamp(ConfigPath);}catch(Exception error) when(error is IOException||error is UnauthorizedAccessException){return new ConfigurationLoadResult{Settings=defaults,NeedsSetup=true,NoticeKey="原有安装配置无法读取。请选择目录；保存时会保留原文件备份。"};}
        if(expectedHash!=null){
            try {
                var saved=Settings.Load(ConfigPath);lastSettings=ConfigurationValidator.Clone(saved);
                try{ConfigurationValidator.Validate(saved);return new ConfigurationLoadResult{Settings=saved};}
                catch(SettingsValidationException){return new ConfigurationLoadResult{Settings=saved,NeedsSetup=true,NoticeKey="已保存的安装信息未通过检查，请核对目录或等待更新完成。"};}
            }
            catch(Exception error) when(error is IOException||error is InvalidDataException||error is UnauthorizedAccessException||error is JsonException||error is ArgumentException||error is NotSupportedException){return new ConfigurationLoadResult{Settings=defaults,NeedsSetup=true,NoticeKey="原有安装配置无法读取。请选择目录；保存时会保留原文件备份。"};}
        }
        if(!String.IsNullOrEmpty(legacyPath)&&File.Exists(legacyPath)){
            Settings legacy=null;
            try {legacy=Settings.Load(legacyPath);lastSettings=ConfigurationValidator.Clone(legacy);var migrated=Save(legacy);return new ConfigurationLoadResult{Settings=migrated,Migrated=true};}
            catch(Exception error) when(error is IOException||error is InvalidDataException||error is UnauthorizedAccessException||error is JsonException||error is ArgumentException||error is NotSupportedException){return new ConfigurationLoadResult{Settings=legacy??defaults,NeedsSetup=true,NoticeKey="已读取旧版目录信息。请检查并保存到当前用户配置。"};}
        }
        return new ConfigurationLoadResult{Settings=defaults,NeedsSetup=true,NoticeKey="首次使用，请设置战网和两套游戏的安装目录。"};
    }
    static bool SamePaths(Settings a,Settings b)=>a!=null&&Paths.Same(a.BattleNetPath,b.BattleNetPath)&&Paths.Same(a.VariablesPath,b.VariablesPath)&&Paths.Same(a.CN.GamePath,b.CN.GamePath)&&Paths.Same(a.Global.GamePath,b.Global.GamePath);
    public Settings Save(Settings candidate) {
        using var mutex=new Mutex(false,mutexName);bool acquired=false;
        try {
            try{acquired=mutex.WaitOne(0);}catch(AbandonedMutexException){acquired=true;}
            if(!acquired)throw new IOException(UiText.T("另一个切换窗口正在操作，请稍后重试。"));
            var validated=ConfigurationValidator.Validate(candidate);
            if(Stamp(ConfigPath)!=expectedHash)throw new IOException(UiText.T("配置已被其他窗口修改。请重新打开程序后再保存。"));
            if(File.Exists(Path.Combine(stateRoot,"pending-language.json"))&&!SamePaths(lastSettings,validated))throw new IOException(UiText.T("存在未恢复的切换记录，请先按原配置完成恢复后再修改安装目录。"));
            if(expectedHash!=null&&lastSettings!=null&&JsonSerializer.Serialize(lastSettings)==JsonSerializer.Serialize(validated))return validated;
            if(expectedHash!=null){
                string backup=Path.Combine(stateRoot,"ConfigurationBackups","profiles-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N")+".json");
                Files.AtomicWrite(backup,File.ReadAllBytes(ConfigPath));
            }
            if(Stamp(ConfigPath)!=expectedHash)throw new IOException(UiText.T("配置已被其他窗口修改。请重新打开程序后再保存。"));
            Json.Write(ConfigPath,validated);
            expectedHash=Files.HashFile(ConfigPath);lastSettings=ConfigurationValidator.Clone(validated);
            return validated;
        }finally{if(acquired)mutex.ReleaseMutex();}
    }
}
