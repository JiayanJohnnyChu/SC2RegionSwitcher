using System;
using System.Globalization;
using Sc2Switch2;

namespace Sc2Switch2;

public sealed class GameLanguageOption {
    public string Code {get;}
    public string Label=>GameLocales.DisplayName(Code);
    public GameLanguageOption(string code){Code=code;}
}

public static class GameLocales {
    public static string DisplayName(string code) {
        if(String.IsNullOrEmpty(code))return "";
        // Native names and stable locale codes, matching the design's labels.
        string name=code switch {"enUS"=>"English", "deDE"=>"Deutsch", "frFR"=>"français", "esES"=>"español", "esMX"=>"español (Latinoamérica)", "itIT"=>"italiano", "ptBR"=>"português (Brasil)", "koKR"=>"한국어", "zhCN"=>"简体中文", "zhTW"=>"中文（繁體）", "plPL"=>"polski", "ruRU"=>"русский", _=>null};
        if(name!=null)return name+" ("+code+")";
        try {
            Language.Validate(code);
            var culture=CultureInfo.GetCultureInfo(code.Insert(2,"-"),predefinedOnly:true);
            return culture.NativeName+" ("+code+")";
        }catch(ArgumentException){return code;}catch(System.IO.InvalidDataException){return code;}
    }
    public static string Describe(string text,string speech)=>text==speech?DisplayName(text):UiText.Format("文字：{0}；语音：{1}",DisplayName(text),DisplayName(speech));
}
