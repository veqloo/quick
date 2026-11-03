using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Microsoft.Win32;
using ShapePath = System.Windows.Shapes.Path;

namespace VeqloQuick
{
    public static class WpfTheme
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWCP_ROUND = 2;

        public static bool IsDarkMode = true;

        // Semantic Color Tokens (ibelick / ui-skills design tokens)
        public static Color ColorBackground;
        public static Color ColorSurface;
        public static Color ColorSurfaceHover;
        public static Color ColorSurfaceSelected;
        public static Color ColorBorder;
        public static Color ColorBorderHover;
        public static Color ColorBorderSelected;
        public static Color ColorTextPrimary;
        public static Color ColorTextSecondary;
        public static Color ColorTextMuted;
        public static Color ColorAccent;
        public static Color ColorAccentHover;
        public static Color ColorAccentPressed;
        public static Color ColorAccentMuted;
        public static Color ColorSuccess;
        public static Color ColorError;
        public static Color ColorErrorHover;

        // Solid Color Brushes
        public static SolidColorBrush BrushBackground;
        public static SolidColorBrush BrushSurface;
        public static SolidColorBrush BrushSurfaceHover;
        public static SolidColorBrush BrushSurfaceSelected;
        public static SolidColorBrush BrushBorder;
        public static SolidColorBrush BrushBorderHover;
        public static SolidColorBrush BrushBorderSelected;
        public static SolidColorBrush BrushTextPrimary;
        public static SolidColorBrush BrushTextSecondary;
        public static SolidColorBrush BrushTextMuted;
        public static SolidColorBrush BrushAccent;
        public static SolidColorBrush BrushAccentHover;
        public static SolidColorBrush BrushAccentPressed;
        public static SolidColorBrush BrushAccentMuted;
        public static SolidColorBrush BrushSuccess;
        public static SolidColorBrush BrushError;
        public static SolidColorBrush BrushErrorHover;

        // Typography Fonts (Windows 11 Fluent / Segoe UI Variable font stack)
        public static FontFamily FontDisplay;
        public static FontFamily FontText;
        public static FontFamily FontSmall;
        public static FontFamily FontMono;

        // Custom Component Styles (no default WPF chrome)
        public static Style PrimaryButtonStyle;
        public static Style SecondaryButtonStyle;
        public static Style GhostButtonStyle;
        public static Style DangerButtonStyle;
        public static Style ModernCheckBoxStyle;
        public static Style ModernTextBoxStyle;

