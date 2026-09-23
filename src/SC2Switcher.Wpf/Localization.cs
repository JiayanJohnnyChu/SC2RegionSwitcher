using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Sc2Switch2;

// Interface language is independent of the game profiles and locale settings.
public sealed class UiText : INotifyPropertyChanged {
    static readonly Dictionary<string,Dictionary<string,string>> catalogs =
        new[]{"en-US","zh-CN","fr-FR","de-DE","nl-NL","ko-KR","it-IT","es-ES","pt-PT","la","el-GR"}
        .ToDictionary(language=>language,Read);
    public static UiText Instance { get; } = new();
    public static string Language { get; private set; } = "en-US";
    public static IReadOnlyCollection<string> Keys => catalogs["en-US"].Keys;
    public static IReadOnlyCollection<string> KeysFor(string language)=>catalogs[language].Keys;
    public event PropertyChangedEventHandler PropertyChanged;
    public string this[string key] => T(key);
    static Dictionary<string,string> Read(string language) {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Sc2Switch2.Strings."+language+".json");
        if(stream==null)throw new InvalidOperationException("Missing interface language resources: "+language);
        return JsonSerializer.Deserialize<Dictionary<string,string>>(stream);
    }
    public static bool IsSupported(string language)=>language!=null&&catalogs.ContainsKey(language);
    public static void SetLanguage(string language) {
        if(!IsSupported(language))throw new ArgumentException("Unsupported interface language.",nameof(language));
        if(Language==language)return;
        Language=language;
        Instance.PropertyChanged?.Invoke(Instance,new PropertyChangedEventArgs("Item[]"));
    }
    public static string Get(string language,string key) {
        if(key==null)return "";
        if(catalogs.TryGetValue(language,out var catalog)&&catalog.TryGetValue(key,out var text))return text;
        return catalogs["en-US"].TryGetValue(key,out var fallback)?fallback:key;
    }
    public static string T(string key)=>Get(Language,key);
    // Exceptions can arrive with an already translated application message.
    // Retain an unambiguous catalogue key so the view can change languages;
    // unknown external diagnostics remain exactly as reported.
    public static string MessageKey(string message){
        if(message==null)return null;
        var keys=catalogs[Language].Where(entry=>entry.Value==message).Select(entry=>entry.Key).Take(2).ToArray();
        return keys.Length==1?keys[0]:message;
    }
    public static string Format(string key,params object[] arguments)=>String.Format(CultureInfo.GetCultureInfo(Language),T(key),arguments);
}

public static class UiPreferences {
    sealed class Document { public Document(){} public string Language {get;set;} }
    public static string Load(string path) {
        try {var value=Json.Read<Document>(path)?.Language;return UiText.IsSupported(value)?value:"en-US";}
        catch(IOException){return "en-US";}catch(UnauthorizedAccessException){return "en-US";}catch(JsonException){return "en-US";}
    }
    public static void Save(string path,string language) {
        if(!UiText.IsSupported(language))throw new ArgumentException("Unsupported interface language.");
        Json.Write(path,new Document{Language=language});
    }
}
