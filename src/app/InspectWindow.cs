using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ShapePath = System.Windows.Shapes.Path;

namespace VeqloQuick
{
    public class SpecRow
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public SpecRow(string label, string val) { Label = label; Value = val; }
    }

    public class InspectWindow : Window
    {
        private List<string> inputFiles;
        private int currentIndex = 0;
        private ImageMetadata currentMeta;

        private Grid rootGrid;
        private StackPanel contentStack;
        private Button btnClose;
        private Button btnCopySpecs;
        private Button btnCleanMetadata;

        private TextBlock txtFileHeader;
        private StackPanel specsPanel;
        private TextBlock txtCleanStatus;
        private TextBlock txtPager;
        private Button btnPrev;
        private Button btnNext;

        public InspectWindow(List<string> files)
        {
            this.inputFiles = (files != null && files.Count > 0) ? files : new List<string>();
            this.Title = "Veqlo Quick — Image Details";
            this.Width = 560;
            this.Height = 480;
            WpfTheme.ApplyToWindow(this);

            BuildLayout();
            LoadCurrentFile();
        }

        private void BuildLayout()
        {
            rootGrid = new Grid { Margin = new Thickness(24) };
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: Header
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 1: Content
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2: Footer

            // --- 0: HEADER ---
            StackPanel headerStack = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };

            StackPanel titleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            ShapePath infoIcon = LucideIcons.CreatePath(LucideIcons.Image, WpfTheme.BrushAccent, 18);
            infoIcon.Margin = new Thickness(0, 2, 8, 0);
            titleRow.Children.Add(infoIcon);

            TextBlock title = new TextBlock
            {
                Text = "Image details & specifications",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                FontFamily = WpfTheme.FontDisplay,
                Foreground = WpfTheme.BrushTextPrimary
            };
            titleRow.Children.Add(title);
            headerStack.Children.Add(titleRow);

            TextBlock subtitle = new TextBlock
            {
                Text = "Inspect dimensions, color space, camera settings, and strip sensitive location data.",
                FontSize = 12,
                Foreground = WpfTheme.BrushTextSecondary
            };
            headerStack.Children.Add(subtitle);

            Grid.SetRow(headerStack, 0);
            rootGrid.Children.Add(headerStack);

            // --- 1: CONTENT STACK ---
            contentStack = new StackPanel();
            Grid.SetRow(contentStack, 1);
            rootGrid.Children.Add(contentStack);

            // File summary bar
            Border fileBar = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 14)
            };

            Grid fileBarGrid = new Grid();
            fileBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fileBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            txtFileHeader = new TextBlock
            {
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = WpfTheme.BrushTextPrimary,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtFileHeader, 0);
            fileBarGrid.Children.Add(txtFileHeader);

            // Pager controls if multiple files
            StackPanel pagerStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            btnPrev = WpfTheme.CreateGhostButton("◀");
            btnPrev.Width = 28;
            btnPrev.Height = 24;
            btnPrev.Click += (s, e) => NavigateFile(-1);
            pagerStack.Children.Add(btnPrev);

            txtPager = new TextBlock
            {
                Text = "1 of 1",
                FontSize = 11,
                Foreground = WpfTheme.BrushTextMuted,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 4, 0)
            };
            pagerStack.Children.Add(txtPager);

            btnNext = WpfTheme.CreateGhostButton("▶");
            btnNext.Width = 28;
            btnNext.Height = 24;
            btnNext.Click += (s, e) => NavigateFile(1);
            pagerStack.Children.Add(btnNext);

            Grid.SetColumn(pagerStack, 1);
            fileBarGrid.Children.Add(pagerStack);

            fileBar.Child = fileBarGrid;
            contentStack.Children.Add(fileBar);

            // Specs container (2 columns: Technical vs EXIF)
            specsPanel = new StackPanel();
            contentStack.Children.Add(specsPanel);

            // Status message for Clean Metadata action
            txtCleanStatus = new TextBlock
            {
                FontSize = 12,
                Foreground = WpfTheme.BrushSuccess,
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 10, 0, 0),
                Visibility = Visibility.Collapsed
            };
            contentStack.Children.Add(txtCleanStatus);

            // --- 2: FOOTER BUTTONS ---
            Grid footerGrid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel leftButtons = new StackPanel { Orientation = Orientation.Horizontal };

            btnCleanMetadata = WpfTheme.CreateSecondaryButton("Strip Metadata");
            btnCleanMetadata.Margin = new Thickness(0, 0, 8, 0);
            btnCleanMetadata.Click += (s, e) => ExecuteStripMetadata();
            leftButtons.Children.Add(btnCleanMetadata);

            btnCopySpecs = WpfTheme.CreateSecondaryButton("Copy Specs");
            btnCopySpecs.Click += (s, e) => ExecuteCopySpecs();
            leftButtons.Children.Add(btnCopySpecs);

            Grid.SetColumn(leftButtons, 0);
            footerGrid.Children.Add(leftButtons);

            btnClose = WpfTheme.CreatePrimaryButton("Close");
            btnClose.Click += (s, e) => this.Close();

            Grid.SetColumn(btnClose, 2);
            footerGrid.Children.Add(btnClose);

            Grid.SetRow(footerGrid, 2);
            rootGrid.Children.Add(footerGrid);

            this.Content = rootGrid;
        }

        private void LoadCurrentFile()
        {
            if (inputFiles.Count == 0)
            {
                txtFileHeader.Text = "No file selected.";
                pagerStackVisibility(false);
                return;
            }

            if (currentIndex < 0) currentIndex = 0;
            if (currentIndex >= inputFiles.Count) currentIndex = inputFiles.Count - 1;

            string filePath = inputFiles[currentIndex];
            txtPager.Text = string.Format("{0} of {1}", currentIndex + 1, inputFiles.Count);
            btnPrev.IsEnabled = (currentIndex > 0);
            btnNext.IsEnabled = (currentIndex < inputFiles.Count - 1);
            pagerStackVisibility(inputFiles.Count > 1);

            try
            {
                currentMeta = ImageEngine.GetMetadata(filePath);
                txtFileHeader.Text = string.Format("{0}  •  {1}  •  {2}",
                    currentMeta.FileName,
                    currentMeta.FormatName,
                    ImageEngine.FormatBytes(currentMeta.FileSizeBytes));

                RenderSpecs();
            }
            catch (Exception ex)
            {
                txtFileHeader.Text = Path.GetFileName(filePath) + " (Error reading file)";
                specsPanel.Children.Clear();
                TextBlock err = new TextBlock
                {
                    Text = "Could not inspect image: " + ex.Message,
                    Foreground = WpfTheme.BrushError,
                    FontSize = 12,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                specsPanel.Children.Add(err);
            }

            txtCleanStatus.Visibility = Visibility.Collapsed;
        }

        private void pagerStackVisibility(bool visible)
        {
            btnPrev.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            txtPager.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            btnNext.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NavigateFile(int delta)
        {
            currentIndex += delta;
            LoadCurrentFile();
        }

        private void RenderSpecs()
        {
            specsPanel.Children.Clear();

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new Thickness(14).Left > 0 ? GridLength.Auto : new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Left Card: Image Dimensions & Color
            Border techCard = CreateSpecCard("Technical Specifications", new SpecRow[]
            {
                new SpecRow("Dimensions", string.Format("{0} × {1} px", currentMeta.Width, currentMeta.Height)),
                new SpecRow("Aspect Ratio", string.Format("{0}:1 ({1})", currentMeta.AspectRatio, GetCommonAspect(currentMeta.Width, currentMeta.Height))),
                new SpecRow("Megapixels", currentMeta.MegaPixels + " MP"),
                new SpecRow("Color Space", currentMeta.ColorDepth),
                new SpecRow("Resolution", string.Format("{0} × {1} DPI", currentMeta.DpiX, currentMeta.DpiY)),
                new SpecRow("Transparency", currentMeta.HasAlpha ? "Yes (Alpha channel)" : "None")
            });
            Grid.SetColumn(techCard, 0);
            grid.Children.Add(techCard);

            // Right Card: EXIF & Camera
            List<SpecRow> exifRows = new List<SpecRow>();
            if (!string.IsNullOrEmpty(currentMeta.CameraMake) || !string.IsNullOrEmpty(currentMeta.CameraModel))
            {
                string camera = (currentMeta.CameraMake + " " + currentMeta.CameraModel).Trim();
                exifRows.Add(new SpecRow("Camera", camera));
            }
            else
            {
                exifRows.Add(new SpecRow("Camera", "None recorded"));
            }

            if (!string.IsNullOrEmpty(currentMeta.LensModel)) exifRows.Add(new SpecRow("Lens", currentMeta.LensModel));
            if (!string.IsNullOrEmpty(currentMeta.DateTaken)) exifRows.Add(new SpecRow("Date Taken", currentMeta.DateTaken));

            List<string> shotParams = new List<string>();
            if (!string.IsNullOrEmpty(currentMeta.ExposureTime)) shotParams.Add(currentMeta.ExposureTime);
            if (!string.IsNullOrEmpty(currentMeta.FNumber)) shotParams.Add(currentMeta.FNumber);
            if (!string.IsNullOrEmpty(currentMeta.IsoSpeed)) shotParams.Add(currentMeta.IsoSpeed);
            if (!string.IsNullOrEmpty(currentMeta.FocalLength)) shotParams.Add(currentMeta.FocalLength);

            if (shotParams.Count > 0)
                exifRows.Add(new SpecRow("Shot Settings", string.Join(" • ", shotParams.ToArray())));
            else
                exifRows.Add(new SpecRow("Shot Settings", "None recorded"));

            if (!string.IsNullOrEmpty(currentMeta.Software)) exifRows.Add(new SpecRow("Software", currentMeta.Software));
            exifRows.Add(new SpecRow("Location (GPS)", !string.IsNullOrEmpty(currentMeta.GpsCoordinates) ? currentMeta.GpsCoordinates : "None (Private)"));

            Border exifCard = CreateSpecCard("Camera & Metadata", exifRows.ToArray());
            Grid.SetColumn(exifCard, 2);
            grid.Children.Add(exifCard);

            specsPanel.Children.Add(grid);
        }

        private Border CreateSpecCard(string cardTitle, SpecRow[] rows)
        {
            Border card = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 12, 14, 12)
            };

            StackPanel stack = new StackPanel();

            TextBlock title = new TextBlock
            {
                Text = cardTitle,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushAccent,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(title);

            foreach (SpecRow row in rows)
            {
                Grid rowGrid = new Grid { Margin = new Thickness(0, 2, 0, 2) };
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                TextBlock lbl = new TextBlock
                {
                    Text = row.Label,
                    FontSize = 11,
                    Foreground = WpfTheme.BrushTextSecondary
                };
                Grid.SetColumn(lbl, 0);
                rowGrid.Children.Add(lbl);

                TextBlock val = new TextBlock
                {
                    Text = row.Value,
                    FontSize = 11,
                    FontWeight = FontWeights.Medium,
                    Foreground = WpfTheme.BrushTextPrimary,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(val, 1);
                rowGrid.Children.Add(val);

                stack.Children.Add(rowGrid);
            }

            card.Child = stack;
            return card;
        }

        private string GetCommonAspect(int w, int h)
        {
            if (w <= 0 || h <= 0) return "Unknown";
            double r = (double)w / h;
            if (Math.Abs(r - 1.0) < 0.05) return "1:1 Square";
            if (Math.Abs(r - (16.0 / 9.0)) < 0.05) return "16:9 Landscape";
            if (Math.Abs(r - (9.0 / 16.0)) < 0.05) return "9:16 Portrait";
            if (Math.Abs(r - (4.0 / 3.0)) < 0.05) return "4:3 Standard";
            if (Math.Abs(r - (3.0 / 2.0)) < 0.05) return "3:2 Photo";
            return "Custom";
        }

        private void ExecuteCopySpecs()
        {
            if (currentMeta == null) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("File: " + currentMeta.FileName);
            sb.AppendLine("Format: " + currentMeta.FormatName);
            sb.AppendLine("Size: " + ImageEngine.FormatBytes(currentMeta.FileSizeBytes));
            sb.AppendLine(string.Format("Dimensions: {0} × {1} px ({2})", currentMeta.Width, currentMeta.Height, currentMeta.MegaPixels + " MP"));
            sb.AppendLine("Color: " + currentMeta.ColorDepth);
            sb.AppendLine(string.Format("DPI: {0} × {1}", currentMeta.DpiX, currentMeta.DpiY));
            if (!string.IsNullOrEmpty(currentMeta.CameraMake) || !string.IsNullOrEmpty(currentMeta.CameraModel))
                sb.AppendLine("Camera: " + (currentMeta.CameraMake + " " + currentMeta.CameraModel).Trim());
            if (!string.IsNullOrEmpty(currentMeta.LensModel))
                sb.AppendLine("Lens: " + currentMeta.LensModel);
            if (!string.IsNullOrEmpty(currentMeta.DateTaken))
                sb.AppendLine("Date Taken: " + currentMeta.DateTaken);

            Clipboard.SetText(sb.ToString());
            txtCleanStatus.Text = "Specifications copied to clipboard.";
            txtCleanStatus.Foreground = WpfTheme.BrushSuccess;
            txtCleanStatus.Visibility = Visibility.Visible;
        }

        private void ExecuteStripMetadata()
        {
            if (currentMeta == null || string.IsNullOrEmpty(currentMeta.FilePath)) return;

            try
            {
                string cleanPath = ImageEngine.StripMetadata(currentMeta.FilePath);
                txtCleanStatus.Text = string.Format("Privacy clean complete: Saved as '{0}'", Path.GetFileName(cleanPath));
                txtCleanStatus.Foreground = WpfTheme.BrushSuccess;
                txtCleanStatus.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                txtCleanStatus.Text = "Could not clean metadata: " + ex.Message;
                txtCleanStatus.Foreground = WpfTheme.BrushError;
                txtCleanStatus.Visibility = Visibility.Visible;
            }
        }
    }
}