        static WpfTheme()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AppsUseLightTheme");
                        if (val is int)
                        {
                            IsDarkMode = ((int)val == 0);
                        }
                    }
                }
            }
            catch
            {
                IsDarkMode = true;
            }

            if (IsDarkMode)
            {
                // UI Skills / Obsidian Deep Neutral Theme
                ColorBackground = Color.FromRgb(11, 11, 14);        // Deep Obsidian #0B0B0E
                ColorSurface = Color.FromRgb(21, 21, 26);           // Elevated Slate #15151A
                ColorSurfaceHover = Color.FromRgb(28, 28, 36);      // Hover Surface #1C1C24
                ColorSurfaceSelected = Color.FromRgb(20, 28, 46);   // Active Selected Slate #141C2E
                ColorBorder = Color.FromRgb(37, 37, 46);           // Hairline 1px border #25252E
                ColorBorderHover = Color.FromRgb(56, 56, 70);      // Hover border #383846
                ColorBorderSelected = Color.FromRgb(59, 130, 246); // Electric Blue #3B82F6
                ColorTextPrimary = Color.FromRgb(244, 244, 246);    // Crisp Near-White #F4F4F6
                ColorTextSecondary = Color.FromRgb(142, 142, 155);  // Refined Neutral #8E8E9B
                ColorTextMuted = Color.FromRgb(90, 90, 102);        // Muted Gray #5A5A66
                ColorAccent = Color.FromRgb(59, 130, 246);          // Royal Blue 500 #3B82F6
                ColorAccentHover = Color.FromRgb(37, 99, 235);      // Blue 600 #2563EB
                ColorAccentPressed = Color.FromRgb(29, 78, 216);    // Blue 700 #1D4ED8
                ColorAccentMuted = Color.FromArgb(40, 59, 130, 246);
                ColorSuccess = Color.FromRgb(16, 185, 129);         // Emerald #10B981
                ColorError = Color.FromRgb(239, 68, 68);            // Ruby #EF4444
                ColorErrorHover = Color.FromRgb(220, 38, 38);       // Ruby Dark #DC2626
            }
            else
            {
                ColorBackground = Color.FromRgb(248, 250, 252);     // Slate 50
                ColorSurface = Color.FromRgb(255, 255, 255);        // Pure White
                ColorSurfaceHover = Color.FromRgb(241, 245, 249);   // Slate 100
                ColorSurfaceSelected = Color.FromRgb(239, 246, 255); // Blue 50
                ColorBorder = Color.FromRgb(226, 232, 240);         // Slate 200
                ColorBorderHover = Color.FromRgb(203, 213, 225);    // Slate 300
                ColorBorderSelected = Color.FromRgb(37, 99, 235);   // Blue 600
                ColorTextPrimary = Color.FromRgb(15, 23, 42);        // Slate 900
                ColorTextSecondary = Color.FromRgb(100, 116, 139);   // Slate 500
                ColorTextMuted = Color.FromRgb(148, 163, 184);       // Slate 400
                ColorAccent = Color.FromRgb(37, 99, 235);           // Blue 600
                ColorAccentHover = Color.FromRgb(29, 78, 216);       // Blue 700
                ColorAccentPressed = Color.FromRgb(30, 64, 175);     // Blue 800
                ColorAccentMuted = Color.FromArgb(30, 37, 99, 235);
                ColorSuccess = Color.FromRgb(5, 150, 105);          // Emerald 600
                ColorError = Color.FromRgb(220, 38, 38);            // Red 600
                ColorErrorHover = Color.FromRgb(185, 28, 28);
            }

            BrushBackground = new SolidColorBrush(ColorBackground);
            BrushSurface = new SolidColorBrush(ColorSurface);
            BrushSurfaceHover = new SolidColorBrush(ColorSurfaceHover);
            BrushSurfaceSelected = new SolidColorBrush(ColorSurfaceSelected);
            BrushBorder = new SolidColorBrush(ColorBorder);
            BrushBorderHover = new SolidColorBrush(ColorBorderHover);
            BrushBorderSelected = new SolidColorBrush(ColorBorderSelected);
            BrushTextPrimary = new SolidColorBrush(ColorTextPrimary);
            BrushTextSecondary = new SolidColorBrush(ColorTextSecondary);
            BrushTextMuted = new SolidColorBrush(ColorTextMuted);
            BrushAccent = new SolidColorBrush(ColorAccent);
            BrushAccentHover = new SolidColorBrush(ColorAccentHover);
            BrushAccentPressed = new SolidColorBrush(ColorAccentPressed);
            BrushAccentMuted = new SolidColorBrush(ColorAccentMuted);
            BrushSuccess = new SolidColorBrush(ColorSuccess);
            BrushError = new SolidColorBrush(ColorError);
            BrushErrorHover = new SolidColorBrush(ColorErrorHover);

            // Official Windows 11 Fluent font hierarchy (clean fallback to Segoe UI)
            FontDisplay = new FontFamily("Segoe UI Variable Display, Segoe UI");
            FontText = new FontFamily("Segoe UI Variable Text, Segoe UI");
            FontSmall = new FontFamily("Segoe UI Variable Small, Segoe UI");
            FontMono = new FontFamily("Cascadia Code, Consolas");

            // Build component styles
            PrimaryButtonStyle = BuildPrimaryButtonStyle();
            SecondaryButtonStyle = BuildSecondaryButtonStyle();
            GhostButtonStyle = BuildGhostButtonStyle();
            DangerButtonStyle = BuildDangerButtonStyle();
            ModernCheckBoxStyle = BuildModernCheckBoxStyle();
            ModernTextBoxStyle = BuildModernTextBoxStyle();
        }

        private static Style BuildPrimaryButtonStyle()
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.HeightProperty, 34.0));
            style.Setters.Add(new Setter(Button.FontSizeProperty, 12.5));
            style.Setters.Add(new Setter(Button.FontFamilyProperty, FontText));
            style.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.FocusVisualStyleProperty, null));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "btnBorder");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BackgroundProperty, BrushAccent);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.BorderBrushProperty, new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)));
            border.SetValue(Border.PaddingProperty, new Thickness(18, 0, 18, 0));

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextElement.ForegroundProperty, Brushes.White);
            border.AppendChild(content);
            template.VisualTree = border;

            Trigger hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushAccentHover, "btnBorder"));
            template.Triggers.Add(hoverTrigger);

            Trigger pressTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushAccentPressed, "btnBorder"));
            template.Triggers.Add(pressTrigger);

            Trigger disTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disTrigger.Setters.Add(new Setter(Border.OpacityProperty, 0.45, "btnBorder"));
            template.Triggers.Add(disTrigger);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            return style;
        }

        private static Style BuildSecondaryButtonStyle()
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.HeightProperty, 34.0));
            style.Setters.Add(new Setter(Button.FontSizeProperty, 12.0));
            style.Setters.Add(new Setter(Button.FontFamilyProperty, FontText));
            style.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.Medium));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.FocusVisualStyleProperty, null));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "btnBorder");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BackgroundProperty, BrushSurface);
            border.SetValue(Border.BorderBrushProperty, BrushBorder);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.PaddingProperty, new Thickness(14, 0, 14, 0));

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextElement.ForegroundProperty, BrushTextPrimary);
            border.AppendChild(content);
            template.VisualTree = border;

            Trigger hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushSurfaceHover, "btnBorder"));
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, BrushBorderHover, "btnBorder"));
            template.Triggers.Add(hoverTrigger);

            Trigger pressTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushSurfaceSelected, "btnBorder"));
            template.Triggers.Add(pressTrigger);

            Trigger disTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disTrigger.Setters.Add(new Setter(Border.OpacityProperty, 0.45, "btnBorder"));
            template.Triggers.Add(disTrigger);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            return style;
        }

        private static Style BuildGhostButtonStyle()
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.HeightProperty, 30.0));
            style.Setters.Add(new Setter(Button.FontSizeProperty, 11.5));
            style.Setters.Add(new Setter(Button.FontFamilyProperty, FontText));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.FocusVisualStyleProperty, null));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "btnBorder");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            border.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            border.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.PaddingProperty, new Thickness(10, 0, 10, 0));

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextElement.ForegroundProperty, BrushTextSecondary);
            border.AppendChild(content);
            template.VisualTree = border;

            Trigger hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushSurfaceHover, "btnBorder"));
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, BrushBorder, "btnBorder"));
            hoverTrigger.Setters.Add(new Setter(TextElement.ForegroundProperty, BrushTextPrimary, "btnBorder"));
            template.Triggers.Add(hoverTrigger);

            Trigger pressTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushSurfaceSelected, "btnBorder"));
            template.Triggers.Add(pressTrigger);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            return style;
        }

        private static Style BuildDangerButtonStyle()
        {
            Style style = new Style(typeof(Button));
            style.Setters.Add(new Setter(Button.HeightProperty, 34.0));
            style.Setters.Add(new Setter(Button.FontSizeProperty, 12.0));
            style.Setters.Add(new Setter(Button.FontFamilyProperty, FontText));
            style.Setters.Add(new Setter(Button.FontWeightProperty, FontWeights.SemiBold));
            style.Setters.Add(new Setter(Button.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(Button.FocusVisualStyleProperty, null));

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "btnBorder");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BackgroundProperty, BrushError);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            border.SetValue(Border.PaddingProperty, new Thickness(16, 0, 16, 0));

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextElement.ForegroundProperty, Brushes.White);
            border.AppendChild(content);
            template.VisualTree = border;

            Trigger hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushErrorHover, "btnBorder"));
            template.Triggers.Add(hoverTrigger);

            Trigger pressTrigger = new Trigger { Property = Button.IsPressedProperty, Value = true };
            pressTrigger.Setters.Add(new Setter(Border.BackgroundProperty, new SolidColorBrush(Color.FromRgb(185, 28, 28)), "btnBorder"));
            template.Triggers.Add(pressTrigger);

            Trigger disTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            disTrigger.Setters.Add(new Setter(Border.OpacityProperty, 0.45, "btnBorder"));
            template.Triggers.Add(disTrigger);

            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            return style;
        }

        private static Style BuildModernCheckBoxStyle()
        {
            Style style = new Style(typeof(CheckBox));
            style.Setters.Add(new Setter(CheckBox.FontSizeProperty, 11.5));
            style.Setters.Add(new Setter(CheckBox.FontFamilyProperty, FontText));
            style.Setters.Add(new Setter(CheckBox.CursorProperty, Cursors.Hand));
            style.Setters.Add(new Setter(CheckBox.FocusVisualStyleProperty, null));

            ControlTemplate template = new ControlTemplate(typeof(CheckBox));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

            FrameworkElementFactory boxBorder = new FrameworkElementFactory(typeof(Border), "boxBorder");
            boxBorder.SetValue(Border.WidthProperty, 16.0);
            boxBorder.SetValue(Border.HeightProperty, 16.0);
            boxBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
            boxBorder.SetValue(Border.BorderBrushProperty, BrushBorderHover);
            boxBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1.2));
            boxBorder.SetValue(Border.BackgroundProperty, BrushSurface);
            boxBorder.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            boxBorder.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);

            FrameworkElementFactory checkMark = new FrameworkElementFactory(typeof(ShapePath), "checkMark");
            checkMark.SetValue(ShapePath.DataProperty, Geometry.Parse(LucideIcons.Check));
            checkMark.SetValue(ShapePath.StrokeProperty, Brushes.White);
            checkMark.SetValue(ShapePath.StrokeThicknessProperty, 2.2);
            checkMark.SetValue(ShapePath.WidthProperty, 10.0);
            checkMark.SetValue(ShapePath.HeightProperty, 10.0);
            checkMark.SetValue(ShapePath.StretchProperty, Stretch.Uniform);
            checkMark.SetValue(ShapePath.VisibilityProperty, Visibility.Collapsed);
            boxBorder.AppendChild(checkMark);

            grid.AppendChild(boxBorder);

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.MarginProperty, new Thickness(24, 0, 0, 0));
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextElement.ForegroundProperty, BrushTextPrimary);
            grid.AppendChild(content);

            template.VisualTree = grid;

            Trigger chkTrigger = new Trigger { Property = CheckBox.IsCheckedProperty, Value = true };
            chkTrigger.Setters.Add(new Setter(Border.BackgroundProperty, BrushAccent, "boxBorder"));
            chkTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, BrushAccent, "boxBorder"));
            chkTrigger.Setters.Add(new Setter(ShapePath.VisibilityProperty, Visibility.Visible, "checkMark"));
            template.Triggers.Add(chkTrigger);

            Trigger hovTrigger = new Trigger { Property = CheckBox.IsMouseOverProperty, Value = true };
            hovTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, BrushAccent, "boxBorder"));
            template.Triggers.Add(hovTrigger);

            style.Setters.Add(new Setter(CheckBox.TemplateProperty, template));
            return style;
        }

        private static Style BuildModernTextBoxStyle()
        {
            Style style = new Style(typeof(TextBox));
            style.Setters.Add(new Setter(TextBox.FontSizeProperty, 12.0));
            style.Setters.Add(new Setter(TextBox.FontFamilyProperty, FontText));
            style.Setters.Add(new Setter(TextBox.ForegroundProperty, BrushTextPrimary));
            style.Setters.Add(new Setter(TextBox.CaretBrushProperty, BrushAccent));
            style.Setters.Add(new Setter(TextBox.PaddingProperty, new Thickness(8, 5, 8, 5)));

            ControlTemplate template = new ControlTemplate(typeof(TextBox));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border), "txtBorder");
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));
            border.SetValue(Border.BackgroundProperty, BrushSurface);
            border.SetValue(Border.BorderBrushProperty, BrushBorder);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));

            FrameworkElementFactory scroll = new FrameworkElementFactory(typeof(ScrollViewer), "PART_ContentHost");
            scroll.SetValue(ScrollViewer.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(scroll);

            template.VisualTree = border;

            Trigger focusTrigger = new Trigger { Property = TextBox.IsFocusedProperty, Value = true };
            focusTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, BrushAccent, "txtBorder"));
            template.Triggers.Add(focusTrigger);

            Trigger hoverTrigger = new Trigger { Property = TextBox.IsMouseOverProperty, Value = true };
            hoverTrigger.Setters.Add(new Setter(Border.BorderBrushProperty, BrushBorderHover, "txtBorder"));
            template.Triggers.Add(hoverTrigger);

            style.Setters.Add(new Setter(TextBox.TemplateProperty, template));
            return style;
        }

        public static Button CreatePrimaryButton(string text)
        {
            return new Button { Content = text, Style = PrimaryButtonStyle };
        }

        public static Button CreateSecondaryButton(string text)
        {
            return new Button { Content = text, Style = SecondaryButtonStyle };
        }

        public static Button CreateGhostButton(string text)
        {
            return new Button { Content = text, Style = GhostButtonStyle };
        }

        public static Button CreateDangerButton(string text)
        {
            return new Button { Content = text, Style = DangerButtonStyle };
        }

        public static CheckBox CreateCheckBox(string text, bool isChecked = false)
        {
            return new CheckBox { Content = text, IsChecked = isChecked, Style = ModernCheckBoxStyle };
        }

        public static TextBox CreateTextBox(string text = "")
        {
            return new TextBox { Text = text, Style = ModernTextBoxStyle };
        }

        public static void ApplyToWindow(Window win)
        {
            win.Background = BrushBackground;
            win.Foreground = BrushTextPrimary;
            win.FontFamily = FontText;
            win.FontSize = 12.0;
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            win.ResizeMode = ResizeMode.NoResize;
            win.WindowStyle = WindowStyle.SingleBorderWindow;
            win.UseLayoutRounding = true;
            win.SnapsToDevicePixels = true;

            TextOptions.SetTextFormattingMode(win, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(win, TextRenderingMode.ClearType);
            RenderOptions.SetClearTypeHint(win, ClearTypeHint.Enabled);

            win.SourceInitialized += (s, e) =>
            {
                IntPtr hwnd = new WindowInteropHelper(win).Handle;
                int dark = IsDarkMode ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                int round = DWMWCP_ROUND;
                DwmSetWindowAttribute(hwnd, DWMWA_WINDOW_CORNER_PREFERENCE, ref round, sizeof(int));
            };

            win.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) win.Close();
            };
        }
    }

    public class ModernSliderControl : UserControl
    {
        public event EventHandler<double> ValueChanged;

        private double _minimum = 0;
        private double _maximum = 100;
        private double _value = 50;
        private bool _isDragging = false;

        private Border _trackBg;
        private Border _trackFill;
        private Border _thumb;
        private Canvas _canvas;

        public double Minimum
        {
            get { return _minimum; }
            set { _minimum = value; UpdateLayoutPositions(); }
        }

        public double Maximum
        {
            get { return _maximum; }
            set { _maximum = value; UpdateLayoutPositions(); }
        }

        public double Value
        {
            get { return _value; }
            set
            {
                double clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (Math.Abs(_value - clamped) > 0.001)
                {
                    _value = clamped;
                    UpdateLayoutPositions();
                    if (ValueChanged != null) ValueChanged(this, _value);
                }
            }
        }

        public ModernSliderControl()
        {
            this.Height = 22;
            this.Cursor = Cursors.Hand;
            this.Focusable = true;

            Grid root = new Grid { VerticalAlignment = VerticalAlignment.Center, Height = 20 };

            _canvas = new Canvas { Height = 20 };

            _trackBg = new Border
            {
                Height = 4,
                CornerRadius = new CornerRadius(2),
                Background = WpfTheme.BrushBorder,
                VerticalAlignment = VerticalAlignment.Center
            };
            root.Children.Add(_trackBg);

            _trackFill = new Border
            {
                Height = 4,
                CornerRadius = new CornerRadius(2),
                Background = WpfTheme.BrushAccent,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            root.Children.Add(_trackFill);

            _thumb = new Border
            {
                Width = 14,
                Height = 14,
                CornerRadius = new CornerRadius(7),
                Background = Brushes.White,
                BorderBrush = WpfTheme.BrushAccent,
                BorderThickness = new Thickness(2),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 3,
                    Opacity = 0.35,
                    ShadowDepth = 1,
                    Direction = 270
                }
            };
            _canvas.Children.Add(_thumb);
            root.Children.Add(_canvas);

            this.Content = root;

            this.SizeChanged += (s, e) => UpdateLayoutPositions();

            this.MouseDown += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _isDragging = true;
                    this.CaptureMouse();
                    UpdateFromMouse(e.GetPosition(this).X);
                }
            };

            this.MouseMove += (s, e) =>
            {
                if (_isDragging)
                {
                    UpdateFromMouse(e.GetPosition(this).X);
                }
            };

            this.MouseUp += (s, e) =>
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    this.ReleaseMouseCapture();
                }
            };
        }

        private void UpdateFromMouse(double mouseX)
        {
            double width = this.ActualWidth;
            if (width <= 14) return;
            double usable = width - 14;
            double rel = Math.Max(0, Math.Min(usable, mouseX - 7));
            double frac = rel / usable;
            Value = _minimum + frac * (_maximum - _minimum);
        }

        private void UpdateLayoutPositions()
        {
            double width = this.ActualWidth;
            if (width <= 14) return;
            double usable = width - 14;
            double frac = (_maximum > _minimum) ? (_value - _minimum) / (_maximum - _minimum) : 0;
            frac = Math.Max(0, Math.Min(1, frac));

            double thumbX = frac * usable;
            Canvas.SetLeft(_thumb, thumbX);
            Canvas.SetTop(_thumb, 3);

            _trackFill.Width = thumbX + 7;
        }
    }

    public class ModernProgressBarControl : UserControl
    {
        private double _minimum = 0;
        private double _maximum = 100;
        private double _value = 0;
        private Border _track;
        private Border _indicator;

        public double Minimum
        {
            get { return _minimum; }
            set { _minimum = value; UpdateIndicator(); }
        }

        public double Maximum
        {
            get { return _maximum; }
            set { _maximum = value; UpdateIndicator(); }
        }

        public double Value
        {
            get { return _value; }
            set { _value = Math.Max(_minimum, Math.Min(_maximum, value)); UpdateIndicator(); }
        }

        public ModernProgressBarControl()
        {
            this.Height = 6;
            Grid grid = new Grid();

            _track = new Border
            {
                Height = 6,
                CornerRadius = new CornerRadius(3),
                Background = WpfTheme.BrushBorder,
                ClipToBounds = true
            };

            _indicator = new Border
            {
                Height = 6,
                CornerRadius = new CornerRadius(3),
                Background = WpfTheme.BrushAccent,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 0
            };

            _track.Child = _indicator;
            grid.Children.Add(_track);
            this.Content = grid;
            this.SizeChanged += (s, e) => UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            double width = this.ActualWidth;
            if (width <= 0) return;
            double frac = (_maximum > _minimum) ? (_value - _minimum) / (_maximum - _minimum) : 0;
            frac = Math.Max(0, Math.Min(1, frac));
            _indicator.Width = frac * width;
        }
    }

    public static class LucideIcons
    {
        public const string Image = "M3 3h18a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2zm6 8a2 2 0 1 0 0-4 2 2 0 0 0 0 4zm12 4-3.086-3.086a2 2 0 0 0-2.828 0L6 21";
        public const string Check = "M20 6 9 17l-5-5";
        public const string X = "M18 6 6 18M6 6l12 12";
        public const string ArrowRight = "M5 12h14M12 5l7 7-7 7";
        public const string Folder = "M4 20h16a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.93a2 2 0 0 1-1.66-.9l-.82-1.2A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13c0 1.1.9 2 2 2Z";
        public const string Eye = "M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6Z";
        public const string Lock = "M19 11H5a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7a2 2 0 0 0-2-2ZM7 11V7a5 5 0 0 1 10 0v4";
        public const string Unlock = "M19 11H5a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7a2 2 0 0 0-2-2ZM7 11V7a5 5 0 0 1 9.9-1";
        public const string Compress = "M4 14h6v6M20 10h-6V4M14 10l7-7M3 21l7-7";
        public const string Resize = "M15 3h6v6M9 21H3v-6M21 3l-7 7M3 21l7-7";
        public const string Sparkles = "m12 3-1.912 5.813a2 2 0 0 1-1.275 1.275L3 12l5.813 1.912a2 2 0 0 1 1.275 1.275L12 21l1.912-5.813a2 2 0 0 1 1.275-1.275L21 12l-5.813-1.912a2 2 0 0 1-1.275-1.275L12 3Z";
        public const string Sliders = "M4 21v-7M4 10V3M12 21v-9M12 8V3M20 21v-5M20 12V3M1 14h6M9 8h6M17 16h6";
        public const string Sun = "M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M6.34 17.66l-1.41 1.41M19.07 4.93l-1.41 1.41M12 17a5 5 0 1 0 0-10 5 5 0 0 0 0 10z";
        public const string Layers = "m12 2 10 5-10 5L2 7l10-5zm0 9 10 5-10 5-10-5 10-5zm0 9 10 5-10 5-10-5 10-5z";
        public const string AppWindow = "M20 4H4a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2zM10 4v4H2M2 8h20";

        public static ShapePath CreatePath(string geometryData, Brush stroke, double size = 18, double strokeThickness = 1.6)
        {
            return new ShapePath
            {
                Data = Geometry.Parse(geometryData),
                Stroke = stroke,
                StrokeThickness = strokeThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform
            };
        }
    }
}