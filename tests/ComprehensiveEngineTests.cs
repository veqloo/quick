using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using VeqloQuick;

namespace VeqloQuickTests
{
    class Program
    {
        static int totalTests = 0;
        static int passedTests = 0;
        static int failedTests = 0;

        static void Assert(bool condition, string testName, string details = "")
        {
            totalTests++;
            if (condition)
            {
                passedTests++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[PASS] " + testName + (string.IsNullOrEmpty(details) ? "" : " (" + details + ")"));
            }
            else
            {
                failedTests++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[FAIL] " + testName + (string.IsNullOrEmpty(details) ? "" : " (" + details + ")"));
            }
            Console.ResetColor();
        }

        static void Main()
        {
            Console.WriteLine("==============================================================");
            Console.WriteLine("   Veqlo Quick v1.1 - Comprehensive Test Suite                ");
            Console.WriteLine("==============================================================");

            string tempDir = Path.Combine(Path.GetTempPath(), "VeqloQuick_Tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                Test_JpgToPng(tempDir);
                Test_PngToJpg(tempDir);
                Test_TransparentPngToJpg(tempDir);
                Test_PngToWebp(tempDir);
                Test_WebpToPng(tempDir);
                Test_PngToIco(tempDir);
                Test_PngToBmp(tempDir);
                Test_BmpToTiff(tempDir);
                Test_Compression(tempDir);
                Test_Resizing(tempDir);
                Test_OriginalFilePreservation(tempDir);
                Test_CollisionAvoidance(tempDir);
                Test_InvalidAndCorruptFiles(tempDir);
                Test_LargeHighResolutionImage(tempDir);
                Test_MetadataInspection(tempDir);
                Test_MetadataStripping(tempDir);
                Test_ResizeFitMode(tempDir);
                Test_ResizeFillMode(tempDir);
                Test_ResizeAutoSwapOrientation(tempDir);
                Test_CompressToTargetSize(tempDir);
            }
            finally
            {
                try { Directory.Delete(tempDir, true); } catch { }
            }

            Console.WriteLine("==============================================================");
            Console.WriteLine(string.Format("Total Tests: {0} | Passed: {1} | Failed: {2}", totalTests, passedTests, failedTests));
            Console.WriteLine("==============================================================");

            if (failedTests > 0)
            {
                Environment.Exit(1);
            }
        }

