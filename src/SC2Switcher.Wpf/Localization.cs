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
    static readonly Dictionary<string,Dictionary<string,string>> catalogs = new() {
        ["zh-CN"] = Read("zh-CN"), ["en-US"] = Read("en-US")
    };
    public static UiText Instance { get; } = new();
    public static string Language { get; private set; } = "zh-CN";
    public static IReadOnlyCollection<string> Keys => catalogs["zh-CN"].Keys;
    public event PropertyChangedEventHandler PropertyChanged;
    public string this[string key] => T(key);
    static Dictionary<string,string> Read(string language) {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("Sc2Switch2.Strings."+language+".json");
        if(stream==null)throw new InvalidOperationException("Missing interface language resources: "+language);
        return JsonSerializer.Deserialize<Dictionary<string,string>>(stream);
    }
    public static bool IsSupported(string language)=>language=="zh-CN"||language=="en-US";
    public static void SetLanguage(string language) {
        if(!IsSupported(language))throw new ArgumentException("Unsupported interface language.",nameof(language));
        if(Language==language)return;
        Language=language;
        Instance.PropertyChanged?.Invoke(Instance,new PropertyChangedEventArgs("Item[]"));
    }
    public static string Get(string language,string key) {
        if(key==null)return "";
        return catalogs.TryGetValue(language,out var catalog)&&catalog.TryGetValue(key,out var text)?text:key;
    }
    public static string T(string key)=>Get(Language,key);
    public static string Format(string key,params object[] arguments)=>String.Format(CultureInfo.GetCultureInfo(Language),T(key),arguments);
}

public static class UiPreferences {
    sealed class Document { public Document(){} public string Language {get;set;} }
    public static string Load(string path) {
        try {var value=Json.Read<Document>(path)?.Language;return UiText.IsSupported(value)?value:"zh-CN";}
        catch(IOException){return "zh-CN";}catch(UnauthorizedAccessException){return "zh-CN";}catch(JsonException){return "zh-CN";}
    }
    public static void Save(string path,string language) {
        if(!UiText.IsSupported(language))throw new ArgumentException("Unsupported interface language.");
        Json.Write(path,new Document{Language=language});
    }
}
