using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Resources;
using System.Windows.Media;
using Sc2Switch2;

namespace Sc2Wpf;
internal static class FontDiagnostics {
    public static void Write(string path){
        var assembly=typeof(FontDiagnostics).Assembly;
        var root=new Uri("pack://application:,,,/SC2Switcher.Wpf;component/");
        using var stream=assembly.GetManifestResourceStream("SC2Switcher.Wpf.g.resources");
        using var reader=new ResourceReader(stream);
        var files=new List<object>();
        foreach(System.Collections.DictionaryEntry entry in reader){
            var key=(string)entry.Key;if(!key.EndsWith(".ttf"))continue;
            try{var face=new GlyphTypeface(new Uri(root,key));files.Add(new{File=key,Names=face.FamilyNames.Values.ToArray(),Win32Names=face.Win32FamilyNames.Values.ToArray(),Weight=face.Weight.ToOpenTypeWeight()});}
            catch(Exception error){files.Add(new{File=key,Error=error.ToString()});}
        }
        Json.Write(path,new{Files=files,Families=Fonts.GetFontFamilies(root,"./Fonts/").Select(f=>new{f.Source,Names=f.FamilyNames.Values.ToArray(),Faces=f.GetTypefaces().Select(t=>new{Weight=t.Weight.ToOpenTypeWeight(),Names=t.FaceNames.Values.ToArray()}).ToArray()}).ToArray()});
    }
}
