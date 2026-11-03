using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using ShapePath = System.Windows.Shapes.Path;

namespace VeqloQuickSetup
{
    public static class SetupTheme
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWCP_ROUND = 2;

        public static bool IsDarkMode = true;

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

        public static FontFamily FontDisplay;
        public static FontFamily FontText;

        public static Style PrimaryButtonStyle;
        public static Style SecondaryButtonStyle;
        public static Style DangerButtonStyle;
        public static Style ModernCheckBoxStyle;
        public static Style ModernTextBoxStyle;

        static SetupTheme()
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
                ColorBackground = Color.FromRgb(11, 11, 14);
                ColorSurface = Color.FromRgb(21, 21, 26);
                ColorSurfaceHover = Color.FromRgb(28, 28, 36);
                ColorSurfaceSelected = Color.FromRgb(20, 28, 46);
                ColorBorder = Color.FromRgb(37, 37, 46);
                ColorBorderHover = Color.FromRgb(56, 56, 70);
                ColorBorderSelected = Color.FromRgb(59, 130, 246);
                ColorTextPrimary = Color.FromRgb(244, 244, 246);
                ColorTextSecondary = Color.FromRgb(142, 142, 155);
                ColorTextMuted = Color.FromRgb(90, 90, 102);
                ColorAccent = Color.FromRgb(59, 130, 246);
                ColorAccentHover = Color.FromRgb(37, 99, 235);
                ColorAccentPressed = Color.FromRgb(29, 78, 216);
                ColorAccentMuted = Color.FromArgb(40, 59, 130, 246);
                ColorSuccess = Color.FromRgb(16, 185, 129);
                ColorError = Color.FromRgb(239, 68, 68);
                ColorErrorHover = Color.FromRgb(220, 38, 38);
            }
            else
            {
                ColorBackground = Color.FromRgb(248, 250, 252);
                ColorSurface = Color.FromRgb(255, 255, 255);
                ColorSurfaceHover = Color.FromRgb(241, 245, 249);
                ColorSurfaceSelected = Color.FromRgb(239, 246, 255);
                ColorBorder = Color.FromRgb(226, 232, 240);
                ColorBorderHover = Color.FromRgb(203, 213, 225);
                ColorBorderSelected = Color.FromRgb(37, 99, 235);
                ColorTextPrimary = Color.FromRgb(15, 23, 42);
                ColorTextSecondary = Color.FromRgb(100, 116, 139);
                ColorTextMuted = Color.FromRgb(148, 163, 184);
                ColorAccent = Color.FromRgb(37, 99, 235);
                ColorAccentHover = Color.FromRgb(29, 78, 216);
                ColorAccentPressed = Color.FromRgb(30, 64, 175);
                ColorAccentMuted = Color.FromArgb(30, 37, 99, 235);
                ColorSuccess = Color.FromRgb(5, 150, 105);
                ColorError = Color.FromRgb(220, 38, 38);
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

            FontDisplay = new FontFamily("Segoe UI Variable Display, Segoe UI");
            FontText = new FontFamily("Segoe UI Variable Text, Segoe UI");

            PrimaryButtonStyle = BuildPrimaryButtonStyle();
            SecondaryButtonStyle = BuildSecondaryButtonStyle();
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
            checkMark.SetValue(ShapePath.DataProperty, Geometry.Parse("M20 6 9 17l-5-5"));
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
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            win.ResizeMode = ResizeMode.NoResize;
            win.WindowStyle = WindowStyle.SingleBorderWindow;

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

    public class SetupWindow : Window
    {
        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(int wEventId, int uFlags, IntPtr dwItem1, IntPtr dwItem2);
        private const int SHCNE_ASSOCCHANGED = 0x08000000;
        private const int SHCNF_IDLIST = 0;

        private const string AppName = "Veqlo Quick";
        private const string PackageName = "Veqlo.Quick";
        private const string Version = "1.1.0";
        private const string Publisher = "Veqlo";

        private bool isUninstall = false;
        private string targetInstallPath;

        private Grid rootGrid;
        private StackPanel contentStack;
        private Button btnPrimary;
        private Button btnSecondary;

        private TextBox txtInstallDir;
        private CheckBox chkWin11;
        private CheckBox chkClassic;

        private TextBlock statusBlock;
        private ProgressBar progressBar;

        public SetupWindow(bool uninstall)
        {
            this.isUninstall = uninstall;
            this.Title = (isUninstall ? "Uninstall " : "Install ") + AppName;
            this.Width = 560;
            this.Height = 470;
            SetupTheme.ApplyToWindow(this);

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            targetInstallPath = System.IO.Path.Combine(localAppData, @"Programs\Veqlo Quick");

            BuildLayout();
            if (isUninstall) ShowUninstallWelcome();
            else ShowInstallWelcome();
        }

        private void BuildLayout()
        {
            rootGrid = new Grid { Margin = new Thickness(24) };
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Footer buttons

            // --- HEADER ---
            Grid headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 16) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Monogram Logo
            Border logoBorder = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(10),
                Background = SetupTheme.BrushAccent,
                Margin = new Thickness(0, 0, 14, 0)
            };
            TextBlock logoText = new TextBlock
            {
                Text = "VQ",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            logoBorder.Child = logoText;
            Grid.SetColumn(logoBorder, 0);
            headerGrid.Children.Add(logoBorder);

            StackPanel titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            StackPanel titleRow = new StackPanel { Orientation = Orientation.Horizontal };

            TextBlock title = new TextBlock
            {
                Text = isUninstall ? "Uninstall Veqlo Quick" : "Veqlo Quick Setup",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                FontFamily = SetupTheme.FontDisplay,
                Foreground = SetupTheme.BrushTextPrimary,
                Margin = new Thickness(0, 0, 8, 0)
            };
            titleRow.Children.Add(title);

            Border verPill = new Border
            {
                Background = SetupTheme.BrushAccentMuted,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            verPill.Child = new TextBlock
            {
                Text = "v" + Version,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = SetupTheme.BrushAccent
            };
            titleRow.Children.Add(verPill);
            titleStack.Children.Add(titleRow);

            TextBlock sub = new TextBlock
            {
                Text = isUninstall ? "Remove shell extension and registry integration" : "Native Windows File Explorer Shell Integration",
                FontSize = 12,
                Foreground = SetupTheme.BrushTextSecondary
            };
            titleStack.Children.Add(sub);

            Grid.SetColumn(titleStack, 1);
            headerGrid.Children.Add(titleStack);

            Grid.SetRow(headerGrid, 0);
            rootGrid.Children.Add(headerGrid);

            // --- CONTENT AREA ---
            contentStack = new StackPanel();
            Grid.SetRow(contentStack, 1);
            rootGrid.Children.Add(contentStack);

            // --- FOOTER BUTTONS ---
            StackPanel footerRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };

            btnSecondary = SetupTheme.CreateSecondaryButton("Cancel");
            btnSecondary.Margin = new Thickness(0, 0, 10, 0);
            btnSecondary.Click += (s, e) => this.Close();
            footerRow.Children.Add(btnSecondary);

            btnPrimary = isUninstall ? SetupTheme.CreateDangerButton("Uninstall") : SetupTheme.CreatePrimaryButton("Install Now");
            btnPrimary.Click += (s, e) => OnPrimaryClick();
            footerRow.Children.Add(btnPrimary);

            Grid.SetRow(footerRow, 2);
            rootGrid.Children.Add(footerRow);

            this.Content = rootGrid;
        }

        private void ShowInstallWelcome()
        {
            contentStack.Children.Clear();

            // 1. Architecture Info Card
            Border infoCard = new Border
            {
                Background = SetupTheme.BrushSurface,
                BorderBrush = SetupTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 14)
            };

            StackPanel infoStack = new StackPanel();
            TextBlock infoTitle = new TextBlock
            {
                Text = "Windows Explorer Shell Integration",
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = SetupTheme.BrushTextPrimary,
                Margin = new Thickness(0, 0, 0, 4)
            };
            infoStack.Children.Add(infoTitle);

            TextBlock infoDesc = new TextBlock
            {
                Text = "Veqlo Quick is a pure shell utility. Once installed, right-click any image (JPG, PNG, WebP, BMP, TIFF, ICO) to Convert, Compress, or Resize directly from File Explorer. Zero background services or system tray overhead.",
                FontSize = 11.5,
                Foreground = SetupTheme.BrushTextSecondary,
                TextWrapping = TextWrapping.Wrap
            };
            infoStack.Children.Add(infoDesc);
            infoCard.Child = infoStack;
            contentStack.Children.Add(infoCard);

            // 2. Install Path
            TextBlock locLabel = new TextBlock
            {
                Text = "Installation Location:",
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = SetupTheme.BrushTextPrimary,
                Margin = new Thickness(0, 0, 0, 6)
            };
            contentStack.Children.Add(locLabel);

            Grid pathGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pathGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border txtBorder = new Border
            {
                Background = SetupTheme.BrushSurface,
                BorderBrush = SetupTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6)
            };
            txtInstallDir = new TextBox
            {
                Text = targetInstallPath,
                FontSize = 12,
                Background = Brushes.Transparent,
                Foreground = SetupTheme.BrushTextPrimary,
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center
            };
            txtBorder.Child = txtInstallDir;
            Grid.SetColumn(txtBorder, 0);
            pathGrid.Children.Add(txtBorder);

            Button btnBrowse = CreateStyledButton("Browse...", false);
            btnBrowse.Margin = new Thickness(8, 0, 0, 0);
            btnBrowse.Click += (s, e) =>
            {
                using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
                {
                    fbd.SelectedPath = txtInstallDir.Text;
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        txtInstallDir.Text = System.IO.Path.Combine(fbd.SelectedPath, "Veqlo Quick");
                    }
                }
            };
            Grid.SetColumn(btnBrowse, 1);
            pathGrid.Children.Add(btnBrowse);

            contentStack.Children.Add(pathGrid);

            // 3. Integration Options
            TextBlock optLabel = new TextBlock
            {
                Text = "Explorer Integration Features:",
                FontSize = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = SetupTheme.BrushTextPrimary,
                Margin = new Thickness(0, 0, 0, 6)
            };
            contentStack.Children.Add(optLabel);

            chkWin11 = SetupTheme.CreateCheckBox("Register modern Windows 11 context menu (Sparse Package)", true);
            chkWin11.Margin = new Thickness(0, 0, 0, 6);
            contentStack.Children.Add(chkWin11);

            chkClassic = SetupTheme.CreateCheckBox("Register classic File Explorer menu (Windows 10 / Legacy fallback)", true);
            chkClassic.Margin = new Thickness(0, 0, 0, 6);
            contentStack.Children.Add(chkClassic);

            btnPrimary.Content = "Install Now";
            btnPrimary.IsEnabled = true;
            btnSecondary.Content = "Cancel";
            btnSecondary.IsEnabled = true;
        }

        private void ShowUninstallWelcome()
        {
            contentStack.Children.Clear();

            Border warnCard = new Border
            {
                Background = SetupTheme.BrushSurface,
                BorderBrush = SetupTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 20, 0, 0)
            };

            StackPanel warnStack = new StackPanel();
            TextBlock warnTitle = new TextBlock
            {
                Text = "Confirm Uninstallation",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = SetupTheme.BrushTextPrimary,
                Margin = new Thickness(0, 0, 0, 6)
            };
            warnStack.Children.Add(warnTitle);

            TextBlock warnDesc = new TextBlock
            {
                Text = "This will remove all Veqlo Quick Explorer context-menu entries, COM shell extensions, and application binaries from your system.\n\nYour personal image files will NOT be modified or deleted.",
                FontSize = 12,
                Foreground = SetupTheme.BrushTextSecondary,
                TextWrapping = TextWrapping.Wrap
            };
            warnStack.Children.Add(warnDesc);
            warnCard.Child = warnStack;
            contentStack.Children.Add(warnCard);

            btnPrimary.Content = "Uninstall";
            btnPrimary.IsEnabled = true;
            btnSecondary.Content = "Cancel";
            btnSecondary.IsEnabled = true;
        }

        private void ShowProgressState(string initialMsg)
        {
            contentStack.Children.Clear();

            StackPanel procStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            };

            statusBlock = new TextBlock
            {
                Text = initialMsg,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = SetupTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };
            procStack.Children.Add(statusBlock);

            progressBar = new ProgressBar
            {
                IsIndeterminate = true,
                Width = 400,
                Height = 8,
                BorderThickness = new Thickness(0),
                Background = SetupTheme.BrushBorder,
                Foreground = SetupTheme.BrushAccent,
                Margin = new Thickness(0, 0, 0, 12)
            };
            procStack.Children.Add(progressBar);

            TextBlock sub = new TextBlock
            {
                Text = "Please wait while Windows updates shell associations...",
                FontSize = 11,
                Foreground = SetupTheme.BrushTextSecondary,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            procStack.Children.Add(sub);

            contentStack.Children.Add(procStack);

            btnPrimary.IsEnabled = false;
            btnSecondary.IsEnabled = false;
        }

        private void ShowCompletedState()
        {
            contentStack.Children.Clear();

            StackPanel compStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            Border iconCircle = new Border
            {
                Width = 56,
                Height = 56,
                CornerRadius = new CornerRadius(28),
                Background = new SolidColorBrush(Color.FromArgb(35, 16, 185, 129)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 14)
            };
            ShapePath checkPath = new ShapePath
            {
                Data = Geometry.Parse("M20 6 9 17l-5-5"),
                Stroke = SetupTheme.BrushSuccess,
                StrokeThickness = 2.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Width = 26,
                Height = 26,
                Stretch = Stretch.Uniform
            };
            iconCircle.Child = checkPath;
            compStack.Children.Add(iconCircle);

            TextBlock title = new TextBlock
            {
                Text = isUninstall ? "Veqlo Quick Uninstalled" : "Installation Complete!",
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                FontFamily = SetupTheme.FontDisplay,
                Foreground = SetupTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            compStack.Children.Add(title);

            TextBlock desc = new TextBlock
            {
                Text = isUninstall ?
                    "All context-menu associations and application files have been safely removed." :
                    "Right-click any image file in File Explorer and choose Veqlo Quick to Convert, Compress, or Resize.",
                FontSize = 12,
                Foreground = SetupTheme.BrushTextSecondary,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                MaxWidth = 420
            };
            compStack.Children.Add(desc);

            contentStack.Children.Add(compStack);

            btnPrimary.Content = "Finish";
            btnPrimary.IsEnabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Visibility = Visibility.Collapsed;
        }

        private void ShowErrorState(string msg)
        {
            contentStack.Children.Clear();

            StackPanel errStack = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            Border iconCircle = new Border
            {
                Width = 56,
                Height = 56,
                CornerRadius = new CornerRadius(28),
                Background = new SolidColorBrush(Color.FromArgb(35, 239, 68, 68)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 14)
            };
            ShapePath xPath = new ShapePath
            {
                Data = Geometry.Parse("M18 6 6 18M6 6l12 12"),
                Stroke = SetupTheme.BrushError,
                StrokeThickness = 2.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Width = 24,
                Height = 24,
                Stretch = Stretch.Uniform
            };
            iconCircle.Child = xPath;
            errStack.Children.Add(iconCircle);

            TextBlock title = new TextBlock
            {
                Text = isUninstall ? "Uninstallation Failed" : "Installation Failed",
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Foreground = SetupTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            errStack.Children.Add(title);

            TextBlock desc = new TextBlock
            {
                Text = msg,
                FontSize = 12,
                Foreground = SetupTheme.BrushTextSecondary,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                MaxWidth = 440
            };
            errStack.Children.Add(desc);

            contentStack.Children.Add(errStack);

            btnPrimary.Content = "Close";
            btnPrimary.IsEnabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Visibility = Visibility.Collapsed;
        }

        private void OnPrimaryClick()
        {
            if (txtInstallDir != null)
            {
                targetInstallPath = txtInstallDir.Text.Trim();
            }

            ShowProgressState(isUninstall ? "Removing Veqlo Quick..." : "Installing Veqlo Quick...");

            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    if (isUninstall)
                    {
                        ExecuteUninstall();
                    }
                    else
                    {
                        ExecuteInstall();
                    }

                    this.Dispatcher.Invoke((Action)delegate
                    {
                        ShowCompletedState();
                    });
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        ShowErrorState(ex.Message);
                    });
                }
            });
        }

        public void ExecuteSilentUninstall()
        {
            ExecuteUninstall();
        }

        public void ExecuteSilentInstall()
        {
            ExecuteInstall();
        }

        private void UpdateStatus(string msg)
        {
            try
            {
                if (this.Dispatcher != null && !this.Dispatcher.HasShutdownStarted)
                {
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        if (statusBlock != null) statusBlock.Text = msg;
                    });
                }
            }
            catch { }
        }

        private void ExecuteInstall()
        {
            UpdateStatus("Stopping running instances...");
            foreach (Process p in Process.GetProcessesByName("VeqloQuick"))
            {
                try { p.Kill(); p.WaitForExit(1000); } catch { }
            }

            UpdateStatus("Creating installation directory...");
            Directory.CreateDirectory(targetInstallPath);
            string assetsDir = System.IO.Path.Combine(targetInstallPath, "Assets");
            Directory.CreateDirectory(assetsDir);

            UpdateStatus("Extracting binaries and tools...");
            DeployPayload(targetInstallPath);

            // Copy self as uninstaller
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;
            string uninstaller = System.IO.Path.Combine(targetInstallPath, "VeqloQuickSetup.exe");
            if (!currentExe.Equals(uninstaller, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(currentExe, uninstaller, true);
            }

            UpdateStatus("Registering Windows 11 Shell extension...");
            string manifestPath = System.IO.Path.Combine(targetInstallPath, "AppxManifest.xml");
            if (File.Exists(manifestPath))
            {
                try
                {
                    string script = string.Format(
                        "$ErrorActionPreference = 'SilentlyContinue'; " +
                        "$pkg = Get-AppxPackage -Name '{0}' -ErrorAction SilentlyContinue; " +
                        "if ($pkg) {{ Remove-AppxPackage -Package $pkg.PackageFullName }}; " +
                        "Add-AppxPackage -Register '{1}' -ExternalLocation '{2}'",
                        PackageName, manifestPath, targetInstallPath);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = string.Format("-NoProfile -ExecutionPolicy Bypass -Command \"{0}\"", script),
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (Process proc = Process.Start(psi))
                    {
                        proc.WaitForExit();
                    }
                }
                catch { }
            }

            UpdateStatus("Registering classic Explorer context menu...");
            string appExe = System.IO.Path.Combine(targetInstallPath, "VeqloQuick.exe");
            RegisterClassicMenu(targetInstallPath, appExe);

            UpdateStatus("Registering Windows Uninstaller...");
            RegisterUninstallKey(targetInstallPath, uninstaller);

            UpdateStatus("Refreshing Windows Shell...");
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            Thread.Sleep(400);
        }

        private void ExecuteUninstall()
        {
            UpdateStatus("Stopping running instances...");
            foreach (Process p in Process.GetProcessesByName("VeqloQuick"))
            {
                try { p.Kill(); p.WaitForExit(1000); } catch { }
            }

            UpdateStatus("Unregistering modern package...");
            try
            {
                string script = string.Format(
                    "$pkg = Get-AppxPackage -Name '{0}' -ErrorAction SilentlyContinue; " +
                    "if ($pkg) {{ Remove-AppxPackage -Package $pkg.PackageFullName }}",
                    PackageName);

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = string.Format("-NoProfile -ExecutionPolicy Bypass -Command \"{0}\"", script),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                }
            }
            catch { }

            UpdateStatus("Removing registry keys...");
            UnregisterClassicMenu();

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (key != null) key.DeleteSubKeyTree("VeqloQuick", false);
            }

            UpdateStatus("Cleaning up files...");
            if (Directory.Exists(targetInstallPath))
            {
                try
                {
                    Directory.Delete(targetInstallPath, true);
                }
                catch
                {
                    string cleanupBat = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "veqlo_cleanup.bat");
                    string batContent = string.Format(
                        "@echo off\r\ntimeout /t 2 /nobreak > nul\r\n:retry\r\nrd /s /q \"{0}\" 2>nul\r\nif exist \"{0}\" (\r\n  timeout /t 1 /nobreak > nul\r\n  goto retry\r\n)\r\ndel \"%~f0\"\r\n", targetInstallPath);
                    File.WriteAllText(cleanupBat, batContent);
                    Process.Start(new ProcessStartInfo("cmd.exe", string.Format("/c \"{0}\"", cleanupBat))
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }

            UpdateStatus("Refreshing Windows Shell...");
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
            Thread.Sleep(300);
        }

        private void DeployPayload(string destDir)
        {
            // 1. Check embedded resource
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream resStream = asm.GetManifestResourceStream("VeqloQuickPayload"))
            {
                if (resStream != null)
                {
                    using (ZipArchive zip = new ZipArchive(resStream))
                    {
                        zip.ExtractToDirectory(destDir);
                    }
                    return;
                }
            }

            // 2. Fallback: adjacent package folder
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string pkgDir = System.IO.Path.Combine(baseDir, "package");
            if (!Directory.Exists(pkgDir))
            {
                pkgDir = System.IO.Path.Combine(Directory.GetParent(baseDir).FullName, "package");
            }

            string[] files = new string[] { "VeqloQuick.exe", "VeqloQuickShell.dll", "AppxManifest.xml", "cwebp.exe", "dwebp.exe" };
            foreach (string f in files)
            {
                string s = System.IO.Path.Combine(pkgDir, f);
                if (File.Exists(s)) File.Copy(s, System.IO.Path.Combine(destDir, f), true);
            }

            string srcAssets = System.IO.Path.Combine(pkgDir, "Assets");
            string dstAssets = System.IO.Path.Combine(destDir, "Assets");
            if (Directory.Exists(srcAssets))
            {
                Directory.CreateDirectory(dstAssets);
                foreach (string f in Directory.GetFiles(srcAssets))
                {
                    File.Copy(f, System.IO.Path.Combine(dstAssets, System.IO.Path.GetFileName(f)), true);
                }
            }
        }

        private void RegisterClassicMenu(string targetDir, string appExe)
        {
            try
            {
                string[] assocs = new string[]
                {
                    @"Software\Classes\SystemFileAssociations\image\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.jpg\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.jpeg\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.png\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.webp\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.bmp\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.tiff\shell\VeqloQuick",
                    @"Software\Classes\SystemFileAssociations\.ico\shell\VeqloQuick"
                };

                foreach (string assoc in assocs)
                {
                    using (RegistryKey root = Registry.CurrentUser.CreateSubKey(assoc))
                    {
                        if (root == null) continue;
                        root.SetValue("MUIVerb", "Veqlo Quick");
                        root.SetValue("SubCommands", "");
                        root.SetValue("Icon", string.Format("\"{0}\",0", appExe));

                        using (RegistryKey shell = root.CreateSubKey("Shell"))
                        {
                            using (RegistryKey k1 = shell.CreateSubKey("1_Convert"))
                            {
                                k1.SetValue("MUIVerb", "Convert");
                                using (RegistryKey c = k1.CreateSubKey("command"))
                                {
                                    c.SetValue("", string.Format("\"{0}\" convert --input \"%1\"", appExe));
                                }
                            }
                            using (RegistryKey k2 = shell.CreateSubKey("2_Compress"))
                            {
                                k2.SetValue("MUIVerb", "Compress");
                                using (RegistryKey c = k2.CreateSubKey("command"))
                                {
                                    c.SetValue("", string.Format("\"{0}\" compress --input \"%1\"", appExe));
                                }
                            }
                            using (RegistryKey k3 = shell.CreateSubKey("3_Resize"))
                            {
                                k3.SetValue("MUIVerb", "Resize");
                                using (RegistryKey c = k3.CreateSubKey("command"))
                                {
                                    c.SetValue("", string.Format("\"{0}\" resize --input \"%1\"", appExe));
                                }
                            }
                            using (RegistryKey k4 = shell.CreateSubKey("4_Inspect"))
                            {
                                k4.SetValue("MUIVerb", "Inspect details");
                                using (RegistryKey c = k4.CreateSubKey("command"))
                                {
                                    c.SetValue("", string.Format("\"{0}\" inspect --input \"%1\"", appExe));
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void UnregisterClassicMenu()
        {
            try
            {
                string[] assocs = new string[]
                {
                    @"Software\Classes\SystemFileAssociations\image\shell",
                    @"Software\Classes\SystemFileAssociations\.jpg\shell",
                    @"Software\Classes\SystemFileAssociations\.jpeg\shell",
                    @"Software\Classes\SystemFileAssociations\.png\shell",
                    @"Software\Classes\SystemFileAssociations\.webp\shell",
                    @"Software\Classes\SystemFileAssociations\.bmp\shell",
                    @"Software\Classes\SystemFileAssociations\.tiff\shell",
                    @"Software\Classes\SystemFileAssociations\.ico\shell"
                };

                foreach (string assoc in assocs)
                {
                    using (RegistryKey shell = Registry.CurrentUser.OpenSubKey(assoc, true))
                    {
                        if (shell != null) shell.DeleteSubKeyTree("VeqloQuick", false);
                    }
                }
            }
            catch { }
        }

        private void RegisterUninstallKey(string installDir, string uninstallerPath)
        {
            using (RegistryKey uninstallRoot = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\VeqloQuick"))
            {
                if (uninstallRoot != null)
                {
                    uninstallRoot.SetValue("DisplayName", AppName);
                    uninstallRoot.SetValue("DisplayVersion", Version);
                    uninstallRoot.SetValue("Publisher", Publisher);
                    uninstallRoot.SetValue("InstallLocation", installDir);
                    uninstallRoot.SetValue("UninstallString", string.Format("\"{0}\" --uninstall", uninstallerPath));
                    uninstallRoot.SetValue("DisplayIcon", string.Format("\"{0}\",0", System.IO.Path.Combine(installDir, "VeqloQuick.exe")));
                    uninstallRoot.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    uninstallRoot.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    uninstallRoot.SetValue("EstimatedSize", 2500, RegistryValueKind.DWord);
                }
            }
        }

        private Button CreateStyledButton(string text, bool isPrimary)
        {
            return isPrimary ? SetupTheme.CreatePrimaryButton(text) : SetupTheme.CreateSecondaryButton(text);
        }
    }

    public class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            App app = new App();

            bool isUninstall = false;
            bool isSilent = false;

            foreach (string arg in args)
            {
                if (arg.Equals("--uninstall", StringComparison.OrdinalIgnoreCase) || arg.Equals("-u", StringComparison.OrdinalIgnoreCase))
                {
                    isUninstall = true;
                }
                else if (arg.Equals("--silent", StringComparison.OrdinalIgnoreCase) || arg.Equals("-s", StringComparison.OrdinalIgnoreCase))
                {
                    isSilent = true;
                }
            }

            SetupWindow win = new SetupWindow(isUninstall);
            if (isSilent)
            {
                if (isUninstall)
                {
                    win.ExecuteSilentUninstall();
                }
                else
                {
                    win.ExecuteSilentInstall();
                }
                return;
            }

            app.Run(win);
        }
    }
}
