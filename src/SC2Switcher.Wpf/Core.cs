using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;

namespace Sc2Switch2 {
public sealed class Profile {
    public string Name { get; set; }
    public string GamePath { get; set; }
    public string TextLocale { get; set; }
    public string SpeechLocale { get; set; }
}
public sealed class Settings {
    public int SchemaVersion { get; set; }
    public string BattleNetPath { get; set; }
    public string VariablesPath { get; set; }
    public string GlobalRegion { get; set; }
    public Profile CN { get; set; }
    public Profile Global { get; set; }
    public static Settings Load(string path) {
        Settings s = Json.Read<Settings>(path);
        if(s == null || s.SchemaVersion != 2 || s.CN == null || s.Global == null) throw new InvalidDataException(UiText.T("配置版本不受支持，请检查 profiles.json。"));
        if(!new [] {"EU","US","KR"}.Contains(s.GlobalRegion)) throw new InvalidDataException(UiText.T("外服登录区域无效。"));
        Paths.RequireSeparate(s.CN.GamePath,s.Global.GamePath);
        Paths.Normal(s.BattleNetPath); Paths.Normal(s.VariablesPath);
        foreach(Profile p in new [] {s.CN,s.Global}) { Language.Validate(p.TextLocale); Language.Validate(p.SpeechLocale); }
        return s;
    }
}
public static class Json {
    static readonly JsonSerializerOptions Options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
    public static T Read<T>(string path) { return JsonSerializer.Deserialize<T>(File.ReadAllText(path,Encoding.UTF8),Options); }
    public static void Write(string path, object value) { Files.AtomicWrite(path,JsonSerializer.SerializeToUtf8Bytes(value,value.GetType(),Options)); }
}
public static class Files {
    public static string Hash(byte[] bytes) { using(var sha=SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-",""); }
    public static string HashFile(string path) { return Hash(File.ReadAllBytes(path)); }
    public static void AtomicWrite(string path, byte[] bytes) {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try {
            using(var fs=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)) { fs.Write(bytes,0,bytes.Length); fs.Flush(true); }
            if(File.Exists(path)) File.Replace(temp,path,null); else File.Move(temp,path);
        } finally { if(File.Exists(temp)) File.Delete(temp); }
    }
}
public static class Paths {
    public static string Normal(string path) {
        if(String.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path)) throw new InvalidDataException(UiText.T("需要完整的本机路径。"));
        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);
    }
    public static bool Same(string a,string b) { return String.Equals(Normal(a),Normal(b),StringComparison.OrdinalIgnoreCase); }
    public static bool Under(string a,string root) { return Normal(a).StartsWith(Normal(root)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase); }
    public static void RequireSeparate(string a,string b) {
        if(Same(a,b) || Under(a,b) || Under(b,a)) throw new InvalidDataException(UiText.T("国服与外服目录不能相同或互相包含。"));
    }
    public static void RejectLinks(string path) {
        string cursor=Normal(path);
        while(!String.IsNullOrEmpty(cursor)) {
            if((File.Exists(cursor)||Directory.Exists(cursor)) && (File.GetAttributes(cursor)&FileAttributes.ReparsePoint)!=0) throw new IOException(UiText.T("路径经过链接或重定向，已停止：")+cursor);
            cursor=Path.GetDirectoryName(cursor);
        }
    }
}
public sealed class BuildInfo {
    public string Branch {get;set;}
    public string Version {get;set;}
    public string Tags {get;set;}
    public string Fingerprint {get;set;}
    public static BuildInfo Parse(string text) {
        string[] lines=text.Split(new [] {"\r\n","\n"},StringSplitOptions.RemoveEmptyEntries);
        if(lines.Length<2) throw new InvalidDataException(UiText.T("版本清单不完整。"));
        string[] columns=lines[0].Split('|').Select(x=>x.Split('!')[0]).ToArray();
        if(columns.Distinct().Count()!=columns.Length) throw new InvalidDataException(UiText.T("版本清单包含重复字段。"));
        string[] required={"Branch","Active","Version","Tags"};
        foreach(string key in required) if(!columns.Contains(key)) throw new InvalidDataException(UiText.T("版本清单格式已变化，缺少 ")+key+"。");
        var active=new List<BuildInfo>();
        foreach(string line in lines.Skip(1)) {
            string[] cells=line.Split('|');
            if(cells.Length!=columns.Length) throw new InvalidDataException(UiText.T("版本清单列数异常。"));
            if(cells[Array.IndexOf(columns,"Active")]!="1") continue;
            var b=new BuildInfo {Branch=cells[Array.IndexOf(columns,"Branch")].ToLowerInvariant(),Version=cells[Array.IndexOf(columns,"Version")],Tags=cells[Array.IndexOf(columns,"Tags")]};
            if(!Regex.IsMatch(b.Version,@"^\d+\.\d+\.\d+\.\d+$")) throw new InvalidDataException(UiText.T("无法识别当前游戏版本。"));
            active.Add(b);
        }
        if(active.Count!=1) throw new InvalidDataException(UiText.T("版本清单没有唯一的当前版本，可能仍在更新。"));
        active[0].Fingerprint=Files.Hash(Encoding.UTF8.GetBytes(text));
        return active[0];
    }
    public static BuildInfo Load(Profile p,bool cn) {
        Paths.RejectLinks(p.GamePath);
        foreach(string part in new [] {"SC2Data","Versions","Support","Support64",".build.info",".patch.result"}) Paths.RejectLinks(Path.Combine(p.GamePath,part));
        string manifest=Path.Combine(p.GamePath,".build.info"), patch=Path.Combine(p.GamePath,".patch.result");
        if(!File.Exists(manifest)) throw new IOException(UiText.T(p.Name)+UiText.T("缺少版本清单。请在战网中完成安装。"));
        if(!File.Exists(patch) || File.ReadAllText(patch).Trim()!="0") throw new IOException(UiText.T(p.Name)+UiText.T("缺少成功完成更新的标记。请先让战网完成更新或修复。"));
        if(!File.Exists(Path.Combine(p.GamePath,"StarCraft II.exe")) || !Directory.Exists(Path.Combine(p.GamePath,"Versions"))) throw new IOException(UiText.T(p.Name)+UiText.T("安装文件不完整。"));
        BuildInfo b=Parse(File.ReadAllText(manifest,Encoding.UTF8));
        if(cn ? b.Branch!="cn" : !new [] {"eu","us","kr","tw"}.Contains(b.Branch)) throw new IOException(UiText.T(p.Name)+UiText.T("目录中的版本分支不正确。"));
        if(!Regex.IsMatch(b.Tags,@"\b"+Regex.Escape(p.TextLocale)+@"\s+text(?:\?|\b)") || !Regex.IsMatch(b.Tags,@"\b"+Regex.Escape(p.SpeechLocale)+@"\s+speech(?:\?|\b)")) throw new IOException(UiText.T(p.Name)+UiText.T("的版本清单未声明已安装所选语言。请检查战网语言设置。"));
        return b;
    }
}
public sealed class ActivityGuard : IDisposable {
    readonly List<FileSystemWatcher> watchers=new List<FileSystemWatcher>();
    int changed;
    public ActivityGuard(IEnumerable<string> paths) {
        try {
            foreach(string path in paths) {
                var watcher=new FileSystemWatcher(path); watcher.IncludeSubdirectories=true;
                watcher.NotifyFilter=NotifyFilters.LastWrite|NotifyFilters.Size|NotifyFilters.FileName|NotifyFilters.DirectoryName;
                watcher.Changed+=OnChange; watcher.Created+=OnChange; watcher.Deleted+=OnChange; watcher.Renamed+=OnChange;
                watcher.Error+=(s,e)=>Interlocked.Exchange(ref changed,1);
                watcher.EnableRaisingEvents=true; watchers.Add(watcher);
            }
        } catch {Dispose();throw;}
    }
    void OnChange(object s,FileSystemEventArgs e) {Interlocked.Exchange(ref changed,1);}
    public void Check() {if(Volatile.Read(ref changed)!=0) throw new IOException(UiText.T("检测到游戏文件变化或监测中断。更新、安装或修复结束后再切换。"));}
    public void Dispose() {foreach(var w in watchers) w.Dispose();watchers.Clear();}
}
public static class Language {
    public static void Validate(string locale) {if(locale==null || !Regex.IsMatch(locale,@"^[a-z]{2}[A-Z]{2}$")) throw new InvalidDataException(UiText.T("语言代码无效。"));}
    public static byte[] Change(byte[] source,string textLocale,string speechLocale) {
        Validate(textLocale);Validate(speechLocale);
        int skip=0; Encoding encoding=new UTF8Encoding(false,true);
        if(source.Length>=3 && source[0]==239 && source[1]==187 && source[2]==191) {encoding=new UTF8Encoding(true,true);skip=3;}
        else if(source.Length>=2 && source[0]==255 && source[1]==254) {encoding=new UnicodeEncoding(false,true,true);skip=2;}
        else if(source.Length>=2 && source[0]==254 && source[1]==255) {encoding=new UnicodeEncoding(true,true,true);skip=2;}
        string value=encoding.GetString(source,skip,source.Length-skip);
        foreach(var item in new [] {new KeyValuePair<string,string>("localeidassets",speechLocale),new KeyValuePair<string,string>("localeiddata",textLocale)}) {
            string pattern=@"(?m)^"+item.Key+@"=[^\r\n]*";
            if(Regex.Matches(value,pattern).Count!=1) throw new InvalidDataException(UiText.T("共享设置中的 ")+item.Key+UiText.T(" 缺失或重复，已停止修改。"));
            value=Regex.Replace(value,pattern,item.Key+"="+item.Value);
        }
        return encoding.GetPreamble().Concat(encoding.GetBytes(value)).ToArray();
    }
}
public sealed class Journal {
    public string VariablesPath {get;set;}
    public string BackupPath {get;set;}
    public string OriginalHash {get;set;}
    public string AppliedHash {get;set;}
    public string Target {get;set;}
    public string Stage {get;set;}
}
public sealed class LanguageTransaction {
    readonly string journalPath;
    readonly string variables;
    public Journal Entry {get;private set;}
    public bool Changed {get;private set;}
    public LanguageTransaction(string stateRoot,string vars) {journalPath=Path.Combine(stateRoot,"pending-language.json");variables=vars;}
    public void Apply(string target,Profile profile,Action finalCheck) {
        if(File.Exists(journalPath)) throw new IOException(UiText.T("上次切换尚未恢复，已停止新的语言修改。"));
        Paths.RejectLinks(variables);
        byte[] old=File.ReadAllBytes(variables), updated=Language.Change(old,profile.TextLocale,profile.SpeechLocale);
        if(old.SequenceEqual(updated)) return;
        string backup=Path.Combine(Path.GetDirectoryName(journalPath),"Backups","Variables-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+"-"+Guid.NewGuid().ToString("N").Substring(0,6)+".txt");
        Files.AtomicWrite(backup,old);
        Entry=new Journal {VariablesPath=variables,BackupPath=backup,OriginalHash=Files.Hash(old),AppliedHash=Files.Hash(updated),Target=target,Stage="prepared"};
        Json.Write(journalPath,Entry);
        finalCheck();
        if(Files.HashFile(variables)!=Entry.OriginalHash) throw new IOException(UiText.T("共享设置刚被其他程序修改，已停止。"));
        Files.AtomicWrite(variables,updated);Changed=true;
        Entry.Stage="applied";Json.Write(journalPath,Entry);
    }
    public void Commit() {if(File.Exists(journalPath)) File.Delete(journalPath);}
    public void Rollback(Action safetyCheck) {Recover(Path.GetDirectoryName(journalPath),variables,safetyCheck);}
    public static bool Recover(string stateRoot,string expectedPath,Action safetyCheck) {
        string jp=Path.Combine(stateRoot,"pending-language.json");
        if(!File.Exists(jp)) return false;
        Journal j=Json.Read<Journal>(jp);
        if(j==null || !Paths.Same(j.VariablesPath,expectedPath) || !Paths.Under(j.BackupPath,Path.Combine(stateRoot,"Backups"))) throw new IOException(UiText.T("待恢复记录路径异常，已停止自动恢复。"));
        safetyCheck();Paths.RejectLinks(expectedPath);Paths.RejectLinks(j.BackupPath);
        byte[] backup=File.ReadAllBytes(j.BackupPath);
        if(Files.Hash(backup)!=j.OriginalHash) throw new IOException(UiText.T("语言备份校验失败，已停止自动恢复。"));
        string current=Files.HashFile(expectedPath);
        if(current==j.AppliedHash) {safetyCheck();Files.AtomicWrite(expectedPath,backup);}
        else if(current!=j.OriginalHash) throw new IOException(UiText.T("设置在切换中断后又发生变化，已保留备份并停止自动恢复，避免覆盖新设置。"));
        File.Delete(jp);return true;
    }
}
public interface IPlatform {
    bool GameRunning();
    bool BattleNetRunning();
    void RequestExit();
    void StartRegion(string region);
    string ReadRegion();
    Task Delay(int milliseconds,CancellationToken token);
}
public sealed class NativePlatform : IPlatform {
    readonly string executable;
    public NativePlatform(string exe) {executable=exe;}
    public bool GameRunning() {
        var names=new HashSet<string>(new [] {"SC2","SC2_x64","SC2Switcher","SC2Switcher_x64","SC2Editor","SC2Editor_x64","StarCraft II","StarCraft II Editor","StarCraft II Editor_x64"},StringComparer.OrdinalIgnoreCase);
        foreach(var p in Process.GetProcesses()) {using(p) {try {if(names.Contains(p.ProcessName)) return true;}catch(InvalidOperationException){}}}return false;
    }
    public bool BattleNetRunning() {var ps=Process.GetProcessesByName("Battle.net");bool any=ps.Length>0;foreach(var p in ps)p.Dispose();return any;}
    public void RequestExit() {using(Process.Start(new ProcessStartInfo(executable,"--exec=\"shutdown\""){UseShellExecute=false,WorkingDirectory=Path.GetDirectoryName(executable)})) {}}
    public void StartRegion(string region) {if(!new [] {"CN","EU","US","KR"}.Contains(region))throw new ArgumentException(UiText.T("区域无效"));using(Process.Start(new ProcessStartInfo(executable,"--setregion="+region){UseShellExecute=false,WorkingDirectory=Path.GetDirectoryName(executable)})) {}}
    public string ReadRegion() {
        string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"Battle.net","Battle.net.config");
        try {
            return ParseRegion(File.ReadAllText(path));
        } catch(IOException){return null;}catch(UnauthorizedAccessException){return null;}catch(JsonException){return null;}catch(InvalidOperationException){return null;}
    }
    public static string ParseRegion(string json) {
        using(var document=JsonDocument.Parse(json)) {
            if(document.RootElement.ValueKind!=JsonValueKind.Object)return null;
            var regions=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var group in document.RootElement.EnumerateObject()) {
                JsonElement services,region;
                if(group.Value.ValueKind!=JsonValueKind.Object || !group.Value.TryGetProperty("Services",out services) || services.ValueKind!=JsonValueKind.Object)continue;
                if(services.TryGetProperty("LastLoginRegion",out region) && region.ValueKind==JsonValueKind.String)regions.Add(region.GetString().ToUpperInvariant());
            }
            return regions.Count==1?regions.First():null;
        }
    }
    public Task Delay(int milliseconds,CancellationToken token) {return Task.Delay(milliseconds,token);}
}
public sealed class SwitchResult {
    public string Target {get;set;}
    public string Region {get;set;}
    public string Version {get;set;}
    public string TextLocale {get;set;}
    public string SpeechLocale {get;set;}
    public string Backup {get;set;}
    public string ConfirmedAt {get;set;}
    public string BattleNetVersion {get;set;}
    public string Meaning {get;set;}
}
public enum SwitchPhase { Checking, StoppingBattleNet, Rechecking, SettingLanguage, StartingBattleNet, Completed }
public sealed class Engine {
    readonly Settings settings; readonly string stateRoot; readonly IPlatform platform;
    public Engine(Settings settings,string stateRoot,IPlatform platform) {this.settings=settings;this.stateRoot=stateRoot;this.platform=platform;}
    public void Safe() {if(platform.GameRunning())throw new IOException(UiText.T("请先退出星际争霸 II 和地图编辑器，再切换。"));}
    public BuildInfo[] Inspect() {
        Paths.RequireSeparate(settings.CN.GamePath,settings.Global.GamePath);
        if(!File.Exists(settings.BattleNetPath))throw new IOException(UiText.T("找不到官方战网程序。"));
        return new [] {BuildInfo.Load(settings.CN,true),BuildInfo.Load(settings.Global,false)};
    }
    public async Task<SwitchResult> Switch(string target,Action<string> report,CancellationToken token,Action<SwitchPhase> phaseChanged=null) {
        token.ThrowIfCancellationRequested();
        if(target!="CN"&&target!="Global")throw new ArgumentException(UiText.T("切换目标无效"));
        var transaction=new LanguageTransaction(stateRoot,settings.VariablesPath);
        Safe();
        if(LanguageTransaction.Recover(stateRoot,settings.VariablesPath,Safe)) report(UiText.T("已恢复上次未完成的语言修改。"));
        Inspect();
        Profile selected=target=="CN"?settings.CN:settings.Global;
        string region=target=="CN"?"CN":settings.GlobalRegion;
        using(var guard=new ActivityGuard(new [] {settings.CN.GamePath,settings.Global.GamePath})) {
            phaseChanged?.Invoke(SwitchPhase.Checking);
            report(UiText.T("正在检查安装完成标记与文件活动……"));
            await platform.Delay(3000,token);guard.Check();Safe();Inspect();
            if(platform.BattleNetRunning()) {
                phaseChanged?.Invoke(SwitchPhase.StoppingBattleNet);
                report(UiText.T("正在正常退出战网。若战网显示下载确认，请先完成更新。"));
                platform.RequestExit();
                for(int n=0;n<60 && platform.BattleNetRunning();n++) {await platform.Delay(500,token);Safe();guard.Check();}
                if(platform.BattleNetRunning()) throw new IOException(UiText.T("战网尚未退出。请在其菜单中正常退出后再试；程序没有强制结束进程。"));
            }
            phaseChanged?.Invoke(SwitchPhase.Rechecking);
            report(UiText.T("正在复查更新状态……"));
            await platform.Delay(2000,token);guard.Check();Safe();var builds=Inspect();
            try {
                phaseChanged?.Invoke(SwitchPhase.SettingLanguage);
                report(UiText.T("正在同步目标语言并记录可回滚备份……"));
                transaction.Apply(target,selected,()=>{token.ThrowIfCancellationRequested();Safe();guard.Check();Inspect();});
                token.ThrowIfCancellationRequested();Safe();guard.Check();
                // The launch request cannot be recalled. From this boundary onwards,
                // finish region verification instead of reporting a cancelled switch.
                phaseChanged?.Invoke(SwitchPhase.StartingBattleNet);
                token.ThrowIfCancellationRequested();
                report(UiText.T("正在打开 ")+region+UiText.T(" 战网并核对区域记录……"));
                platform.StartRegion(region);
                bool confirmed=false;
                for(int n=0;n<60;n++) {
                    await platform.Delay(500,CancellationToken.None);
                    if(platform.BattleNetRunning() && platform.ReadRegion()==region) {confirmed=true;break;}
                }
                if(!confirmed)throw new IOException(UiText.T("战网已收到启动请求，但未能确认区域。当前战网版本可能需要适配；请检查登录页。"));
                var result=new SwitchResult {Target=target,Region=region,Version=builds[target=="CN"?0:1].Version,TextLocale=selected.TextLocale,SpeechLocale=selected.SpeechLocale,Backup=transaction.Entry==null?null:transaction.Entry.BackupPath,ConfirmedAt=DateTime.Now.ToString("o"),BattleNetVersion=FileVersionInfo.GetVersionInfo(settings.BattleNetPath).FileVersion,Meaning=UiText.T("区域配置已核对；账号认证与游戏启动由战网完成。")};
                Json.Write(Path.Combine(stateRoot,"last-success.json"),result);transaction.Commit();
                phaseChanged?.Invoke(SwitchPhase.Completed);
                report(UiText.T("已打开 ")+region+UiText.T(" 战网。请完成登录，并在星际争霸 II 页面点击开始游戏。"));return result;
            } catch(Exception error) {
                try {transaction.Rollback(Safe);} catch(Exception recovery) {throw new IOException(error.Message+"\r\n"+recovery.Message);}
                throw;
            }
        }
    }
}
}
