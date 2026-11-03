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
    public class CompressWindow : Window
    {
        private List<string> inputFiles;
        private ImageInfo primaryInfo;
        private int selectedQuality = 75;
        private bool stripMetadata = true;
        private bool isTargetSizeMode = false;
        private long targetSizeBytes = 500 * 1024; // 500 KB default
        private string customOutputDir = null;

        private Grid rootGrid;
        private StackPanel contentStack;
        private Button btnPrimary;
        private Button btnSecondary;
        private Button btnTertiary;
        private TextBlock qualityLabel;
        private ModernSliderControl qualitySlider;
        private TextBlock txtSavingsEstimate;
        private TextBox txtTargetKB;

        private List<string> createdFiles = new List<string>();
        private long originalTotalBytes = 0;
        private long compressedTotalBytes = 0;
        private string errorMessage = "";
        private List<Border> presetCards = new List<Border>();

        public CompressWindow(List<string> files)
        {
            this.inputFiles = (files != null && files.Count > 0) ? files : new List<string>();
            this.Title = "Veqlo Quick — Compress";
            this.Width = 540;
            this.Height = 490;
            WpfTheme.ApplyToWindow(this);

            InspectFiles();
            BuildWindowLayout();
            ShowOptionsState();
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
                originalTotalBytes = 0;
                foreach (string f in inputFiles)
                {
                    if (File.Exists(f)) originalTotalBytes += new FileInfo(f).Length;
                }
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
            ShapePath icon = LucideIcons.CreatePath(LucideIcons.Compress, WpfTheme.BrushAccent, 18);
            icon.Margin = new Thickness(0, 2, 8, 0);
            titleRow.Children.Add(icon);

            string titleStr = (inputFiles.Count > 1)
                ? string.Format("Compress {0} images", inputFiles.Count)
                : "Compress image";

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
                Text = "Optimize image encoding, adjust quality factor, and eliminate metadata overhead.",
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

            btnPrimary = CreateStyledButton("Compress", true);
            btnPrimary.Click += (s, e) => OnCompressClick();
            rightButtons.Children.Add(btnPrimary);

            Grid.SetColumn(rightButtons, 2);
            footerGrid.Children.Add(rightButtons);

            Grid.SetRow(footerGrid, 2);
            rootGrid.Children.Add(footerGrid);

            this.Content = rootGrid;
        }

        private void ShowOptionsState()
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
                fileDesc = string.Format("{0} images selected  •  {1} total size",
                    inputFiles.Count, ImageEngine.FormatBytes(originalTotalBytes));
            }
            else if (primaryInfo != null)
            {
                fileDesc = string.Format("{0}  •  {1}  •  {2} × {3} px  •  {4}",
                    primaryInfo.FileName,
                    primaryInfo.FormatName,
                    primaryInfo.Width,
                    primaryInfo.Height,
                    ImageEngine.FormatBytes(originalTotalBytes));
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

            // 2. Preset Cards (Balanced, Maximum, Light, Target Size)
            TextBlock presetTitle = new TextBlock
            {
                Text = "COMPRESSION PRESET",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 0, 0, 8)
            };
            contentStack.Children.Add(presetTitle);

            presetCards.Clear();
            Grid presetGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            presetGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border cardBalanced = CreatePresetCard("Balanced", "75% quality", "Optimal web & storage", 75, false);
            Grid.SetColumn(cardBalanced, 0);
            presetGrid.Children.Add(cardBalanced);
            presetCards.Add(cardBalanced);

            Border cardMax = CreatePresetCard("Maximum", "50% quality", "Smallest file size", 50, false);
            Grid.SetColumn(cardMax, 2);
            presetGrid.Children.Add(cardMax);
            presetCards.Add(cardMax);

            Border cardLight = CreatePresetCard("Light", "90% quality", "Preserve fine detail", 90, false);
            Grid.SetColumn(cardLight, 4);
            presetGrid.Children.Add(cardLight);
            presetCards.Add(cardLight);

            contentStack.Children.Add(presetGrid);

            // 3. Fine Tuning & Settings Card
            Border optCard = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 6)
            };

            StackPanel optStack = new StackPanel();

            // Quality Slider Row
            Grid qualGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            qualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            qualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            qualGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

            TextBlock qTitle = new TextBlock
            {
                Text = "Fine-tune quality",
                FontSize = 11,
                Foreground = WpfTheme.BrushTextSecondary,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(qTitle, 0);
            qualGrid.Children.Add(qTitle);

            qualitySlider = new ModernSliderControl
            {
                Minimum = 15,
                Maximum = 95,
                Value = selectedQuality,
                VerticalAlignment = VerticalAlignment.Center
            };
            qualitySlider.ValueChanged += (s, val) =>
            {
                selectedQuality = (int)val;
                if (qualityLabel != null) qualityLabel.Text = selectedQuality + "%";
                UpdateSavingsEstimate();
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

            // Strip Metadata Checkbox
            CheckBox chkMeta = WpfTheme.CreateCheckBox("Strip EXIF camera & GPS metadata (recommended for privacy & extra savings)", stripMetadata);
            chkMeta.Margin = new Thickness(0, 0, 0, 8);
            chkMeta.Checked += (s, e) => stripMetadata = true;
            chkMeta.Unchecked += (s, e) => stripMetadata = false;
            optStack.Children.Add(chkMeta);

            // Target max size row (optional ceiling)
            Grid targetSizeGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            targetSizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            targetSizeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            CheckBox chkTarget = WpfTheme.CreateCheckBox("Enforce max file size ceiling (KB):", isTargetSizeMode);
            chkTarget.VerticalAlignment = VerticalAlignment.Center;
            chkTarget.Checked += (s, e) => { isTargetSizeMode = true; txtTargetKB.IsEnabled = true; };
            chkTarget.Unchecked += (s, e) => { isTargetSizeMode = false; txtTargetKB.IsEnabled = false; };
            Grid.SetColumn(chkTarget, 0);
            targetSizeGrid.Children.Add(chkTarget);

            txtTargetKB = WpfTheme.CreateTextBox((targetSizeBytes / 1024).ToString());
            txtTargetKB.Width = 70;
            txtTargetKB.Height = 26;
            txtTargetKB.IsEnabled = false;
            txtTargetKB.VerticalAlignment = VerticalAlignment.Center;
            txtTargetKB.TextChanged += (s, e) =>
            {
                long val;
                if (long.TryParse(txtTargetKB.Text.Trim(), out val) && val > 0)
                {
                    targetSizeBytes = val * 1024;
                }
            };
            Grid.SetColumn(txtTargetKB, 1);
            targetSizeGrid.Children.Add(txtTargetKB);
            optStack.Children.Add(targetSizeGrid);

            // Output Folder Row
            Grid outGrid = new Grid();
            outGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
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
                    dlg.Description = "Select destination folder for compressed images";
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

            optCard.Child = optStack;
            contentStack.Children.Add(optCard);

            // Savings Projection Pill
            txtSavingsEstimate = new TextBlock
            {
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = WpfTheme.BrushAccent,
                Margin = new Thickness(2, 6, 0, 0)
            };
            contentStack.Children.Add(txtSavingsEstimate);

            UpdatePresetCardVisuals();
            UpdateSavingsEstimate();

            btnPrimary.Content = "Compress";
            btnPrimary.IsEnabled = true;
            btnSecondary.Content = "Cancel";
            btnSecondary.Visibility = Visibility.Visible;
            btnTertiary.Visibility = Visibility.Collapsed;
        }

        private Border CreatePresetCard(string title, string rate, string desc, int qualityVal, bool isTarget)
        {
            Border card = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 9, 12, 9),
                Cursor = Cursors.Hand,
                Tag = qualityVal
            };

            StackPanel stack = new StackPanel();

            TextBlock t = new TextBlock
            {
                Text = title,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextPrimary
            };
            stack.Children.Add(t);

            TextBlock r = new TextBlock
            {
                Text = rate,
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = WpfTheme.BrushAccent,
                Margin = new Thickness(0, 2, 0, 0)
            };
            stack.Children.Add(r);

            TextBlock d = new TextBlock
            {
                Text = desc,
                FontSize = 10,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 2, 0, 0)
            };
            stack.Children.Add(d);

            card.Child = stack;

            card.MouseEnter += (s, e) =>
            {
                if ((int)card.Tag != selectedQuality)
                {
                    card.Background = WpfTheme.BrushSurfaceHover;
                    card.BorderBrush = WpfTheme.BrushBorderHover;
                }
            };
            card.MouseLeave += (s, e) =>
            {
                if ((int)card.Tag != selectedQuality)
                {
                    card.Background = WpfTheme.BrushSurface;
                    card.BorderBrush = WpfTheme.BrushBorder;
                }
            };
            card.MouseDown += (s, e) =>
            {
                selectedQuality = (int)card.Tag;
                if (qualitySlider != null) qualitySlider.Value = selectedQuality;
                UpdatePresetCardVisuals();
                UpdateSavingsEstimate();
            };

            return card;
        }

        private void UpdatePresetCardVisuals()
        {
            foreach (Border card in presetCards)
            {
                int q = (int)card.Tag;
                if (q == selectedQuality)
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
        }

        private void UpdateSavingsEstimate()
        {
            if (txtSavingsEstimate == null) return;

            // Project estimated size based on quality factor
            double estRatio;
            if (selectedQuality >= 85) estRatio = 0.55;
            else if (selectedQuality >= 70) estRatio = 0.35;
            else estRatio = 0.22;

            long estBytes = (long)(originalTotalBytes * estRatio);
            int estPct = (int)Math.Round((1.0 - estRatio) * 100);

            txtSavingsEstimate.Text = string.Format("Estimated output: ~{0} (approx. ~{1}% space reduction)",
                ImageEngine.FormatBytes(estBytes), estPct);
        }

        private void OnCompressClick()
        {
            contentStack.Children.Clear();
            btnPrimary.IsEnabled = false;
            btnSecondary.IsEnabled = false;

            StackPanel progStack = new StackPanel { Margin = new Thickness(0, 30, 0, 0) };

            TextBlock txtStatus = new TextBlock
            {
                Text = string.Format("Compressing {0} at {1}% quality...",
                    inputFiles.Count > 1 ? inputFiles.Count + " images" : Path.GetFileName(inputFiles[0]),
                    selectedQuality),
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
            compressedTotalBytes = 0;
            errorMessage = "";

            ThreadPool.QueueUserWorkItem((state) =>
            {
                try
                {
                    foreach (string file in inputFiles)
                    {
                        long origSz, newSz;
                        string outPath;
                        if (isTargetSizeMode && targetSizeBytes > 0)
                        {
                            outPath = ImageEngine.CompressToTargetSize(file, targetSizeBytes, stripMetadata, out origSz, out newSz, customOutputDir);
                        }
                        else
                        {
                            outPath = ImageEngine.Compress(file, selectedQuality, stripMetadata, out origSz, out newSz, customOutputDir);
                        }

                        createdFiles.Add(outPath);
                        compressedTotalBytes += newSz;
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
                Text = createdFiles.Count > 1 ? "Images Compressed Successfully" : "Image Compressed Successfully",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                FontFamily = WpfTheme.FontDisplay,
                Foreground = WpfTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            compStack.Children.Add(title);

            long bytesSaved = Math.Max(0, originalTotalBytes - compressedTotalBytes);
            double pctSaved = originalTotalBytes > 0 ? (double)bytesSaved / originalTotalBytes * 100.0 : 0;

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

            TextBlock saveText = new TextBlock
            {
                Text = string.Format("Saved {0:0.#}%  •  {1} reduced to {2}",
                    pctSaved,
                    ImageEngine.FormatBytes(originalTotalBytes),
                    ImageEngine.FormatBytes(compressedTotalBytes)),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushSuccess,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            resStack.Children.Add(saveText);

            TextBlock fileInfoText = new TextBlock
            {
                Text = (createdFiles.Count == 1)
                    ? Path.GetFileName(createdFiles[0])
                    : string.Format("{0} files optimized successfully", createdFiles.Count),
                FontSize = 11,
                Foreground = WpfTheme.BrushTextSecondary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0)
            };
            resStack.Children.Add(fileInfoText);

            resultCard.Child = resStack;
            compStack.Children.Add(resultCard);

            contentStack.Children.Add(compStack);

            btnPrimary.Content = "Done";
            btnPrimary.IsEnabled = true;
            btnPrimary.Click -= (s, e) => OnCompressClick();
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
                Text = "Compression Ineffective or Failed",
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
            btnPrimary.Click -= (s, e) => OnCompressClick();
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
