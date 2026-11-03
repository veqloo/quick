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
    public class ResizeWindow : Window
    {
        private List<string> inputFiles;
        private ImageInfo primaryInfo;

        private Grid rootGrid;
        private StackPanel contentStack;
        private Button btnPrimary;
        private Button btnSecondary;
        private Button btnTertiary;

        private TextBox txtWidth;
        private TextBox txtHeight;
        private Button btnLockRatio;
        private bool isRatioLocked = true;
        private bool isUpdatingText = false;

        private ResizeFitMode selectedFitMode = ResizeFitMode.Fit;
        private bool autoSwapOrientation = true;
        private bool preventUpscale = false;
        private string customOutputDir = null;

        private List<string> createdFiles = new List<string>();
        private int finalWidth = 0;
        private int finalHeight = 0;
        private string errorMessage = "";

        private List<Border> fitModeCards = new List<Border>();
        private List<Border> scalePresetCards = new List<Border>();

        public ResizeWindow(List<string> files)
        {
            this.inputFiles = (files != null && files.Count > 0) ? files : new List<string>();
            this.Title = "Veqlo Quick — Resize";
            this.Width = 540;
            this.Height = 520;
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
                finalWidth = primaryInfo.Width;
                finalHeight = primaryInfo.Height;
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
            ShapePath icon = LucideIcons.CreatePath(LucideIcons.Resize, WpfTheme.BrushAccent, 18);
            icon.Margin = new Thickness(0, 2, 8, 0);
            titleRow.Children.Add(icon);

            string titleStr = (inputFiles.Count > 1)
                ? string.Format("Resize {0} images", inputFiles.Count)
                : "Resize image";

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
                Text = "Scale dimensions, lock aspect ratio, and choose fit, crop or stretch mode.",
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

            btnPrimary = CreateStyledButton("Resize", true);
            btnPrimary.Click += (s, e) => OnResizeClick();
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
                Margin = new Thickness(0, 0, 0, 12)
            };

            string fileDesc;
            if (inputFiles.Count > 1)
            {
                fileDesc = string.Format("{0} images selected  •  Original dimensions vary", inputFiles.Count);
            }
            else if (primaryInfo != null)
            {
                fileDesc = string.Format("{0}  •  {1} × {2} px  •  {3}",
                    primaryInfo.FileName,
                    primaryInfo.Width,
                    primaryInfo.Height,
                    primaryInfo.FormatName);
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

            // 2. Sizing Mode Selector (Fit vs Fill vs Stretch)
            Grid fitModeGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            fitModeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fitModeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            fitModeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            fitModeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            fitModeGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            fitModeCards.Clear();
            Border cardFit = CreateFitModeCard(ResizeFitMode.Fit, "Fit", "Keep ratio in bounds");
            Grid.SetColumn(cardFit, 0);
            fitModeGrid.Children.Add(cardFit);
            fitModeCards.Add(cardFit);

            Border cardFill = CreateFitModeCard(ResizeFitMode.Fill, "Fill", "Crop to exact size");
            Grid.SetColumn(cardFill, 2);
            fitModeGrid.Children.Add(cardFill);
            fitModeCards.Add(cardFill);

            Border cardStretch = CreateFitModeCard(ResizeFitMode.Stretch, "Stretch", "Exact W × H dimensions");
            Grid.SetColumn(cardStretch, 4);
            fitModeGrid.Children.Add(cardStretch);
            fitModeCards.Add(cardStretch);

            contentStack.Children.Add(fitModeGrid);

            // 3. Quick Scale Presets (25%, 50%, 75%, 100%, 150%, 200%)
            TextBlock scaleTitle = new TextBlock
            {
                Text = "QUICK SCALE",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 0, 0, 6)
            };
            contentStack.Children.Add(scaleTitle);

            scalePresetCards.Clear();
            Grid scaleGrid = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            for (int i = 0; i < 6; i++)
            {
                scaleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                if (i < 5) scaleGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(6) });
            }

            int[] scales = new int[] { 25, 50, 75, 100, 150, 200 };
            for (int i = 0; i < scales.Length; i++)
            {
                int sc = scales[i];
                Border card = CreateScaleCard(sc);
                Grid.SetColumn(card, i * 2);
                scaleGrid.Children.Add(card);
                scalePresetCards.Add(card);
            }
            contentStack.Children.Add(scaleGrid);

            // 4. Custom Dimension Inputs with Aspect Ratio Lock
            Border dimCard = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(12, 10, 12, 10),
                Margin = new Thickness(0, 0, 0, 6)
            };

            StackPanel dimStack = new StackPanel();

            Grid dimRow = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            dimRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            dimRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            dimRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Width Stack
            StackPanel wStack = new StackPanel();
            wStack.Children.Add(new TextBlock
            {
                Text = "Width (px)",
                FontSize = 11,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 0, 0, 4)
            });

            txtWidth = WpfTheme.CreateTextBox(finalWidth.ToString());
            txtWidth.FontSize = 13;
            txtWidth.FontWeight = FontWeights.SemiBold;
            txtWidth.TextChanged += OnWidthChanged;
            wStack.Children.Add(txtWidth);
            Grid.SetColumn(wStack, 0);
            dimRow.Children.Add(wStack);

            // Aspect Lock Button
            btnLockRatio = WpfTheme.CreateSecondaryButton("🔒 Locked");
            btnLockRatio.FontSize = 11;
            btnLockRatio.Height = 30;
            btnLockRatio.Padding = new Thickness(10, 0, 10, 0);
            btnLockRatio.VerticalAlignment = VerticalAlignment.Bottom;
            btnLockRatio.Margin = new Thickness(10, 0, 10, 0);
            btnLockRatio.Click += ToggleRatioLock;
            Grid.SetColumn(btnLockRatio, 1);
            dimRow.Children.Add(btnLockRatio);

            // Height Stack
            StackPanel hStack = new StackPanel();
            hStack.Children.Add(new TextBlock
            {
                Text = "Height (px)",
                FontSize = 11,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 0, 0, 4)
            });

            txtHeight = WpfTheme.CreateTextBox(finalHeight.ToString());
            txtHeight.FontSize = 13;
            txtHeight.FontWeight = FontWeights.SemiBold;
            txtHeight.TextChanged += OnHeightChanged;
            hStack.Children.Add(txtHeight);
            Grid.SetColumn(hStack, 2);
            dimRow.Children.Add(hStack);

            dimStack.Children.Add(dimRow);

            // Checkboxes
            CheckBox chkSwap = WpfTheme.CreateCheckBox("Auto-swap width and height for portrait orientation photos", autoSwapOrientation);
            chkSwap.Margin = new Thickness(0, 0, 0, 6);
            chkSwap.Checked += (s, e) => autoSwapOrientation = true;
            chkSwap.Unchecked += (s, e) => autoSwapOrientation = false;
            dimStack.Children.Add(chkSwap);

            CheckBox chkUpscale = WpfTheme.CreateCheckBox("Do not enlarge if image is already smaller than target (prevents pixelation)", preventUpscale);
            chkUpscale.Margin = new Thickness(0, 0, 0, 6);
            chkUpscale.Checked += (s, e) => preventUpscale = true;
            chkUpscale.Unchecked += (s, e) => preventUpscale = false;
            dimStack.Children.Add(chkUpscale);

            // Destination Folder Row
            Grid outGrid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
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
                    dlg.Description = "Select destination folder for resized images";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        customOutputDir = dlg.SelectedPath;
                        outPathText.Text = customOutputDir;
                    }
                }
            };
            Grid.SetColumn(btnChooseDir, 2);
            outGrid.Children.Add(btnChooseDir);
            dimStack.Children.Add(outGrid);

            dimCard.Child = dimStack;
            contentStack.Children.Add(dimCard);

            UpdateFitModeVisuals();

            btnPrimary.Content = "Resize";
            btnPrimary.IsEnabled = true;
            btnSecondary.Content = "Cancel";
            btnSecondary.Visibility = Visibility.Visible;
            btnTertiary.Visibility = Visibility.Collapsed;
        }

        private Border CreateFitModeCard(ResizeFitMode mode, string title, string desc)
        {
            Border card = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 8, 10, 8),
                Cursor = Cursors.Hand,
                Tag = mode
            };

            StackPanel stack = new StackPanel();

            TextBlock t = new TextBlock
            {
                Text = title,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = WpfTheme.BrushTextPrimary
            };
            stack.Children.Add(t);

            TextBlock d = new TextBlock
            {
                Text = desc,
                FontSize = 10,
                Foreground = WpfTheme.BrushTextSecondary,
                Margin = new Thickness(0, 1, 0, 0)
            };
            stack.Children.Add(d);

            card.Child = stack;

            card.MouseDown += (s, e) =>
            {
                selectedFitMode = (ResizeFitMode)card.Tag;
                UpdateFitModeVisuals();
            };

            return card;
        }

        private void UpdateFitModeVisuals()
        {
            foreach (Border card in fitModeCards)
            {
                ResizeFitMode mode = (ResizeFitMode)card.Tag;
                if (mode == selectedFitMode)
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

        private Border CreateScaleCard(int pct)
        {
            Border card = new Border
            {
                Background = WpfTheme.BrushSurface,
                BorderBrush = WpfTheme.BrushBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(0, 6, 0, 6),
                Cursor = Cursors.Hand,
                Tag = pct
            };

            TextBlock t = new TextBlock
            {
                Text = pct + "%",
                FontSize = 11,
                FontWeight = FontWeights.Medium,
                Foreground = WpfTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            card.Child = t;

            card.MouseEnter += (s, e) =>
            {
                card.Background = WpfTheme.BrushSurfaceHover;
                card.BorderBrush = WpfTheme.BrushBorderHover;
            };
            card.MouseLeave += (s, e) =>
            {
                card.Background = WpfTheme.BrushSurface;
                card.BorderBrush = WpfTheme.BrushBorder;
            };
            card.MouseDown += (s, e) =>
            {
                ApplyScalePreset((int)card.Tag);
            };

            return card;
        }

        private void ApplyScalePreset(int pct)
        {
            if (primaryInfo == null || primaryInfo.Width <= 0 || primaryInfo.Height <= 0) return;
            isUpdatingText = true;
            finalWidth = Math.Max(1, (int)Math.Round(primaryInfo.Width * (pct / 100.0)));
            finalHeight = Math.Max(1, (int)Math.Round(primaryInfo.Height * (pct / 100.0)));
            txtWidth.Text = finalWidth.ToString();
            txtHeight.Text = finalHeight.ToString();
            isUpdatingText = false;
        }

        private void ToggleRatioLock(object sender, RoutedEventArgs e)
        {
            isRatioLocked = !isRatioLocked;
            if (isRatioLocked)
            {
                btnLockRatio.Content = "🔒 Locked";
                btnLockRatio.Background = WpfTheme.BrushAccentMuted;
                btnLockRatio.BorderBrush = WpfTheme.BrushAccent;
                btnLockRatio.Foreground = WpfTheme.BrushAccent;

                if (primaryInfo != null && primaryInfo.Width > 0 && primaryInfo.Height > 0)
                {
                    double ratio = (double)primaryInfo.Width / primaryInfo.Height;
                    finalHeight = Math.Max(1, (int)Math.Round(finalWidth / ratio));
                    txtHeight.Text = finalHeight.ToString();
                }
            }
            else
            {
                btnLockRatio.Content = "🔓 Free";
                btnLockRatio.Background = WpfTheme.BrushSurface;
                btnLockRatio.BorderBrush = WpfTheme.BrushBorder;
                btnLockRatio.Foreground = WpfTheme.BrushTextSecondary;
            }
        }

        private void OnWidthChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingText) return;
            int w;
            if (int.TryParse(txtWidth.Text.Trim(), out w) && w > 0)
            {
                finalWidth = w;
                if (isRatioLocked && primaryInfo != null && primaryInfo.Width > 0 && primaryInfo.Height > 0)
                {
                    isUpdatingText = true;
                    double ratio = (double)primaryInfo.Width / primaryInfo.Height;
                    finalHeight = Math.Max(1, (int)Math.Round(w / ratio));
                    txtHeight.Text = finalHeight.ToString();
                    isUpdatingText = false;
                }
            }
        }

        private void OnHeightChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingText) return;
            int h;
            if (int.TryParse(txtHeight.Text.Trim(), out h) && h > 0)
            {
                finalHeight = h;
                if (isRatioLocked && primaryInfo != null && primaryInfo.Width > 0 && primaryInfo.Height > 0)
                {
                    isUpdatingText = true;
                    double ratio = (double)primaryInfo.Width / primaryInfo.Height;
                    finalWidth = Math.Max(1, (int)Math.Round(h * ratio));
                    txtWidth.Text = finalWidth.ToString();
                    isUpdatingText = false;
                }
            }
        }

        private void OnResizeClick()
        {
            if (finalWidth <= 0 || finalHeight <= 0)
            {
                MessageBox.Show("Please enter valid positive dimensions.", "Veqlo Quick", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            contentStack.Children.Clear();
            btnPrimary.IsEnabled = false;
            btnSecondary.IsEnabled = false;

            StackPanel progStack = new StackPanel { Margin = new Thickness(0, 30, 0, 0) };

            TextBlock txtStatus = new TextBlock
            {
                Text = string.Format("Resizing {0} to {1} × {2} px...",
                    inputFiles.Count > 1 ? inputFiles.Count + " images" : Path.GetFileName(inputFiles[0]),
                    finalWidth, finalHeight),
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
                        int resW, resH;
                        string outPath = ImageEngine.Resize(file, finalWidth, finalHeight, selectedFitMode, autoSwapOrientation, preventUpscale, out resW, out resH, customOutputDir);
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
                Text = createdFiles.Count > 1 ? "Images Resized Successfully" : "Image Resized Successfully",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                FontFamily = WpfTheme.FontDisplay,
                Foreground = WpfTheme.BrushTextPrimary,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };
            compStack.Children.Add(title);

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
                    Text = string.Format("Target: {0} × {1} px  •  Mode: {2}  •  {3}",
                        finalWidth, finalHeight, selectedFitMode, ImageEngine.FormatBytes(fi.Length)),
                    FontSize = 11,
                    Foreground = WpfTheme.BrushTextSecondary,
                    Margin = new Thickness(0, 2, 0, 0)
                };
                resStack.Children.Add(sub);
            }
            else
            {
                TextBlock fn = new TextBlock
                {
                    Text = string.Format("{0} files resized to {1} × {2} px", createdFiles.Count, finalWidth, finalHeight),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = WpfTheme.BrushTextPrimary
                };
                resStack.Children.Add(fn);

                TextBlock sub = new TextBlock
                {
                    Text = "Scaling mode: " + selectedFitMode,
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
            btnPrimary.Click -= (s, e) => OnResizeClick();
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
                Text = "Resize Failed",
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
            btnPrimary.Click -= (s, e) => OnResizeClick();
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
