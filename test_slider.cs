using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

class Test {
    public static void Main() {
        Style s = new Style(typeof(Slider));
        ControlTemplate t = new ControlTemplate(typeof(Slider));
        FrameworkElementFactory root = new FrameworkElementFactory(typeof(Grid));
        FrameworkElementFactory trackBorder = new FrameworkElementFactory(typeof(Border));
        trackBorder.SetValue(Border.HeightProperty, 4.0);
        trackBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(2));
        trackBorder.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(40, 40, 50)));
        trackBorder.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);
        root.AppendChild(trackBorder);

        FrameworkElementFactory track = new FrameworkElementFactory(typeof(Track), "PART_Track");
        FrameworkElementFactory thumb = new FrameworkElementFactory(typeof(Thumb));
        ControlTemplate thumbT = new ControlTemplate(typeof(Thumb));
        FrameworkElementFactory thumbBorder = new FrameworkElementFactory(typeof(Border));
        thumbBorder.SetValue(Border.WidthProperty, 16.0);
        thumbBorder.SetValue(Border.HeightProperty, 16.0);
        thumbBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        thumbBorder.SetValue(Border.BackgroundProperty, Brushes.White);
        thumbBorder.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(59, 130, 246)));
        thumbBorder.SetValue(Border.BorderThicknessProperty, new Thickness(2));
        thumbT.VisualTree = thumbBorder;
        thumb.SetValue(Thumb.TemplateProperty, thumbT);
        track.SetValue(Track.ThumbProperty, thumb);

        root.AppendChild(track);
        t.VisualTree = root;
        s.Setters.Add(new Setter(Slider.TemplateProperty, t));
        Console.WriteLine("Slider Style created successfully!");
    }
}
