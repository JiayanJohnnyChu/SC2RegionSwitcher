using System.Collections.Generic;
using System.Windows.Markup;
using System.Windows.Media;

namespace Sc2Switch2;

public sealed record InterfaceLanguage(string Code, string Name) {
    public FontFamily Font => UiTypography.ForLanguage(Code);
}

public static class UiTypography {
    public static IReadOnlyList<InterfaceLanguage> Languages { get; } = new[] {
        new InterfaceLanguage("en-US", "English"), new("zh-CN", "简体中文"),
        new("fr-FR", "Français"), new("de-DE", "Deutsch"), new("nl-NL", "Nederlands"),
        new("ko-KR", "한국어"), new("it-IT", "Italiano"), new("es-ES", "Español"),
        new("pt-PT", "Português"), new("la", "Latina"), new("el-GR", "Ελληνικά")
    };
    const string Prefix = "./Fonts/#";
    static System.Uri BaseUri=>new("pack://application:,,,/SC2Switcher.Wpf;component/");
    public static FontFamily Latin => new(BaseUri,Prefix + "Switcher Sans");
    public static FontFamily Path => new(BaseUri,"Consolas, " + Prefix + "Switcher Han, " + Prefix + "Switcher Hangul, Segoe UI");
    public static FontFamily DisplayForLanguage(string language) => new(BaseUri,Prefix+(language switch {"zh-CN"=>"Switcher Han Display","ko-KR"=>"Switcher Hangul Display",_=>"Switcher Display"}));
    public static FontFamily ForLanguage(string language) => new(BaseUri,Prefix + (language switch {
        "zh-CN" => "Switcher Han", "ko-KR" => "Switcher Hangul", _ => "Switcher Sans"
    }) + ", " + Prefix + "Switcher Han, " + Prefix + "Switcher Hangul, Segoe UI");
    public static XmlLanguage CurrentLanguage => XmlLanguage.GetLanguage(UiText.Language);
}
