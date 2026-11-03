using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ShapePath = System.Windows.Shapes.Path;

namespace VeqloQuick
{
    public class ConvertWindow : Window
    {
        private List<string> inputFiles;
        private ImageInfo primaryInfo;
        private ImageFormatType selectedTarget = ImageFormatType.Png;
        private int selectedQuality = 90;
        private bool preserveAlpha = true;
        private string customOutputDir = null;

        private Grid rootGrid;
        private StackPanel contentStack;
        private Button btnPrimary;
        private Button btnSecondary;
        private Button btnTertiary;

        private List<string> createdFiles = new List<string>();
        private string errorMessage = "";
        private List<Border> formatCards = new List<Border>();
        private ModernSliderControl qualitySlider;
        private TextBlock qualityLabel;
        private CheckBox chkAlpha;

        private readonly ImageFormatType[] supportedFormats = new ImageFormatType[]
        {
            ImageFormatType.Png,
            ImageFormatType.Jpeg,
            ImageFormatType.WebP,
            ImageFormatType.Bmp,
            ImageFormatType.Tiff,
            ImageFormatType.Ico
        };

        public ConvertWindow(List<string> files)
        {
            this.inputFiles = (files != null && files.Count > 0) ? files : new List<string>();
            this.Title = "Veqlo Quick — Convert";
            this.Width = 540;
            this.Height = 490;
            WpfTheme.ApplyToWindow(this);

            InspectFiles();
            BuildWindowLayout();
            ShowSelectionState();
        }

        private void InspectFiles()
        {
            if (inputFiles == null || inputFiles.Count == 0)
            {
                errorMessage = "No image files provided.";
                return;
            }

            try
            {
                primaryInfo = ImageEngine.Inspect(inputFiles[0]);
                if (primaryInfo.Format == ImageFormatType.Png)
                    selectedTarget = ImageFormatType.WebP;
                else
                    selectedTarget = ImageFormatType.Png;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        private void BuildWindowLayout()
        {
            rootGrid = new Grid { Margin = new Thickness(24) };
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: Header
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 1: Content
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2: Footer

            // --- 0: HEADER ---
            StackPanel headerStack = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };

            StackPanel titleRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            ShapePath icon = LucideIcons.CreatePath(LucideIcons.Image, WpfTheme.BrushAccent, 18);
            icon.Margin = new Thickness(0, 2, 8, 0);
            titleRow.Children.Add(icon);

            string titleStr = (inputFiles.Count > 1)
                ? string.Format("Convert {0} images", inputFiles.Count)
                : "Convert image";

            TextBlock title = new TextBlock
            {
                Text = titleStr,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                FontFamily = WpfTheme.FontDisplay,
                Foreground = WpfTheme.BrushTextPrimary
            };
            titleRow.Children.Add(title);
            headerStack.Children.Add(titleRow);

            TextBlock subtitle = new TextBlock
            {
                Text = "Choose target format, compression fidelity, and output destination.",
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

            // --- 2: FOOTER BUTTONS ---
            Grid footerGrid = new Grid { Margin = new Thickness(0, 16, 0, 0) };
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            btnTertiary = CreateStyledButton("Open Folder", false);
            btnTertiary.Visibility = Visibility.Collapsed;
            Grid.SetColumn(btnTertiary, 0);
            footerGrid.Children.Add(btnTertiary);

            StackPanel rightButtons = new StackPanel { Orientation = Orientation.Horizontal };

            btnSecondary = CreateStyledButton("Cancel", false);
            btnSecondary.Margin = new Thickness(0, 0, 10, 0);
            btnSecondary.Click += (s, e) => this.Close();
            rightButtons.Children.Add(btnSecondary);

            btnPrimary = CreateStyledButton("Convert", true);
            btnPrimary.Click += (s, e) => OnConvertClick();
            rightButtons.Children.Add(btnPrimary);

            Grid.SetColumn(rightButtons, 2);
            footerGrid.Children.Add(rightButtons);

            Grid.SetRow(footerGrid, 2);
            rootGrid.Children.Add(footerGrid);

            this.Content = rootGrid;
        }

        private void ShowSelectionState()
        {
            contentStack.Children.Clear();

            // 1. File Summary Bar
            Border fileBar = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 9, 12, 9),
                Margin = new Thickness(0, 0, 0, 14)
            };

            string fileDesc;
            if (inputFiles.Count > 1)
            {
                long totalBytes = 0;
                foreach (string f in inputFiles)
                {
                    if (File.Exists(f)) totalBytes += new FileInfo(f).Length;
                }
                fileDesc = string.Format("{0} images selected  •  {1} total", inputFiles.Count, ImageEngine.FormatBytes(totalBytes));
            }
            else if (primaryInfo != null)
            {
                fileDesc = string.Format("{0}  •  {1}  •  {2} × {3} px  •  {4}",
                    primaryInfo.FileName,
                    primaryInfo.FormatName,
                    primaryInfo.Width,
                    primaryInfo.Height,
                    ImageEngine.FormatBytes(primaryInfo.FileSizeBytes));
            }
            else
            {
                fileDesc = "No image selected";
            }

            TextBlock txtFile = new TextBlock
            {
                Text = fileDesc,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = WpfTheme.BrushTextPrimary,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            fileBar.Child = txtFile;
            contentStack.Children.Add(fileBar);

            // 2. Format Selection Section
            TextBlock formatLabel = new TextBlock
            {
                Text = "TARGET FORMAT",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 0, 0, 8)
            };
            contentStack.Children.Add(formatLabel);

            formatCards.Clear();
            Grid formatGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            formatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            formatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            formatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            formatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            formatGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            formatGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            formatGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            formatGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int i = 0; i < supportedFormats.Length; i++)
            {
                ImageFormatType fmt = supportedFormats[i];
                int col = (i % 3) * 2;
                int row = (i / 3) * 2;

                Border card = CreateFormatCard(fmt);
                Grid.SetColumn(card, col);
                Grid.SetRow(card, row);
                formatGrid.Children.Add(card);
                formatCards.Add(card);
            }
            contentStack.Children.Add(formatGrid);

