using System.Windows;
using System.Windows.Controls;

namespace Sc2Wpf;

// One radio control and one text tree at every breakpoint.
public sealed class InstallationChoice : RadioButton {
    public static readonly DependencyProperty LabelProperty=DependencyProperty.Register(nameof(Label),typeof(string),typeof(InstallationChoice));
    public static readonly DependencyProperty NumberProperty=DependencyProperty.Register(nameof(Number),typeof(string),typeof(InstallationChoice));
    public static readonly DependencyProperty RegionCodeProperty=DependencyProperty.Register(nameof(RegionCode),typeof(string),typeof(InstallationChoice));
    public static readonly DependencyProperty AvailabilityProperty=DependencyProperty.Register(nameof(Availability),typeof(string),typeof(InstallationChoice));
    public static readonly DependencyProperty IsAvailableProperty=DependencyProperty.Register(nameof(IsAvailable),typeof(bool),typeof(InstallationChoice),new PropertyMetadata(false));
    public static readonly DependencyProperty IsCompactProperty=DependencyProperty.Register(nameof(IsCompact),typeof(bool),typeof(InstallationChoice),new PropertyMetadata(false));
    public static readonly DependencyProperty LabelSizeProperty=DependencyProperty.Register(nameof(LabelSize),typeof(double),typeof(InstallationChoice),new PropertyMetadata(52.0));
    public string Label{get=>(string)GetValue(LabelProperty);set=>SetValue(LabelProperty,value);}
    public string Number{get=>(string)GetValue(NumberProperty);set=>SetValue(NumberProperty,value);}
    public string RegionCode{get=>(string)GetValue(RegionCodeProperty);set=>SetValue(RegionCodeProperty,value);}
    public string Availability{get=>(string)GetValue(AvailabilityProperty);set=>SetValue(AvailabilityProperty,value);}
    public bool IsAvailable{get=>(bool)GetValue(IsAvailableProperty);set=>SetValue(IsAvailableProperty,value);}
    public bool IsCompact{get=>(bool)GetValue(IsCompactProperty);set=>SetValue(IsCompactProperty,value);}
    public double LabelSize{get=>(double)GetValue(LabelSizeProperty);set=>SetValue(LabelSizeProperty,value);}
}
