using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace VeqloQuick
{
    public static class UiTheme
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int DWMWCP_ROUND = 2;

        public static bool IsDarkMode = true;
        public static Color ColBg;
        public static Color ColCard;
        public static Color ColCardHover;
        public static Color ColBorder;
        public static Color ColBorderSelected;
        public static Color ColTextPrimary;
        public static Color ColTextSecondary;
        public static Color ColAccent;
        public static Color ColAccentHover;
        public static Color ColSuccess;
        public static Color ColError;

        public static Font FontTitle;
        public static Font FontHeading;
        public static Font FontBody;
        public static Font FontSubtext;
        public static Font FontBadge;
        public static Font FontLargeStat;

        static UiTheme()
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
                ColBg = Color.FromArgb(24, 24, 27);          // Zinc 900
                ColCard = Color.FromArgb(39, 39, 42);        // Zinc 800
                ColCardHover = Color.FromArgb(50, 50, 54);
                ColBorder = Color.FromArgb(63, 63, 70);      // Zinc 700
                ColBorderSelected = Color.FromArgb(96, 165, 250); // Blue 400
                ColTextPrimary = Color.FromArgb(244, 244, 245); // Zinc 100
                ColTextSecondary = Color.FromArgb(161, 161, 170); // Zinc 400
                ColAccent = Color.FromArgb(59, 130, 246);    // Blue 500
                ColAccentHover = Color.FromArgb(37, 99, 235); // Blue 600
                ColSuccess = Color.FromArgb(16, 185, 129);  // Emerald 500
                ColError = Color.FromArgb(239, 68, 68);      // Red 500
            }
            else
            {
                ColBg = Color.FromArgb(248, 250, 252);       // Slate 50
                ColCard = Color.FromArgb(255, 255, 255);     // White
                ColCardHover = Color.FromArgb(241, 245, 249);
                ColBorder = Color.FromArgb(226, 232, 240);   // Slate 200
                ColBorderSelected = Color.FromArgb(37, 99, 235); // Blue 600
                ColTextPrimary = Color.FromArgb(15, 23, 42);  // Slate 900
                ColTextSecondary = Color.FromArgb(100, 116, 139); // Slate 500
                ColAccent = Color.FromArgb(37, 99, 235);     // Blue 600
                ColAccentHover = Color.FromArgb(29, 78, 216); // Blue 700
                ColSuccess = Color.FromArgb(5, 150, 105);    // Emerald 600
                ColError = Color.FromArgb(220, 38, 38);      // Red 600
            }

            string fontName = "Segoe UI Variable Text";
            using (Font test = new Font(fontName, 9f))
            {
                if (test.Name != fontName) fontName = "Segoe UI";
            }

            FontTitle = new Font(fontName, 13f, FontStyle.Bold);
            FontHeading = new Font(fontName, 10.5f, FontStyle.Bold);
            FontBody = new Font(fontName, 9.5f, FontStyle.Regular);
            FontSubtext = new Font(fontName, 8.5f, FontStyle.Regular);
            FontBadge = new Font(fontName, 8f, FontStyle.Bold);
            FontLargeStat = new Font(fontName, 18f, FontStyle.Bold);
        }

        public static void ApplyToForm(Form form)
        {
            form.BackColor = ColBg;
            form.ForeColor = ColTextPrimary;
            form.Font = FontBody;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ShowIcon = false;
            form.KeyPreview = true;
            try
            {
                typeof(Form).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, form, new object[] { true });
            }
            catch { }

            int darkModeVal = IsDarkMode ? 1 : 0;
            DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkModeVal, sizeof(int));
            int roundVal = DWMWCP_ROUND;
            DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref roundVal, sizeof(int));

            form.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) form.Close();
            };
        }
    }

    // -------------------------------------------------------------
    // CONVERT FORM
    // -------------------------------------------------------------
    public class ConvertForm : Form
    {
        private List<string> inputFiles;
        private ImageInfo primaryInfo;
        private ImageFormatType selectedTarget = ImageFormatType.Png;

        private enum State { Selection, Processing, Completed, Error }
        private State currentState = State.Selection;

        private List<string> createdFiles = new List<string>();
        private string errorMessage = "";

        private Label lblBadge;
        private Label lblTitle;
        private Panel contentPanel;
        private Button btnPrimary;
        private Button btnSecondary;
        private Button btnTertiary;

        private readonly ImageFormatType[] supportedFormats = new ImageFormatType[]
        {
            ImageFormatType.Jpeg,
            ImageFormatType.Png,
            ImageFormatType.WebP,
            ImageFormatType.Bmp,
            ImageFormatType.Tiff,
            ImageFormatType.Ico
        };

        public ConvertForm(List<string> files)
        {
            this.inputFiles = files;
            this.Text = "Veqlo Quick — Convert";
            this.ClientSize = new Size(500, 390);
            UiTheme.ApplyToForm(this);

            InspectFiles();
            BuildBaseLayout();
            RenderCurrentState();
        }

        private void InspectFiles()
        {
            if (inputFiles == null || inputFiles.Count == 0)
            {
                currentState = State.Error;
                errorMessage = "No image files provided.";
                return;
            }

            try
            {
                primaryInfo = ImageEngine.Inspect(inputFiles[0]);
                // Default target format: if source is PNG, default to WebP or JPEG; otherwise default to PNG
                if (primaryInfo.Format == ImageFormatType.Png)
                    selectedTarget = ImageFormatType.WebP;
                else
                    selectedTarget = ImageFormatType.Png;
            }
            catch (Exception ex)
            {
                currentState = State.Error;
                errorMessage = ex.Message;
            }
        }

        private void BuildBaseLayout()
        {
            lblBadge = new Label
            {
                Text = "VEQLO QUICK",
                Font = UiTheme.FontBadge,
                ForeColor = UiTheme.ColAccent,
                Location = new Point(24, 16),
                AutoSize = true
            };
            this.Controls.Add(lblBadge);

            lblTitle = new Label
            {
                Text = inputFiles.Count > 1 ? string.Format("Convert {0} Images", inputFiles.Count) : "Convert Image",
                Font = UiTheme.FontTitle,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(24, 34),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            contentPanel = new Panel
            {
                Location = new Point(24, 68),
                Size = new Size(452, 260),
                BackColor = Color.Transparent
            };
            this.Controls.Add(contentPanel);

            // Bottom Buttons
            btnTertiary = new Button
            {
                Text = "View File",
                Font = UiTheme.FontBody,
                Size = new Size(100, 36),
                Location = new Point(24, 338),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnTertiary.FlatAppearance.BorderColor = UiTheme.ColBorder;
            btnTertiary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    try { Process.Start(new ProcessStartInfo(createdFiles[0]) { UseShellExecute = true }); } catch { }
                }
            };
            this.Controls.Add(btnTertiary);

            btnSecondary = new Button
            {
                Text = "Cancel",
                Font = UiTheme.FontBody,
                Size = new Size(100, 36),
                Location = new Point(268, 338),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSecondary.FlatAppearance.BorderColor = UiTheme.ColBorder;
            btnSecondary.Click += (s, e) => this.Close();
            this.Controls.Add(btnSecondary);

            btnPrimary = new Button
            {
                Text = "Convert",
                Font = UiTheme.FontHeading,
                Size = new Size(100, 36),
                Location = new Point(376, 338),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = UiTheme.ColAccent,
                ForeColor = Color.White
            };
            btnPrimary.FlatAppearance.BorderSize = 0;
            btnPrimary.Click += (s, e) => OnPrimaryClick();
            this.Controls.Add(btnPrimary);

            this.AcceptButton = btnPrimary;
            this.CancelButton = btnSecondary;
        }

        private void RenderCurrentState()
        {
            contentPanel.Controls.Clear();

            switch (currentState)
            {
                case State.Selection:
                    RenderSelection();
                    break;
                case State.Processing:
                    RenderProcessing();
                    break;
                case State.Completed:
                    RenderCompleted();
                    break;
                case State.Error:
                    RenderError();
                    break;
            }
        }

        private void RenderSelection()
        {
            // File summary card
            Panel card = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(452, 58),
                BackColor = UiTheme.ColCard
            };
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(UiTheme.ColBorder, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblName = new Label
            {
                Text = inputFiles.Count > 1 ? string.Format("{0} images selected ({1})", inputFiles.Count, primaryInfo.FileName) : primaryInfo.FileName,
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(14, 10),
                Size = new Size(420, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblName);

            string dimStr = (primaryInfo.Width > 0 && primaryInfo.Height > 0) ? string.Format("{0} × {1} px  •  ", primaryInfo.Width, primaryInfo.Height) : "";
            Label lblMeta = new Label
            {
                Text = string.Format("Current format: {0}  •  {1}{2}", primaryInfo.FormatName, dimStr, ImageEngine.FormatBytes(primaryInfo.FileSizeBytes)),
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(14, 30),
                Size = new Size(420, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblMeta);
            contentPanel.Controls.Add(card);

            // Question Prompt
            Label lblPrompt = new Label
            {
                Text = "What format would you like?",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 72),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblPrompt);

            // Grid of 6 format options (3 columns x 2 rows)
            int startY = 100;
            int cardW = 144;
            int cardH = 68;
            int gap = 10;

            for (int i = 0; i < supportedFormats.Length; i++)
            {
                ImageFormatType fmt = supportedFormats[i];
                int col = i % 3;
                int row = i / 3;

                Panel fmtPanel = new Panel
                {
                    Location = new Point(col * (cardW + gap), startY + (row * (cardH + gap))),
                    Size = new Size(cardW, cardH),
                    BackColor = (selectedTarget == fmt) ? UiTheme.ColCardHover : UiTheme.ColCard,
                    Cursor = Cursors.Hand,
                    Tag = fmt
                };

                bool isSelected = (selectedTarget == fmt);
                fmtPanel.Paint += (s, e) =>
                {
                    Panel pnl = (Panel)s;
                    ImageFormatType pFmt = (ImageFormatType)pnl.Tag;
                    bool sel = (selectedTarget == pFmt);
                    using (Pen pen = new Pen(sel ? UiTheme.ColAccent : UiTheme.ColBorder, sel ? 2 : 1))
                    {
                        e.Graphics.DrawRectangle(pen, sel ? 1 : 0, sel ? 1 : 0, pnl.Width - (sel ? 2 : 1), pnl.Height - (sel ? 2 : 1));
                    }
                };

                Label lblFmtName = new Label
                {
                    Text = ImageEngine.GetFormatDisplayName(fmt),
                    Font = UiTheme.FontHeading,
                    ForeColor = isSelected ? UiTheme.ColAccent : UiTheme.ColTextPrimary,
                    Location = new Point(12, 12),
                    AutoSize = true,
                    Tag = fmt
                };

                string desc = "";
                switch (fmt)
                {
                    case ImageFormatType.Jpeg: desc = "Standard / Photos"; break;
                    case ImageFormatType.Png: desc = "Lossless + Alpha"; break;
                    case ImageFormatType.WebP: desc = "Modern Compact"; break;
                    case ImageFormatType.Bmp: desc = "Raw Bitmap"; break;
                    case ImageFormatType.Tiff: desc = "High Dynamic"; break;
                    case ImageFormatType.Ico: desc = "Windows Icon"; break;
                }

                Label lblFmtDesc = new Label
                {
                    Text = desc,
                    Font = UiTheme.FontSubtext,
                    ForeColor = UiTheme.ColTextSecondary,
                    Location = new Point(12, 34),
                    Size = new Size(120, 24),
                    Tag = fmt
                };

                Action selectAction = () =>
                {
                    selectedTarget = fmt;
                    RenderSelection();
                };

                fmtPanel.Click += (s, e) => selectAction();
                lblFmtName.Click += (s, e) => selectAction();
                lblFmtDesc.Click += (s, e) => selectAction();

                fmtPanel.Controls.Add(lblFmtName);
                fmtPanel.Controls.Add(lblFmtDesc);
                contentPanel.Controls.Add(fmtPanel);
            }

            btnPrimary.Text = "Convert";
            btnPrimary.Visible = true;
            btnPrimary.Enabled = true;

            btnSecondary.Text = "Cancel";
            btnSecondary.Visible = true;
            btnSecondary.Enabled = true;

            btnTertiary.Visible = false;
        }

        private void RenderProcessing()
        {
            Label lblProc = new Label
            {
                Text = string.Format("Converting to {0}...", ImageEngine.GetFormatDisplayName(selectedTarget)),
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 70),
                Size = new Size(452, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblProc);

            ProgressBar pb = new ProgressBar
            {
                Location = new Point(46, 110),
                Size = new Size(360, 8),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 25
            };
            contentPanel.Controls.Add(pb);

            Label lblHint = new Label
            {
                Text = "Genuine multi-format encoding with full color and transparency preservation.",
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(0, 134),
                Size = new Size(452, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblHint);

            btnPrimary.Enabled = false;
            btnSecondary.Enabled = false;
            btnTertiary.Visible = false;
        }

        private void RenderCompleted()
        {
            Label lblCheck = new Label
            {
                Text = "✓",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = UiTheme.ColSuccess,
                Location = new Point(0, 20),
                Size = new Size(452, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblCheck);

            Label lblSuccess = new Label
            {
                Text = createdFiles.Count > 1 ? string.Format("Successfully converted {0} images!", createdFiles.Count) : "Conversion complete!",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 68),
                Size = new Size(452, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblSuccess);

            Label lblOut = new Label
            {
                Text = createdFiles.Count > 0 ? Path.GetFileName(createdFiles[0]) : "",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColAccent,
                Location = new Point(0, 96),
                Size = new Size(452, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            contentPanel.Controls.Add(lblOut);

            Label lblPath = new Label
            {
                Text = "Saved safely in the same directory without modifying original.",
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(0, 122),
                Size = new Size(452, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblPath);

            btnTertiary.Text = "View File";
            btnTertiary.Visible = (createdFiles.Count > 0);
            btnTertiary.Enabled = true;

            btnSecondary.Text = "Open Folder";
            btnSecondary.Visible = true;
            btnSecondary.Enabled = true;
            btnSecondary.Click -= (s, e) => this.Close();
            btnSecondary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    try { Process.Start("explorer.exe", string.Format("/select,\"{0}\"", createdFiles[0])); } catch { }
                }
            };

            btnPrimary.Text = "Done";
            btnPrimary.Enabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();
        }

        private void RenderError()
        {
            Label lblErrIcon = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = UiTheme.ColError,
                Location = new Point(0, 20),
                Size = new Size(452, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrIcon);

            Label lblErrTitle = new Label
            {
                Text = "Conversion could not be completed",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 68),
                Size = new Size(452, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrTitle);

            Label lblErrDesc = new Label
            {
                Text = errorMessage,
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(20, 96),
                Size = new Size(412, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrDesc);

            btnPrimary.Text = "Close";
            btnPrimary.Enabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Visible = false;
            btnTertiary.Visible = false;
        }

        private void OnPrimaryClick()
        {
            if (currentState == State.Selection)
            {
                currentState = State.Processing;
                RenderCurrentState();

                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        createdFiles.Clear();
                        foreach (string file in inputFiles)
                        {
                            string outPath = ImageEngine.Convert(file, selectedTarget);
                            createdFiles.Add(outPath);
                        }

                        Thread.Sleep(300); // brief UI smoothness
                        this.Invoke((MethodInvoker)delegate
                        {
                            currentState = State.Completed;
                            RenderCurrentState();
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            currentState = State.Error;
                            errorMessage = ex.Message;
                            RenderCurrentState();
                        });
                    }
                });
            }
        }
    }

    // -------------------------------------------------------------
    // COMPRESS FORM
    // -------------------------------------------------------------
    public class CompressForm : Form
    {
        private List<string> inputFiles;
        private ImageInfo primaryInfo;
        private int selectedQuality = 80;

        private enum State { Options, Processing, Completed, Error }
        private State currentState = State.Options;

        private List<string> createdFiles = new List<string>();
        private long originalTotalBytes = 0;
        private long compressedTotalBytes = 0;
        private string errorMessage = "";

        private Label lblBadge;
        private Label lblTitle;
        private Panel contentPanel;
        private Button btnPrimary;
        private Button btnSecondary;
        private Button btnTertiary;

        public CompressForm(List<string> files)
        {
            this.inputFiles = files;
            this.Text = "Veqlo Quick — Compress";
            this.ClientSize = new Size(480, 360);
            UiTheme.ApplyToForm(this);

            InspectFiles();
            BuildBaseLayout();
            RenderCurrentState();
        }

        private void InspectFiles()
        {
            if (inputFiles == null || inputFiles.Count == 0)
            {
                currentState = State.Error;
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
                currentState = State.Error;
                errorMessage = ex.Message;
            }
        }

        private void BuildBaseLayout()
        {
            lblBadge = new Label
            {
                Text = "VEQLO QUICK",
                Font = UiTheme.FontBadge,
                ForeColor = UiTheme.ColAccent,
                Location = new Point(24, 16),
                AutoSize = true
            };
            this.Controls.Add(lblBadge);

            lblTitle = new Label
            {
                Text = "Compress Image",
                Font = UiTheme.FontTitle,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(24, 34),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            contentPanel = new Panel
            {
                Location = new Point(24, 68),
                Size = new Size(432, 230),
                BackColor = Color.Transparent
            };
            this.Controls.Add(contentPanel);

            // Bottom Buttons
            btnTertiary = new Button
            {
                Text = "View File",
                Font = UiTheme.FontBody,
                Size = new Size(100, 36),
                Location = new Point(24, 310),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnTertiary.FlatAppearance.BorderColor = UiTheme.ColBorder;
            btnTertiary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    try { Process.Start(new ProcessStartInfo(createdFiles[0]) { UseShellExecute = true }); } catch { }
                }
            };
            this.Controls.Add(btnTertiary);

            btnSecondary = new Button
            {
                Text = "Cancel",
                Font = UiTheme.FontBody,
                Size = new Size(100, 36),
                Location = new Point(248, 310),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSecondary.FlatAppearance.BorderColor = UiTheme.ColBorder;
            btnSecondary.Click += (s, e) => this.Close();
            this.Controls.Add(btnSecondary);

            btnPrimary = new Button
            {
                Text = "Compress",
                Font = UiTheme.FontHeading,
                Size = new Size(100, 36),
                Location = new Point(356, 310),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = UiTheme.ColAccent,
                ForeColor = Color.White
            };
            btnPrimary.FlatAppearance.BorderSize = 0;
            btnPrimary.Click += (s, e) => OnPrimaryClick();
            this.Controls.Add(btnPrimary);

            this.AcceptButton = btnPrimary;
            this.CancelButton = btnSecondary;
        }

        private void RenderCurrentState()
        {
            contentPanel.Controls.Clear();

            switch (currentState)
            {
                case State.Options:
                    RenderOptions();
                    break;
                case State.Processing:
                    RenderProcessing();
                    break;
                case State.Completed:
                    RenderCompleted();
                    break;
                case State.Error:
                    RenderError();
                    break;
            }
        }

        private void RenderOptions()
        {
            // File summary card
            Panel card = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(432, 58),
                BackColor = UiTheme.ColCard
            };
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(UiTheme.ColBorder, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblName = new Label
            {
                Text = inputFiles.Count > 1 ? string.Format("{0} images selected", inputFiles.Count) : primaryInfo.FileName,
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(14, 10),
                Size = new Size(400, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblName);

            Label lblMeta = new Label
            {
                Text = string.Format("Original size: {0}  •  Format: {1}", ImageEngine.FormatBytes(originalTotalBytes), primaryInfo.FormatName),
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(14, 30),
                Size = new Size(400, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblMeta);
            contentPanel.Controls.Add(card);

            Label lblPrompt = new Label
            {
                Text = "Select Compression Level:",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 72),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblPrompt);

            // 3 Presets: Balanced (80), Maximum (55), Light (90)
            int[] qualities = new int[] { 80, 55, 90 };
            string[] titles = new string[] { "Balanced (Recommended)", "Maximum Reduction", "Light / High Fidelity" };
            string[] descs = new string[] { "Optimal balance of small file size and high clarity", "Aggressive compression for fast sharing and web", "Subtle optimization preserving maximum quality" };

            int startY = 98;
            int cardH = 42;
            int gap = 8;

            for (int i = 0; i < qualities.Length; i++)
            {
                int q = qualities[i];
                Panel optCard = new Panel
                {
                    Location = new Point(0, startY + (i * (cardH + gap))),
                    Size = new Size(432, cardH),
                    BackColor = (selectedQuality == q) ? UiTheme.ColCardHover : UiTheme.ColCard,
                    Cursor = Cursors.Hand,
                    Tag = q
                };

                optCard.Paint += (s, e) =>
                {
                    Panel pnl = (Panel)s;
                    int curQ = (int)pnl.Tag;
                    bool sel = (selectedQuality == curQ);
                    using (Pen p = new Pen(sel ? UiTheme.ColAccent : UiTheme.ColBorder, sel ? 2 : 1))
                    {
                        e.Graphics.DrawRectangle(p, sel ? 1 : 0, sel ? 1 : 0, pnl.Width - (sel ? 2 : 1), pnl.Height - (sel ? 2 : 1));
                    }
                };

                Label lblOptTitle = new Label
                {
                    Text = titles[i],
                    Font = UiTheme.FontHeading,
                    ForeColor = (selectedQuality == q) ? UiTheme.ColAccent : UiTheme.ColTextPrimary,
                    Location = new Point(14, 4),
                    AutoSize = true,
                    Tag = q
                };

                Label lblOptDesc = new Label
                {
                    Text = descs[i],
                    Font = UiTheme.FontSubtext,
                    ForeColor = UiTheme.ColTextSecondary,
                    Location = new Point(14, 22),
                    AutoSize = true,
                    Tag = q
                };

                Action act = () =>
                {
                    selectedQuality = q;
                    RenderOptions();
                };

                optCard.Click += (s, e) => act();
                lblOptTitle.Click += (s, e) => act();
                lblOptDesc.Click += (s, e) => act();

                optCard.Controls.Add(lblOptTitle);
                optCard.Controls.Add(lblOptDesc);
                contentPanel.Controls.Add(optCard);
            }

            btnPrimary.Text = "Compress";
            btnPrimary.Enabled = true;
            btnSecondary.Text = "Cancel";
            btnSecondary.Visible = true;
            btnTertiary.Visible = false;
        }

        private void RenderProcessing()
        {
            Label lblProc = new Label
            {
                Text = "Optimizing and re-encoding image data...",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 60),
                Size = new Size(432, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblProc);

            ProgressBar pb = new ProgressBar
            {
                Location = new Point(40, 100),
                Size = new Size(352, 8),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 25
            };
            contentPanel.Controls.Add(pb);

            btnPrimary.Enabled = false;
            btnSecondary.Enabled = false;
            btnTertiary.Visible = false;
        }

        private void RenderCompleted()
        {
            Label lblCheck = new Label
            {
                Text = "✓",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = UiTheme.ColSuccess,
                Location = new Point(0, 10),
                Size = new Size(432, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblCheck);

            Label lblDone = new Label
            {
                Text = "Compression complete!",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 52),
                Size = new Size(432, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblDone);

            double reductionPct = ((double)(originalTotalBytes - compressedTotalBytes) / originalTotalBytes) * 100.0;
            Label lblStats = new Label
            {
                Text = string.Format("Saved {0:0.#}%  ({1} → {2})", reductionPct, ImageEngine.FormatBytes(originalTotalBytes), ImageEngine.FormatBytes(compressedTotalBytes)),
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColSuccess,
                Location = new Point(0, 80),
                Size = new Size(432, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblStats);

            Label lblSavedName = new Label
            {
                Text = createdFiles.Count > 0 ? Path.GetFileName(createdFiles[0]) : "",
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(0, 108),
                Size = new Size(432, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            contentPanel.Controls.Add(lblSavedName);

            btnTertiary.Text = "View File";
            btnTertiary.Visible = true;
            btnTertiary.Enabled = true;

            btnSecondary.Text = "Open Folder";
            btnSecondary.Visible = true;
            btnSecondary.Enabled = true;
            btnSecondary.Click -= (s, e) => this.Close();
            btnSecondary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    try { Process.Start("explorer.exe", string.Format("/select,\"{0}\"", createdFiles[0])); } catch { }
                }
            };

            btnPrimary.Text = "Done";
            btnPrimary.Enabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();
        }

        private void RenderError()
        {
            Label lblErrIcon = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = UiTheme.ColError,
                Location = new Point(0, 16),
                Size = new Size(432, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrIcon);

            Label lblErrTitle = new Label
            {
                Text = "Compression Notice",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 58),
                Size = new Size(432, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrTitle);

            Label lblErrDesc = new Label
            {
                Text = errorMessage,
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(20, 86),
                Size = new Size(392, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrDesc);

            btnPrimary.Text = "Close";
            btnPrimary.Enabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Visible = false;
            btnTertiary.Visible = false;
        }

        private void OnPrimaryClick()
        {
            if (currentState == State.Options)
            {
                currentState = State.Processing;
                RenderCurrentState();

                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        createdFiles.Clear();
                        compressedTotalBytes = 0;

                        foreach (string file in inputFiles)
                        {
                            long orig, comp;
                            string res = ImageEngine.Compress(file, selectedQuality, out orig, out comp);
                            createdFiles.Add(res);
                            compressedTotalBytes += comp;
                        }

                        Thread.Sleep(300);
                        this.Invoke((MethodInvoker)delegate
                        {
                            currentState = State.Completed;
                            RenderCurrentState();
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            currentState = State.Error;
                            errorMessage = ex.Message;
                            RenderCurrentState();
                        });
                    }
                });
            }
        }
    }

    // -------------------------------------------------------------
    // RESIZE FORM
    // -------------------------------------------------------------
    public class ResizeForm : Form
    {
        private List<string> inputFiles;
        private ImageInfo primaryInfo;

        private enum State { Options, Processing, Completed, Error }
        private State currentState = State.Options;

        private List<string> createdFiles = new List<string>();
        private int finalWidth = 0;
        private int finalHeight = 0;
        private string errorMessage = "";

        private Label lblBadge;
        private Label lblTitle;
        private Panel contentPanel;
        private Button btnPrimary;
        private Button btnSecondary;
        private Button btnTertiary;

        private TextBox txtWidth;
        private TextBox txtHeight;
        private CheckBox chkAspect;
        private bool isUpdatingDimensions = false;

        public ResizeForm(List<string> files)
        {
            this.inputFiles = files;
            this.Text = "Veqlo Quick — Resize";
            this.ClientSize = new Size(480, 360);
            UiTheme.ApplyToForm(this);

            InspectFiles();
            BuildBaseLayout();
            RenderCurrentState();
        }

        private void InspectFiles()
        {
            if (inputFiles == null || inputFiles.Count == 0)
            {
                currentState = State.Error;
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
                currentState = State.Error;
                errorMessage = ex.Message;
            }
        }

        private void BuildBaseLayout()
        {
            lblBadge = new Label
            {
                Text = "VEQLO QUICK",
                Font = UiTheme.FontBadge,
                ForeColor = UiTheme.ColAccent,
                Location = new Point(24, 16),
                AutoSize = true
            };
            this.Controls.Add(lblBadge);

            lblTitle = new Label
            {
                Text = "Resize Image",
                Font = UiTheme.FontTitle,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(24, 34),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            contentPanel = new Panel
            {
                Location = new Point(24, 68),
                Size = new Size(432, 230),
                BackColor = Color.Transparent
            };
            this.Controls.Add(contentPanel);

            // Bottom Buttons
            btnTertiary = new Button
            {
                Text = "View File",
                Font = UiTheme.FontBody,
                Size = new Size(100, 36),
                Location = new Point(24, 310),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnTertiary.FlatAppearance.BorderColor = UiTheme.ColBorder;
            btnTertiary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    try { Process.Start(new ProcessStartInfo(createdFiles[0]) { UseShellExecute = true }); } catch { }
                }
            };
            this.Controls.Add(btnTertiary);

            btnSecondary = new Button
            {
                Text = "Cancel",
                Font = UiTheme.FontBody,
                Size = new Size(100, 36),
                Location = new Point(248, 310),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSecondary.FlatAppearance.BorderColor = UiTheme.ColBorder;
            btnSecondary.Click += (s, e) => this.Close();
            this.Controls.Add(btnSecondary);

            btnPrimary = new Button
            {
                Text = "Resize",
                Font = UiTheme.FontHeading,
                Size = new Size(100, 36),
                Location = new Point(356, 310),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = UiTheme.ColAccent,
                ForeColor = Color.White
            };
            btnPrimary.FlatAppearance.BorderSize = 0;
            btnPrimary.Click += (s, e) => OnPrimaryClick();
            this.Controls.Add(btnPrimary);

            this.AcceptButton = btnPrimary;
            this.CancelButton = btnSecondary;
        }

        private void RenderCurrentState()
        {
            contentPanel.Controls.Clear();

            switch (currentState)
            {
                case State.Options:
                    RenderOptions();
                    break;
                case State.Processing:
                    RenderProcessing();
                    break;
                case State.Completed:
                    RenderCompleted();
                    break;
                case State.Error:
                    RenderError();
                    break;
            }
        }

        private void RenderOptions()
        {
            // File summary card
            Panel card = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(432, 58),
                BackColor = UiTheme.ColCard
            };
            card.Paint += (s, e) =>
            {
                using (Pen p = new Pen(UiTheme.ColBorder, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            Label lblName = new Label
            {
                Text = primaryInfo.FileName,
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(14, 10),
                Size = new Size(400, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblName);

            Label lblMeta = new Label
            {
                Text = string.Format("Current: {0} × {1} px  •  {2}  •  {3}", primaryInfo.Width, primaryInfo.Height, primaryInfo.FormatName, ImageEngine.FormatBytes(primaryInfo.FileSizeBytes)),
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(14, 30),
                Size = new Size(400, 18),
                AutoEllipsis = true
            };
            card.Controls.Add(lblMeta);
            contentPanel.Controls.Add(card);

            // Quick Scale presets
            Label lblPresets = new Label
            {
                Text = "Quick Presets:",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 72),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblPresets);

            string[] presetLabels = new string[] { "25%", "50%", "75%", "Original" };
            double[] presetScales = new double[] { 0.25, 0.50, 0.75, 1.00 };

            int btnW = 98;
            int gap = 12;
            for (int i = 0; i < presetLabels.Length; i++)
            {
                double scale = presetScales[i];
                Button btnPreset = new Button
                {
                    Text = presetLabels[i],
                    Font = UiTheme.FontBody,
                    Size = new Size(btnW, 30),
                    Location = new Point(i * (btnW + gap), 96),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    BackColor = UiTheme.ColCard,
                    ForeColor = UiTheme.ColTextPrimary
                };
                btnPreset.FlatAppearance.BorderColor = UiTheme.ColBorder;
                btnPreset.Click += (s, e) =>
                {
                    isUpdatingDimensions = true;
                    int newW = Math.Max(1, (int)Math.Round(primaryInfo.Width * scale));
                    int newH = Math.Max(1, (int)Math.Round(primaryInfo.Height * scale));
                    txtWidth.Text = newW.ToString();
                    txtHeight.Text = newH.ToString();
                    isUpdatingDimensions = false;
                };
                contentPanel.Controls.Add(btnPreset);
            }

            // Width / Height Inputs
            Label lblW = new Label
            {
                Text = "Width (px)",
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(0, 142),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblW);

            txtWidth = new TextBox
            {
                Text = primaryInfo.Width.ToString(),
                Font = UiTheme.FontHeading,
                Location = new Point(0, 162),
                Size = new Size(130, 26),
                BackColor = UiTheme.ColCard,
                ForeColor = UiTheme.ColTextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            contentPanel.Controls.Add(txtWidth);

            Label lblTimes = new Label
            {
                Text = "×",
                Font = UiTheme.FontTitle,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(140, 162),
                Size = new Size(20, 26),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblTimes);

            Label lblH = new Label
            {
                Text = "Height (px)",
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(170, 142),
                AutoSize = true
            };
            contentPanel.Controls.Add(lblH);

            txtHeight = new TextBox
            {
                Text = primaryInfo.Height.ToString(),
                Font = UiTheme.FontHeading,
                Location = new Point(170, 162),
                Size = new Size(130, 26),
                BackColor = UiTheme.ColCard,
                ForeColor = UiTheme.ColTextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            contentPanel.Controls.Add(txtHeight);

            chkAspect = new CheckBox
            {
                Text = "Lock aspect ratio",
                Checked = true,
                Font = UiTheme.FontBody,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(315, 164),
                Size = new Size(120, 24)
            };
            contentPanel.Controls.Add(chkAspect);

            txtWidth.TextChanged += (s, e) =>
            {
                if (isUpdatingDimensions || !chkAspect.Checked) return;
                int w;
                if (int.TryParse(txtWidth.Text, out w) && w > 0 && primaryInfo.Width > 0)
                {
                    isUpdatingDimensions = true;
                    double ratio = (double)primaryInfo.Height / primaryInfo.Width;
                    int h = Math.Max(1, (int)Math.Round(w * ratio));
                    txtHeight.Text = h.ToString();
                    isUpdatingDimensions = false;
                }
            };

            txtHeight.TextChanged += (s, e) =>
            {
                if (isUpdatingDimensions || !chkAspect.Checked) return;
                int h;
                if (int.TryParse(txtHeight.Text, out h) && h > 0 && primaryInfo.Height > 0)
                {
                    isUpdatingDimensions = true;
                    double ratio = (double)primaryInfo.Width / primaryInfo.Height;
                    int w = Math.Max(1, (int)Math.Round(h * ratio));
                    txtWidth.Text = w.ToString();
                    isUpdatingDimensions = false;
                }
            };

            btnPrimary.Text = "Resize";
            btnPrimary.Enabled = true;
            btnSecondary.Text = "Cancel";
            btnSecondary.Visible = true;
            btnTertiary.Visible = false;
        }

        private void RenderProcessing()
        {
            Label lblProc = new Label
            {
                Text = "Resizing with high-quality bicubic interpolation...",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 60),
                Size = new Size(432, 24),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblProc);

            ProgressBar pb = new ProgressBar
            {
                Location = new Point(40, 100),
                Size = new Size(352, 8),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 25
            };
            contentPanel.Controls.Add(pb);

            btnPrimary.Enabled = false;
            btnSecondary.Enabled = false;
            btnTertiary.Visible = false;
        }

        private void RenderCompleted()
        {
            Label lblCheck = new Label
            {
                Text = "✓",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = UiTheme.ColSuccess,
                Location = new Point(0, 10),
                Size = new Size(432, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblCheck);

            Label lblDone = new Label
            {
                Text = "Resize complete!",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 52),
                Size = new Size(432, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblDone);

            Label lblStats = new Label
            {
                Text = string.Format("New Dimensions: {0} × {1} px", finalWidth, finalHeight),
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColAccent,
                Location = new Point(0, 80),
                Size = new Size(432, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblStats);

            Label lblSavedName = new Label
            {
                Text = createdFiles.Count > 0 ? Path.GetFileName(createdFiles[0]) : "",
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(0, 108),
                Size = new Size(432, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };
            contentPanel.Controls.Add(lblSavedName);

            btnTertiary.Text = "View File";
            btnTertiary.Visible = true;
            btnTertiary.Enabled = true;

            btnSecondary.Text = "Open Folder";
            btnSecondary.Visible = true;
            btnSecondary.Enabled = true;
            btnSecondary.Click -= (s, e) => this.Close();
            btnSecondary.Click += (s, e) =>
            {
                if (createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    try { Process.Start("explorer.exe", string.Format("/select,\"{0}\"", createdFiles[0])); } catch { }
                }
            };

            btnPrimary.Text = "Done";
            btnPrimary.Enabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();
        }

        private void RenderError()
        {
            Label lblErrIcon = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                ForeColor = UiTheme.ColError,
                Location = new Point(0, 16),
                Size = new Size(432, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrIcon);

            Label lblErrTitle = new Label
            {
                Text = "Resize Failed",
                Font = UiTheme.FontHeading,
                ForeColor = UiTheme.ColTextPrimary,
                Location = new Point(0, 58),
                Size = new Size(432, 22),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrTitle);

            Label lblErrDesc = new Label
            {
                Text = errorMessage,
                Font = UiTheme.FontSubtext,
                ForeColor = UiTheme.ColTextSecondary,
                Location = new Point(20, 86),
                Size = new Size(392, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            contentPanel.Controls.Add(lblErrDesc);

            btnPrimary.Text = "Close";
            btnPrimary.Enabled = true;
            btnPrimary.Click -= (s, e) => OnPrimaryClick();
            btnPrimary.Click += (s, e) => this.Close();

            btnSecondary.Visible = false;
            btnTertiary.Visible = false;
        }

        private void OnPrimaryClick()
        {
            if (currentState == State.Options)
            {
                int targetW, targetH;
                if (!int.TryParse(txtWidth.Text, out targetW) || targetW <= 0 ||
                    !int.TryParse(txtHeight.Text, out targetH) || targetH <= 0)
                {
                    MessageBox.Show("Please enter valid positive dimensions.", "Invalid Dimensions", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                currentState = State.Processing;
                RenderCurrentState();

                ThreadPool.QueueUserWorkItem((state) =>
                {
                    try
                    {
                        createdFiles.Clear();
                        foreach (string file in inputFiles)
                        {
                            int rW, rH;
                            string res = ImageEngine.Resize(file, targetW, targetH, out rW, out rH);
                            createdFiles.Add(res);
                            finalWidth = rW;
                            finalHeight = rH;
                        }

                        Thread.Sleep(300);
                        this.Invoke((MethodInvoker)delegate
                        {
                            currentState = State.Completed;
                            RenderCurrentState();
                        });
                    }
                    catch (Exception ex)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            currentState = State.Error;
                            errorMessage = ex.Message;
                            RenderCurrentState();
                        });
                    }
                });
            }
        }
    }

    // -------------------------------------------------------------
    // MAIN ENTRY POINT
    // -------------------------------------------------------------
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string action = "convert";
            List<string> inputFiles = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.Equals("convert", StringComparison.OrdinalIgnoreCase))
                {
                    action = "convert";
                }
                else if (arg.Equals("compress", StringComparison.OrdinalIgnoreCase))
                {
                    action = "compress";
                }
                else if (arg.Equals("resize", StringComparison.OrdinalIgnoreCase))
                {
                    action = "resize";
                }
                else if (arg.Equals("--input", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    inputFiles.Add(args[i + 1]);
                    i++;
                }
                else if (!arg.StartsWith("-") && File.Exists(arg))
                {
                    inputFiles.Add(arg);
                }
            }

            if (inputFiles.Count == 0)
            {
                MessageBox.Show(
                    "Veqlo Quick is a Windows File Explorer context-menu utility.\n\n" +
                    "To use it, right-click any image file (JPG, PNG, WebP, BMP, TIFF, ICO) in File Explorer and select Veqlo Quick.",
                    "Veqlo Quick",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (action == "compress")
            {
                Application.Run(new CompressForm(inputFiles));
            }
            else if (action == "resize")
            {
                Application.Run(new ResizeForm(inputFiles));
            }
            else
            {
                Application.Run(new ConvertForm(inputFiles));
            }
        }
    }
}
