using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

class Program {
    static int R(double value)=>(int)Math.Round(value,MidpointRounding.AwayFromZero);
    static Brush B(string hex)=>(Brush)new BrushConverter().ConvertFromString(hex);
    static readonly Brush Blue=B("#2855DC"),Paper=B("#F8FAFF"),Ink=B("#18212D"),Muted=B("#626C78");
    static readonly int[] Sizes={16,20,24,32,40,48,64,96,128,256};
    static void Mark(DrawingContext dc,double size,bool hint) {
        double margin=hint?Math.Max(1,R(size/32)):size/32;
        double radius=hint?R((size-2*margin)*.225):(size-2*margin)*.225;
        double outer=hint?R(size*7/32):size*7/32;
        double stem=hint?Math.Max(2,R(size/8)):size/8;
        double gap=hint?Math.Max(1,R(size/16)):size/16;
        double inner=outer+stem,tip=size-inner-gap,end=tip-stem,bottom=size-outer;
        dc.DrawRoundedRectangle(Blue,null,new Rect(margin,margin,size-2*margin,size-2*margin),radius,radius);
        string data=FormattableString.Invariant($"M {outer},{outer} H {end} L {tip},{inner} H {inner} V {bottom} H {outer} Z");
        var shape=Geometry.Parse(data);
        dc.DrawGeometry(Paper,null,shape);
        dc.PushTransform(new RotateTransform(180,size/2,size/2));dc.DrawGeometry(Paper,null,shape);dc.Pop();
    }
    static RenderTargetBitmap Render(int size) {
        var visual=new DrawingVisual();using(var dc=visual.RenderOpen())Mark(dc,size,true);
        var bitmap=new RenderTargetBitmap(size,size,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    static byte[] Png(BitmapSource bitmap) {
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using var memory=new MemoryStream();encoder.Save(memory);return memory.ToArray();
    }
    static void Text(DrawingContext dc,string value,double x,double y,double size,Brush brush) {
        var text=new FormattedText(value,CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Segoe UI"),size,brush,1);dc.DrawText(text,new Point(x,y));
    }
    [STAThread] static void Main(string[] args) {
        string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);Directory.CreateDirectory(Path.Combine(root,"sizes"));
        var frames=Sizes.Select(size=>(size,bitmap:Render(size))).ToArray();
        var bytes=frames.Select(f=>Png(f.bitmap)).ToArray();
        for(int i=0;i<frames.Length;i++)File.WriteAllBytes(Path.Combine(root,"sizes","icon-"+frames[i].size+".png"),bytes[i]);
        using(var stream=File.Create(Path.Combine(root,"switcher.ico")))using(var writer=new BinaryWriter(stream)) {
            writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)frames.Length);
            uint offset=(uint)(6+16*frames.Length);
            for(int i=0;i<frames.Length;i++) {int size=frames[i].size;writer.Write((byte)(size==256?0:size));writer.Write((byte)(size==256?0:size));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write((uint)bytes[i].Length);writer.Write(offset);offset+=(uint)bytes[i].Length;}
            foreach(var png in bytes)writer.Write(png);
        }
        File.WriteAllText(Path.Combine(root,"icon.svg"),"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 64 64\" width=\"256\" height=\"256\"><rect x=\"2\" y=\"2\" width=\"60\" height=\"60\" rx=\"13.5\" fill=\"#2855DC\"/><g fill=\"#F8FAFF\"><path d=\"M14 14H30L38 22H22V50H14Z\"/><path d=\"M50 50H34L26 42H42V14H50Z\"/></g></svg>\n");
        var preview=new DrawingVisual();using(var dc=preview.RenderOpen()) {
            dc.DrawRectangle(B("#F8F9FB"),null,new Rect(0,0,1040,620));
            Text(dc,"双服切换 / 应用图标",36,26,22,Ink);Text(dc,"矢量原稿与像素对齐版本",36,59,14,Muted);
            dc.DrawRoundedRectangle(Brushes.White,new Pen(B("#E0E5ED"),1),new Rect(36,108,300,294),12,12);
            dc.PushTransform(new TranslateTransform(64,131));Mark(dc,244,false);dc.Pop();
            dc.DrawRoundedRectangle(B("#18212D"),null,new Rect(360,108,300,294),12,12);
            dc.PushTransform(new TranslateTransform(388,131));Mark(dc,244,false);dc.Pop();
            Text(dc,"构成",708,120,12,Muted);Text(dc,"两个等面积单元",708,150,22,Ink);Text(dc,"180° 旋转对称",708,188,16,Muted);Text(dc,"主色 #2855DC",708,219,16,Muted);Text(dc,"实心几何 · 45° 端部",708,250,16,Muted);
            Text(dc,"原始像素尺寸",36,441,12,Muted);
            double x=36;foreach(var f in frames.Where(f=>f.size<=128)) {dc.DrawImage(f.bitmap,new Rect(x,486+(128-f.size)/2.0,f.size,f.size));Text(dc,f.size.ToString(),x,466,12,Muted);x+=f.size+35;}
        }
        var board=new RenderTargetBitmap(1040,620,96,96,PixelFormats.Pbgra32);board.Render(preview);File.WriteAllBytes(Path.Combine(root,"图标预览.png"),Png(board));
        Console.WriteLine("Created SVG, PNG and ICO: "+String.Join(",",Sizes));
    }
}