        static string ComputeFileSha256(string filePath)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream fs = File.OpenRead(filePath))
            {
                byte[] hash = sha.ComputeHash(fs);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        static void Test_JpgToPng(string dir)
        {
            string jpg = Path.Combine(dir, "photo.jpg");
            using (Bitmap bmp = new Bitmap(200, 150))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.CornflowerBlue); }
                bmp.Save(jpg, ImageFormat.Jpeg);
            }

            string outPng = ImageEngine.Convert(jpg, ImageFormatType.Png);
            Assert(File.Exists(outPng), "JPG -> PNG exists");
            Assert(ImageEngine.DetectFormat(outPng) == ImageFormatType.Png, "JPG -> PNG format signature valid");

            byte[] header = new byte[8];
            using (FileStream fs = File.OpenRead(outPng)) { fs.Read(header, 0, 8); }
            Assert(header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47, "JPG -> PNG has 8-byte PNG magic header");

            using (Image check = Image.FromFile(outPng))
            {
                Assert(check.Width == 200 && check.Height == 150, "JPG -> PNG decoded dimensions match 200x150");
            }
        }

        static void Test_PngToJpg(string dir)
        {
            string png = Path.Combine(dir, "graphic.png");
            using (Bitmap bmp = new Bitmap(180, 120))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.MediumPurple); }
                bmp.Save(png, ImageFormat.Png);
            }

            string outJpg = ImageEngine.Convert(png, ImageFormatType.Jpeg);
            Assert(File.Exists(outJpg), "PNG -> JPG exists");
            Assert(ImageEngine.DetectFormat(outJpg) == ImageFormatType.Jpeg, "PNG -> JPG format signature valid");

            byte[] header = new byte[3];
            using (FileStream fs = File.OpenRead(outJpg)) { fs.Read(header, 0, 3); }
            Assert(header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF, "PNG -> JPG has JPEG SOI 0xFFD8FF");

            using (Image check = Image.FromFile(outJpg))
            {
                Assert(check.Width == 180 && check.Height == 120, "PNG -> JPG decoded dimensions match 180x120");
            }
        }

        static void Test_TransparentPngToJpg(string dir)
        {
            string png = Path.Combine(dir, "alpha_logo.png");
            using (Bitmap bmp = new Bitmap(100, 100, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.FillEllipse(Brushes.Crimson, 20, 20, 60, 60);
                }
                bmp.Save(png, ImageFormat.Png);
            }

            string outJpg = ImageEngine.Convert(png, ImageFormatType.Jpeg);
            Assert(File.Exists(outJpg), "Transparent PNG -> JPG exists");

            // Verify corner pixel is white (composited over white, not ugly black)
            using (Bitmap check = new Bitmap(outJpg))
            {
                Color corner = check.GetPixel(5, 5);
                Assert(corner.R > 240 && corner.G > 240 && corner.B > 240, "Transparent area composited with white matte", string.Format("RGB={0},{1},{2}", corner.R, corner.G, corner.B));
            }
        }

        static void Test_PngToWebp(string dir)
        {
            string png = Path.Combine(dir, "source_webp.png");
            using (Bitmap bmp = new Bitmap(240, 160))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.DarkSeaGreen); }
                bmp.Save(png, ImageFormat.Png);
            }

            string outWebp = ImageEngine.Convert(png, ImageFormatType.WebP);
            Assert(File.Exists(outWebp), "PNG -> WebP exists");
            Assert(ImageEngine.DetectFormat(outWebp) == ImageFormatType.WebP, "PNG -> WebP detected as genuine WebP");

            byte[] header = new byte[12];
            using (FileStream fs = File.OpenRead(outWebp)) { fs.Read(header, 0, 12); }
            Assert(header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                   header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50, "PNG -> WebP has RIFF...WEBP header");

            ImageInfo info = ImageEngine.Inspect(outWebp);
            Assert(info.Width == 240 && info.Height == 160, "WebP inspected dimensions match 240x160");
        }

        static void Test_WebpToPng(string dir)
        {
            string webp = Path.Combine(dir, "source.webp");
            using (Bitmap bmp = new Bitmap(160, 160))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Gold); }
                string tempPng = Path.Combine(dir, "temp_for_webp.png");
                bmp.Save(tempPng, ImageFormat.Png);
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ImageEngine.GetCWebpPath(),
                    Arguments = string.Format("-q 80 \"{0}\" -o \"{1}\"", tempPng, webp),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process p = Process.Start(psi)) { p.WaitForExit(); }
                File.Delete(tempPng);
            }

            string outPng = ImageEngine.Convert(webp, ImageFormatType.Png);
            Assert(File.Exists(outPng), "WebP -> PNG exists");
            Assert(ImageEngine.DetectFormat(outPng) == ImageFormatType.Png, "WebP -> PNG detected as genuine PNG");

            using (Image img = Image.FromFile(outPng))
            {
                Assert(img.Width == 160 && img.Height == 160, "WebP -> PNG decoded dimensions match 160x160");
            }
        }

        static void Test_PngToIco(string dir)
        {
            string png = Path.Combine(dir, "app_icon.png");
            using (Bitmap bmp = new Bitmap(256, 256, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.FillEllipse(Brushes.RoyalBlue, 20, 20, 216, 216);
                }
                bmp.Save(png, ImageFormat.Png);
            }

            string outIco = ImageEngine.Convert(png, ImageFormatType.Ico);
            Assert(File.Exists(outIco), "PNG -> ICO exists");
            Assert(ImageEngine.DetectFormat(outIco) == ImageFormatType.Ico, "PNG -> ICO detected as genuine ICO");

            byte[] header = new byte[4];
            using (FileStream fs = File.OpenRead(outIco)) { fs.Read(header, 0, 4); }
            Assert(header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00, "ICO header is 00 00 01 00");

            using (Icon ico = new Icon(outIco))
            {
                Assert(ico.Width > 0 && ico.Height > 0, "Icon loaded natively by Windows Icon class");
            }
        }

        static void Test_PngToBmp(string dir)
        {
            string png = Path.Combine(dir, "raster.png");
            using (Bitmap bmp = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.SlateGray); }
                bmp.Save(png, ImageFormat.Png);
            }

            string outBmp = ImageEngine.Convert(png, ImageFormatType.Bmp);
            Assert(File.Exists(outBmp), "PNG -> BMP exists");
            Assert(ImageEngine.DetectFormat(outBmp) == ImageFormatType.Bmp, "PNG -> BMP detected as BMP");

            byte[] header = new byte[2];
            using (FileStream fs = File.OpenRead(outBmp)) { fs.Read(header, 0, 2); }
            Assert(header[0] == 0x42 && header[1] == 0x4D, "BMP header signature is 'BM'");
        }

        static void Test_BmpToTiff(string dir)
        {
            string bmpPath = Path.Combine(dir, "image.bmp");
            using (Bitmap bmp = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Teal); }
                bmp.Save(bmpPath, ImageFormat.Bmp);
            }

            string outTiff = ImageEngine.Convert(bmpPath, ImageFormatType.Tiff);
            Assert(File.Exists(outTiff), "BMP -> TIFF exists");
            Assert(ImageEngine.DetectFormat(outTiff) == ImageFormatType.Tiff, "BMP -> TIFF detected as TIFF");
        }

        static void Test_Compression(string dir)
        {
            string jpg = Path.Combine(dir, "uncompressed.jpg");
            using (Bitmap bmp = new Bitmap(1200, 900))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    for (int i = 0; i < 200; i++)
                    {
                        g.DrawLine(Pens.Navy, i * 6, 0, i * 6, 900);
                        g.DrawLine(Pens.Orange, 0, i * 4, 1200, i * 4);
                    }
                }
                // Save at 100% quality
                ImageCodecInfo encoder = null;
                foreach (var c in ImageCodecInfo.GetImageEncoders()) if (c.FormatID == ImageFormat.Jpeg.Guid) encoder = c;
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                bmp.Save(jpg, encoder, ep);
            }

            long origSize, compSize;
            string compJpg = ImageEngine.Compress(jpg, 60, out origSize, out compSize);

            Assert(File.Exists(compJpg), "Compressed file exists");
            Assert(compSize < origSize, "Compressed file size is strictly smaller than original", string.Format("{0} -> {1} bytes", origSize, compSize));

            using (Image check = Image.FromFile(compJpg))
            {
                Assert(check.Width == 1200 && check.Height == 900, "Compressed image dimensions preserved");
            }
        }

        static void Test_Resizing(string dir)
        {
            string png = Path.Combine(dir, "to_resize.png");
            using (Bitmap bmp = new Bitmap(1600, 1200))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.DodgerBlue); }
                bmp.Save(png, ImageFormat.Png);
            }

            int rW, rH;
            string resized = ImageEngine.Resize(png, 800, 600, out rW, out rH);

            Assert(File.Exists(resized), "Resized file exists");
            Assert(rW == 800 && rH == 600, "Resize returned correct dimensions (800x600)");

            using (Image check = Image.FromFile(resized))
            {
                Assert(check.Width == 800 && check.Height == 600, "Resized image file has exact dimensions 800x600");
            }
        }

        static void Test_OriginalFilePreservation(string dir)
        {
            string orig = Path.Combine(dir, "immutable.jpg");
            using (Bitmap bmp = new Bitmap(400, 300))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.IndianRed); }
                bmp.Save(orig, ImageFormat.Jpeg);
            }

            string hashBefore = ComputeFileSha256(orig);

            // Run convert, compress, and resize
            ImageEngine.Convert(orig, ImageFormatType.Png);
            long o, c;
            try { ImageEngine.Compress(orig, 50, out o, out c); } catch { }
            int rw, rh;
            ImageEngine.Resize(orig, 200, 150, out rw, out rh);

            string hashAfter = ComputeFileSha256(orig);
            Assert(hashBefore == hashAfter, "Original file hash is 100% unchanged after operations");
        }

        static void Test_CollisionAvoidance(string dir)
        {
            string src = Path.Combine(dir, "test_file.jpg");
            using (Bitmap bmp = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Crimson); }
                bmp.Save(src, ImageFormat.Jpeg);
            }

            string out1 = ImageEngine.Convert(src, ImageFormatType.Png);
            string out2 = ImageEngine.Convert(src, ImageFormatType.Png);
            string out3 = ImageEngine.Convert(src, ImageFormatType.Png);

            Assert(out1 != out2 && out2 != out3, "Auto-increment collision naming produced unique files");
            Assert(File.Exists(out1) && File.Exists(out2) && File.Exists(out3), "All collision copies exist simultaneously");
        }

        static void Test_InvalidAndCorruptFiles(string dir)
        {
            string emptyFile = Path.Combine(dir, "empty.jpg");
            File.WriteAllBytes(emptyFile, new byte[0]);

            bool threwEmpty = false;
            try { ImageEngine.Inspect(emptyFile); } catch { threwEmpty = true; }
            Assert(threwEmpty, "Empty file (0 bytes) rejected safely");

            string corruptFile = Path.Combine(dir, "corrupt.png");
            File.WriteAllText(corruptFile, "Not a real PNG at all!");

            bool threwCorrupt = false;
            try { ImageEngine.Convert(corruptFile, ImageFormatType.Jpeg); } catch { threwCorrupt = true; }
            Assert(threwCorrupt, "Corrupt file rejected safely");
        }

        static void Test_LargeHighResolutionImage(string dir)
        {
            string largeJpg = Path.Combine(dir, "large_4k.jpg");
            using (Bitmap bmp = new Bitmap(3840, 2160))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.DarkBlue); }
                bmp.Save(largeJpg, ImageFormat.Jpeg);
            }

            string outPng = ImageEngine.Convert(largeJpg, ImageFormatType.Png);
            Assert(File.Exists(outPng), "4K image converted to PNG successfully");

            using (Image check = Image.FromFile(outPng))
            {
                Assert(check.Width == 3840 && check.Height == 2160, "4K image dimensions preserved (3840x2160)");
            }
        }

        static void Test_MetadataInspection(string dir)
        {
            string src = Path.Combine(dir, "meta_test.jpg");
            using (Bitmap bmp = new Bitmap(1920, 1080))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.DarkOliveGreen); }
                bmp.Save(src, ImageFormat.Jpeg);
            }

            ImageMetadata meta = ImageEngine.GetMetadata(src);
            Assert(meta.Width == 1920 && meta.Height == 1080, "Metadata dimensions match 1920x1080");
            Assert(meta.AspectRatio == 1.78, "Metadata aspect ratio matches 1.78");
            Assert(meta.MegaPixels == 2.1, "Metadata megapixels matches 2.1");
            Assert(!string.IsNullOrEmpty(meta.ColorDepth), "Metadata color depth reported");
        }

        static void Test_MetadataStripping(string dir)
        {
            string src = Path.Combine(dir, "strip_test.jpg");
            using (Bitmap bmp = new Bitmap(500, 400))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Purple); }
                bmp.Save(src, ImageFormat.Jpeg);
            }

            string clean = ImageEngine.StripMetadata(src);
            Assert(File.Exists(clean), "Stripped metadata file exists");
            Assert(clean.EndsWith("_clean.jpg"), "Stripped file has clean suffix");
            using (Image img = Image.FromFile(clean))
            {
                Assert(img.Width == 500 && img.Height == 400, "Clean image dimensions preserved (500x400)");
            }
        }

        static void Test_ResizeFitMode(string dir)
        {
            string src = Path.Combine(dir, "fit_test.png");
            using (Bitmap bmp = new Bitmap(800, 600))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Coral); }
                bmp.Save(src, ImageFormat.Png);
            }

            int outW, outH;
            string res = ImageEngine.Resize(src, 400, 400, ResizeFitMode.Fit, false, false, out outW, out outH);
            Assert(File.Exists(res), "Resize Fit file exists");
            Assert(outW == 400 && outH == 300, "Resize Fit preserved 4:3 aspect ratio (400x300)");
        }

        static void Test_ResizeFillMode(string dir)
        {
            string src = Path.Combine(dir, "fill_test.png");
            using (Bitmap bmp = new Bitmap(800, 600))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.MediumSeaGreen); }
                bmp.Save(src, ImageFormat.Png);
            }

            int outW, outH;
            string res = ImageEngine.Resize(src, 400, 400, ResizeFitMode.Fill, false, false, out outW, out outH);
            Assert(File.Exists(res), "Resize Fill file exists");
            Assert(outW == 400 && outH == 400, "Resize Fill cropped to exact 400x400 box");
        }

        static void Test_ResizeAutoSwapOrientation(string dir)
        {
            string src = Path.Combine(dir, "portrait_test.jpg");
            using (Bitmap bmp = new Bitmap(600, 800))
            {
                using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.Gold); }
                bmp.Save(src, ImageFormat.Jpeg);
            }

            int outW, outH;
            string res = ImageEngine.Resize(src, 400, 300, ResizeFitMode.Fit, true, false, out outW, out outH);
            Assert(File.Exists(res), "Resize AutoSwap file exists");
            Assert(outW == 300 && outH == 400, "Auto-swap orientation correctly swapped landscape target to portrait (300x400)");
        }

        static void Test_CompressToTargetSize(string dir)
        {
            string src = Path.Combine(dir, "target_size.jpg");
            using (Bitmap bmp = new Bitmap(1400, 1000))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    for (int i = 0; i < 200; i++)
                    {
                        g.DrawLine(Pens.Indigo, i * 7, 0, i * 7, 1000);
                        g.DrawLine(Pens.Crimson, 0, i * 5, 1400, i * 5);
                    }
                }
                ImageCodecInfo encoder = null;
                foreach (var c in ImageCodecInfo.GetImageEncoders()) if (c.FormatID == ImageFormat.Jpeg.Guid) encoder = c;
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                bmp.Save(src, encoder, ep);
            }

            long targetBytes = 450 * 1024;
            long o, n;
            string comp = ImageEngine.CompressToTargetSize(src, targetBytes, true, out o, out n);
            Assert(File.Exists(comp), "Target size compressed file exists");
            Assert(n <= targetBytes || n < o, "Target size compressed file size is below ceiling or minimized", string.Format("{0} -> {1} bytes (target {2})", o, n, targetBytes));
        }
    }
}