            // 3. Format Specific Parameters (Quality / Transparency / Output Dir)
            Border optionsCard = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 6)
            };

            StackPanel optStack = new StackPanel();

            // Quality Row (for lossy formats)
            Grid qualGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            qualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            qualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            qualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

            TextBlock qTitle = new TextBlock
            {
                Text = "Quality fidelity",
                FontSize = 11,
                Foreground = WpfTheme.BrushTextSecondary,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(qTitle, 0);
            qualGrid.Children.Add(qTitle);

            qualitySlider = new ModernSliderControl
            {
                Minimum = 30,
                Maximum = 100,
                Value = selectedQuality,
                VerticalAlignment = VerticalAlignment.Center
            };
            qualitySlider.ValueChanged += (s, val) =>
            {
                selectedQuality = (int)val;
                if (qualityLabel != null) qualityLabel.Text = selectedQuality + "%";
            };
            Grid.SetColumn(qualitySlider, 1);
            qualGrid.Children.Add(qualitySlider);

            qualityLabel = new TextBlock
            {
                Text = selectedQuality + "%",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextPrimary,
                TextAlignment = TextAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(qualityLabel, 2);
            qualGrid.Children.Add(qualityLabel);
            optStack.Children.Add(qualGrid);

            // Transparency Checkbox
            chkAlpha = WpfTheme.CreateCheckBox("Preserve alpha transparency (if supported by format)", preserveAlpha);
            chkAlpha.Margin = new Thickness(0, 0, 0, 8);
            chkAlpha.Checked += (s, e) => preserveAlpha = true;
            chkAlpha.Unchecked += (s, e) => preserveAlpha = false;
            optStack.Children.Add(chkAlpha);

            // Output Folder Row
            Grid outGrid = new Grid();
            outGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            outGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            outGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock outLabel = new TextBlock
            {
                Text = "Destination",
                FontSize = 11,
                Foreground = WpfTheme.BrushTextSecondary,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(outLabel, 0);
            outGrid.Children.Add(outLabel);

            TextBlock outPathText = new TextBlock
            {
                Text = string.IsNullOrEmpty(customOutputDir) ? "Same directory as source" : customOutputDir,
                FontSize = 11,
                Foreground = WpfTheme.BrushTextPrimary,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(outPathText, 1);
            outGrid.Children.Add(outPathText);

            Button btnChooseDir = WpfTheme.CreateSecondaryButton("Browse...");
            btnChooseDir.Height = 26;
            btnChooseDir.Padding = new Thickness(10, 0, 10, 0);
            btnChooseDir.Click += (s, e) =>
            {
                using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
                {
                    dlg.Description = "Select destination folder for converted images";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        customOutputDir = dlg.SelectedPath;
                        outPathText.Text = customOutputDir;
                    }
                }
            };
            Grid.SetColumn(btnChooseDir, 2);
            outGrid.Children.Add(btnChooseDir);
            optStack.Children.Add(outGrid);

            optionsCard.Child = optStack;
            contentStack.Children.Add(optionsCard);

            UpdateCardVisuals();

            btnPrimary.Content = "Convert";
            btnPrimary.IsEnabled = true;
            btnSecondary.Content = "Cancel";
            btnSecondary.Visibility = Visibility.Visible;
            btnTertiary.Visibility = Visibility.Collapsed;
        }

        private Border CreateFormatCard(ImageFormatType fmt)
        {
            Border card = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 9, 12, 9),
                Cursor = Cursors.Hand,
                Tag = fmt
            };

            StackPanel cardStack = new StackPanel();

            TextBlock txtName = new TextBlock
            {
                Text = ImageEngine.GetFormatDisplayName(fmt),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextPrimary
            };
            cardStack.Children.Add(txtName);

            string desc;
            switch (fmt)
            {
                case ImageFormatType.Png: desc = "Lossless • Alpha"; break;
                case ImageFormatType.Jpeg: desc = "Standard photo"; break;
                case ImageFormatType.WebP: desc = "Modern web"; break;
                case ImageFormatType.Bmp: desc = "Raw bitmap"; break;
                case ImageFormatType.Tiff: desc = "Print archive"; break;
                case ImageFormatType.Ico: desc = "Multi-res icon"; break;
                default: desc = ""; break;
            }

            TextBlock txtDesc = new TextBlock
            {
                Text = desc,
                FontSize = 10,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 2, 0, 0)
            };
            cardStack.Children.Add(txtDesc);

            card.Child = cardStack;

            card.MouseEnter += (s, e) =>
            {
                if ((ImageFormatType)card.Tag != selectedTarget)
                {
                    card.Background = WpfTheme.BrushSurfaceHover;
                    card.BorderBrush = WpfTheme.BrushBorderHover;
                }
            };
            card.MouseLeave += (s, e) =>
            {
                if ((ImageFormatType)card.Tag != selectedTarget)
                {
                    card.Background = WpfTheme.BrushSurface;
                    card.BorderBrush = WpfTheme.BrushBorder;
                }
            };
            card.MouseDown += (s, e) =>
            {
                selectedTarget = (ImageFormatType)card.Tag;
                UpdateCardVisuals();
            };

            return card;
        }

        private void UpdateCardVisuals()
        {
            foreach (Border card in formatCards)
            {
                ImageFormatType fmt = (ImageFormatType)card.Tag;
                if (fmt == selectedTarget)
                {
                    card.Background = WpfTheme.BrushSurfaceSelected;
                    card.BorderBrush = WpfTheme.BrushBorderSelected;
                    card.BorderThickness = new Thickness(1.5);
                }
                else
                {
                    card.Background = WpfTheme.BrushSurface;
                    card.BorderBrush = WpfTheme.BrushBorder;
                    card.BorderThickness = new Thickness(1);
                }
            }

            // Quality slider enabled only for lossy formats
            if (qualitySlider != null)
            {
                bool isLossy = (selectedTarget == ImageFormatType.Jpeg || selectedTarget == ImageFormatType.WebP);
                qualitySlider.IsEnabled = isLossy;
                qualityLabel.Opacity = isLossy ? 1.0 : 0.4;
            }
        }

        private void OnConvertClick()
        {
            contentStack.Children.Clear();
            btnPrimary.IsEnabled = false;
            btnSecondary.IsEnabled = false;

            StackPanel progStack = new StackPanel { Margin = new Thickness(0, 30, 0, 0) };

            TextBlock txtStatus = new TextBlock
            {
                Text = string.Format("Converting {0} to {1}...",
                    inputFiles.Count > 1 ? inputFiles.Count + " images" : Path.GetFileName(inputFiles[0]),
                    ImageEngine.GetFormatDisplayName(selectedTarget)),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };
            progStack.Children.Add(txtStatus);

            ModernProgressBarControl pb = new ModernProgressBarControl
            {
                Width = 380,
                Margin = new Thickness(0, 0, 0, 12),
                Value = 70
            };
            progStack.Children.Add(pb);

            contentStack.Children.Add(progStack);

            createdFiles.Clear();
            errorMessage = "";

            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    foreach (string file in inputFiles)
                    {
                        string outPath = ImageEngine.Convert(file, selectedTarget, selectedQuality, preserveAlpha, customOutputDir);
                        createdFiles.Add(outPath);
                    }

                    this.Dispatcher.Invoke((Action)delegate
                    {
                        ShowCompletedState();
                    });
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;
                    this.Dispatcher.Invoke((Action)delegate
                    {
                        ShowErrorState();
                    });
                }
            });
        }

        private void ShowCompletedState()
        {
            contentStack.Children.Clear();

            StackPanel compStack = new StackPanel { Margin = new Thickness(0, 20, 0, 0) };

            Border iconCircle = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(22),
                Background = new SolidColorBrush(Color.FromArgb(30, 16, 185, 129)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            };
            ShapePath chk = LucideIcons.CreatePath(LucideIcons.Check, WpfTheme.BrushSuccess, 22, 2.2);
            iconCircle.Child = chk;
            compStack.Children.Add(iconCircle);

            TextBlock title = new TextBlock
            {
                Text = createdFiles.Count > 1 ? "Images Converted Successfully" : "Image Converted Successfully",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                FontFamily = WpfTheme.FontDisplay,
                Foreground = WpfTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            compStack.Children.Add(title);

            // Result summary
            Border resultCard = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 10, 0, 0)
            };

            StackPanel resStack = new StackPanel();
            if (createdFiles.Count == 1)
            {
                FileInfo fi = new FileInfo(createdFiles[0]);
                TextBlock fn = new TextBlock
                {
                    Text = fi.Name,
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = WpfTheme.BrushTextPrimary,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                resStack.Children.Add(fn);

                TextBlock sub = new TextBlock
                {
                    Text = string.Format("{0}  •  {1}", ImageEngine.GetFormatDisplayName(selectedTarget), ImageEngine.FormatBytes(fi.Length)),
                    FontSize = 11,
                    Foreground = WpfTheme.BrushTextSecondary,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                resStack.Children.Add(sub);
            }
            else
            {
                long totalBytes = 0;
                foreach (string f in createdFiles)
                {
                    if (File.Exists(f)) totalBytes += new FileInfo(f).Length;
                }
                TextBlock fn = new TextBlock
                {
                    Text = string.Format("{0} files created in {1}", createdFiles.Count, ImageEngine.GetFormatDisplayName(selectedTarget)),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = WpfTheme.BrushTextPrimary
                };
                resStack.Children.Add(fn);

                TextBlock sub = new TextBlock
                {
                    Text = "Total output size: " + ImageEngine.FormatBytes(totalBytes),
                    FontSize = 11,
                    Foreground = WpfTheme.BrushTextSecondary,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                resStack.Children.Add(sub);
            }

            resultCard.Child = resStack;
            compStack.Children.Add(resultCard);

            contentStack.Children.Add(compStack);

            btnPrimary.Content = "Done";
            btnPrimary.IsEnabled = true;
            btnPrimary.Click -= (s, e) => OnConvertClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Content = "Open File";
            btnSecondary.IsEnabled = true;
            btnSecondary.Visibility = (createdFiles.Count == 1) ? Visibility.Visible : Visibility.Collapsed;
            btnSecondary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    Process.Start(new ProcessStartInfo(createdFiles[0]) { UseShellExecute = true });
                }
            };

            btnTertiary.Content = "Show in Folder";
            btnTertiary.Visibility = Visibility.Visible;
            btnTertiary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    Process.Start("explorer.exe", string.Format("/select,\"{0}\"", createdFiles[0]));
                }
            };
        }

        private void ShowErrorState()
        {
            contentStack.Children.Clear();

            StackPanel errStack = new StackPanel { Margin = new Thickness(0, 30, 0, 0) };

            Border iconCircle = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(22),
                Background = new SolidColorBrush(Color.FromArgb(30, 239, 68, 68)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12)
            };
            ShapePath xIcon = LucideIcons.CreatePath(LucideIcons.X, WpfTheme.BrushError, 22, 2.2);
            iconCircle.Child = xIcon;
            errStack.Children.Add(iconCircle);

            TextBlock title = new TextBlock
            {
                Text = "Conversion Failed",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            errStack.Children.Add(title);

            TextBlock msg = new TextBlock
            {
                Text = errorMessage,
                FontSize = 12,
                Foreground = WpfTheme.BrushTextSecondary,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 400
            };
            errStack.Children.Add(msg);

            contentStack.Children.Add(errStack);

            btnPrimary.Content = "Close";
            btnPrimary.IsEnabled = true;
            btnPrimary.Click -= (s, e) => OnConvertClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Visibility = Visibility.Collapsed;
            btnTertiary.Visibility = Visibility.Collapsed;
        }

        private Button CreateStyledButton(string text, bool isPrimary)
        {
            return isPrimary ? WpfTheme.CreatePrimaryButton(text) : WpfTheme.CreateSecondaryButton(text);
        }
    }
}
