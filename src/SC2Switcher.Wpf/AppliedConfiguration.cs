using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sc2Switch2;

// A read-only observation, never a replacement for the engine's preflight checks.
public sealed class AppliedConfiguration {
    public string TextLocale { get; init; }
    public string SpeechLocale { get; init; }
    public bool NeedsRecovery { get; init; }
    public bool IsKnown => TextLocale != null && SpeechLocale != null;

    public bool Matches(string currentRegion, string targetRegion, string text, string speech) =>
        IsKnown && !NeedsRecovery && currentRegion == targetRegion && TextLocale == text && SpeechLocale == speech;

    public static AppliedConfiguration Read(string variables, string stateDirectory) {
        bool recovery = File.Exists(Path.Combine(stateDirectory, "pending-language.json"));
        try {
            Paths.RejectLinks(variables);
            if (new FileInfo(variables).Length > 4 * 1024 * 1024) return new() { NeedsRecovery = recovery };
            byte[] bytes = File.ReadAllBytes(variables);
            int skip = 0;
            Encoding encoding = new UTF8Encoding(false, true);
            if (bytes.Take(3).SequenceEqual(new byte[] { 239, 187, 191 })) { encoding = new UTF8Encoding(true, true); skip = 3; }
            else if (bytes.Take(2).SequenceEqual(new byte[] { 255, 254 })) { encoding = new UnicodeEncoding(false, true, true); skip = 2; }
            else if (bytes.Take(2).SequenceEqual(new byte[] { 254, 255 })) { encoding = new UnicodeEncoding(true, true, true); skip = 2; }
            string value = encoding.GetString(bytes, skip, bytes.Length - skip);
            string Locale(string key) {
                var matches = Regex.Matches(value, @"(?m)^" + key + @"=([^\r\n]*)");
                return matches.Count == 1 && Regex.IsMatch(matches[0].Groups[1].Value, @"^[a-z]{2}[A-Z]{2}$")
                    ? matches[0].Groups[1].Value : null;
            }
            return new() { TextLocale = Locale("localeiddata"), SpeechLocale = Locale("localeidassets"), NeedsRecovery = recovery };
        } catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException) {
            return new() { NeedsRecovery = recovery };
        }
    }
}
