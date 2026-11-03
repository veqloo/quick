using System;
using System.Windows;
using System.Windows.Markup;

class Test2 {
    [STAThread]
    public static void Main() {
        string xaml = @""<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>
            <Setter Property='Height' Value='34'/>
        </Style>"";
        Style s = (Style)XamlReader.Parse(xaml);
        Console.WriteLine(""Style TargetType: "" + s.TargetType.Name);
    }
}
