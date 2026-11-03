using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace VeqloQuick
{
    public enum ImageFormatType
    {
        Unknown,
        Jpeg,
        Png,
        WebP,
        Bmp,
        Tiff,
        Ico
    }

    public enum ResizeFitMode
    {
        Fit,       // Contain within box proportionally
        Fill,      // Crop to fill exact box
        Stretch    // Stretch to exact dimensions
    }

    public class ImageInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public ImageFormatType Format { get; set; }
        public string FormatName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSizeBytes { get; set; }
        public bool HasAlpha { get; set; }
    }

    public class ImageMetadata
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public string FormatName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double AspectRatio { get; set; }
        public double MegaPixels { get; set; }
        public float DpiX { get; set; }
        public float DpiY { get; set; }
        public string ColorDepth { get; set; }
        public bool HasAlpha { get; set; }

        // EXIF properties
        public string CameraMake { get; set; }
        public string CameraModel { get; set; }
        public string LensModel { get; set; }
        public string DateTaken { get; set; }
        public string ExposureTime { get; set; }
        public string FNumber { get; set; }
        public string IsoSpeed { get; set; }
        public string FocalLength { get; set; }
        public string Software { get; set; }
        public string GpsCoordinates { get; set; }
        public bool HasMetadata { get; set; }
    }

    public static class ImageEngine
    {
        public static string GetCWebpPath()
        {
            string[] candidates = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cwebp.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"package\cwebp.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\package\cwebp.exe"),
                Path.Combine(Environment.CurrentDirectory, "cwebp.exe"),
                Path.Combine(Environment.CurrentDirectory, @"package\cwebp.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Veqlo Quick\cwebp.exe")
            };
            foreach (string c in candidates)
            {
                try { if (File.Exists(c)) return Path.GetFullPath(c); } catch { }
            }
            return "cwebp.exe";
        }

        public static string GetDWebpPath()
        {
            string[] candidates = new string[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dwebp.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"package\dwebp.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\package\dwebp.exe"),
                Path.Combine(Environment.CurrentDirectory, "dwebp.exe"),
                Path.Combine(Environment.CurrentDirectory, @"package\dwebp.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Veqlo Quick\dwebp.exe")
            };
            foreach (string c in candidates)
            {
                try { if (File.Exists(c)) return Path.GetFullPath(c); } catch { }
            }
            return "dwebp.exe";
        }

        public static ImageFormatType DetectFormat(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return ImageFormatType.Unknown;

            byte[] header = new byte[16];
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length < 4) return ImageFormatType.Unknown;
                    fs.Read(header, 0, Math.Min(header.Length, (int)fs.Length));
                }

                // 1. JPEG: FF D8 FF
                if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    return ImageFormatType.Jpeg;

                // 2. PNG: 89 50 4E 47 0D 0A 1A 0A
                if (header.Length >= 8 &&
                    header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                    return ImageFormatType.Png;

                // 3. WebP: RIFF .... WEBP
                if (header.Length >= 12 &&
                    header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46)
                {
                    byte[] webpBytes = new byte[4];
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (fs.Length >= 12)
                        {
                            fs.Seek(8, SeekOrigin.Begin);
                            fs.Read(webpBytes, 0, 4);
                            if (webpBytes[0] == 0x57 && webpBytes[1] == 0x45 && webpBytes[2] == 0x42 && webpBytes[3] == 0x50)
                                return ImageFormatType.WebP;
                        }
                    }
                }

                // 4. BMP: 42 4D ('BM')
                if (header[0] == 0x42 && header[1] == 0x4D)
                    return ImageFormatType.Bmp;

                // 5. TIFF: 49 49 2A 00 (little endian) or 4D 4D 00 2A (big endian)
                if ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00) ||
                    (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A))
                    return ImageFormatType.Tiff;

                // 6. ICO: 00 00 01 00
                if (header[0] == 0x00 && header[1] == 0x00 && header[2] == 0x01 && header[3] == 0x00)
                    return ImageFormatType.Ico;

                // Extension fallback
                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                switch (ext)
                {
                    case ".jpg":
                    case ".jpeg": return ImageFormatType.Jpeg;
                    case ".png": return ImageFormatType.Png;
                    case ".webp": return ImageFormatType.WebP;
                    case ".bmp": return ImageFormatType.Bmp;
                    case ".tif":
                    case ".tiff": return ImageFormatType.Tiff;
                    case ".ico": return ImageFormatType.Ico;
                }
            }
            catch { }

            return ImageFormatType.Unknown;
        }

        public static ImageInfo Inspect(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);

            FileInfo fi = new FileInfo(filePath);
            if (fi.Length == 0)
                throw new InvalidOperationException("File is empty (0 bytes).");

            ImageFormatType fmt = DetectFormat(filePath);
            ImageInfo info = new ImageInfo
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Format = fmt,
                FormatName = GetFormatDisplayName(fmt),
                FileSizeBytes = fi.Length
            };

            if (fmt == ImageFormatType.WebP)
            {
                ReadWebPDimensions(filePath, info);
            }
            else
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image img = Image.FromStream(fs, false, false))
                {
                    info.Width = img.Width;
                    info.Height = img.Height;
                    info.HasAlpha = (img.PixelFormat == PixelFormat.Format32bppArgb ||
                                     img.PixelFormat == PixelFormat.Format32bppPArgb ||
                                     img.PixelFormat == PixelFormat.Format64bppArgb ||
                                     img.PixelFormat == PixelFormat.Format64bppPArgb);
                }
            }

            return info;
        }

        private static void ReadWebPDimensions(string filePath, ImageInfo info)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    if (fs.Length >= 30)
                    {
                        fs.Seek(12, SeekOrigin.Begin);
                        byte[] fourcc = br.ReadBytes(4);
                        string chunk = Encoding.ASCII.GetString(fourcc);
                        if (chunk == "VP8 ")
                        {
                            fs.Seek(26, SeekOrigin.Begin);
                            ushort w = br.ReadUInt16();
                            ushort h = br.ReadUInt16();
                            info.Width = w & 0x3FFF;
                            info.Height = h & 0x3FFF;
                            info.HasAlpha = false;
                            return;
                        }
                        else if (chunk == "VP8L")
                        {
                            fs.Seek(21, SeekOrigin.Begin);
                            byte b0 = br.ReadByte();
                            byte b1 = br.ReadByte();
                            byte b2 = br.ReadByte();
                            byte b3 = br.ReadByte();
                            info.Width = 1 + (((b1 & 0x3F) << 8) | b0);
                            info.Height = 1 + (((b3 & 0x0F) << 10) | (b2 << 2) | ((b1 & 0xC0) >> 6));
                            info.HasAlpha = (b3 & 0x10) != 0;
                            return;
                        }
                        else if (chunk == "VP8X")
                        {
                            fs.Seek(20, SeekOrigin.Begin);
                            byte flags = br.ReadByte();
                            fs.Seek(24, SeekOrigin.Begin);
                            int w = br.ReadByte() | (br.ReadByte() << 8) | (br.ReadByte() << 16);
                            int h = br.ReadByte() | (br.ReadByte() << 8) | (br.ReadByte() << 16);
                            info.Width = w + 1;
                            info.Height = h + 1;
                            info.HasAlpha = (flags & 0x10) != 0;
                            return;
                        }
                    }
                }
            }
            catch { }

            string tempPng = Path.Combine(Path.GetTempPath(), "veqlo_webp_info_" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                string dwebp = GetDWebpPath();
                if (File.Exists(dwebp))
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = dwebp,
                        Arguments = string.Format("\"{0}\" -o \"{1}\"", filePath, tempPng),
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit(3000);
                    }
                    if (File.Exists(tempPng))
                    {
                        using (Image img = Image.FromFile(tempPng))
                        {
                            info.Width = img.Width;
                            info.Height = img.Height;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (File.Exists(tempPng)) try { File.Delete(tempPng); } catch { }
            }
        }

        public static ImageMetadata GetMetadata(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found: " + filePath);

            FileInfo fi = new FileInfo(filePath);
            ImageInfo info = Inspect(filePath);

            ImageMetadata meta = new ImageMetadata
            {
                FilePath = filePath,
                FileName = fi.Name,
                FileSizeBytes = fi.Length,
                FormatName = info.FormatName,
                Width = info.Width,
                Height = info.Height,
                HasAlpha = info.HasAlpha
            };

            if (info.Width > 0 && info.Height > 0)
            {
                meta.AspectRatio = Math.Round((double)info.Width / info.Height, 2);
                meta.MegaPixels = Math.Round((double)(info.Width * info.Height) / 1000000.0, 1);
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (Image img = Image.FromStream(fs, false, false))
                {
                    meta.DpiX = (float)Math.Round(img.HorizontalResolution, 1);
                    meta.DpiY = (float)Math.Round(img.VerticalResolution, 1);
                    meta.ColorDepth = Image.GetPixelFormatSize(img.PixelFormat) + "-bit " + img.PixelFormat.ToString();

                    PropertyItem[] props = img.PropertyItems;
                    if (props != null && props.Length > 0)
                    {
                        meta.HasMetadata = true;
                        foreach (PropertyItem prop in props)
                        {
                            switch (prop.Id)
                            {
                                case 0x010F: // Camera Make
                                    meta.CameraMake = Encoding.UTF8.GetString(prop.Value).Trim('\0', ' ');
                                    break;
                                case 0x0110: // Camera Model
                                    meta.CameraModel = Encoding.UTF8.GetString(prop.Value).Trim('\0', ' ');
                                    break;
                                case 0xA434: // Lens Model
                                    meta.LensModel = Encoding.UTF8.GetString(prop.Value).Trim('\0', ' ');
                                    break;
                                case 0x0132: // Date Time
                                case 0x9003: // Date Time Original
                                    if (string.IsNullOrEmpty(meta.DateTaken))
                                        meta.DateTaken = Encoding.UTF8.GetString(prop.Value).Trim('\0', ' ');
                                    break;
                                case 0x0131: // Software
                                    meta.Software = Encoding.UTF8.GetString(prop.Value).Trim('\0', ' ');
                                    break;
                                case 0x829D: // FNumber
                                    if (prop.Value.Length >= 8)
                                    {
                                        uint num = BitConverter.ToUInt32(prop.Value, 0);
                                        uint den = BitConverter.ToUInt32(prop.Value, 4);
                                        if (den > 0) meta.FNumber = "f/" + ((double)num / den).ToString("0.0");
                                    }
                                    break;
                                case 0x829A: // ExposureTime
                                    if (prop.Value.Length >= 8)
                                    {
                                        uint num = BitConverter.ToUInt32(prop.Value, 0);
                                        uint den = BitConverter.ToUInt32(prop.Value, 4);
                                        if (num == 1) meta.ExposureTime = "1/" + den + "s";
                                        else if (den > 0) meta.ExposureTime = ((double)num / den).ToString("0.###") + "s";
                                    }
                                    break;
                                case 0x8827: // ISO
                                    if (prop.Value.Length >= 2)
                                    {
                                        meta.IsoSpeed = "ISO " + BitConverter.ToUInt16(prop.Value, 0);
                                    }
                                    break;
                                case 0x920A: // FocalLength
                                    if (prop.Value.Length >= 8)
                                    {
                                        uint num = BitConverter.ToUInt32(prop.Value, 0);
                                        uint den = BitConverter.ToUInt32(prop.Value, 4);
                                        if (den > 0) meta.FocalLength = Math.Round((double)num / den, 1) + " mm";
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            catch { }

            return meta;
        }

        public static string StripMetadata(string inputPath, string outputDir = null)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("File not found: " + inputPath);

            string dir = string.IsNullOrEmpty(outputDir) ? Path.GetDirectoryName(inputPath) : outputDir;
            string baseName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath);
            ImageFormatType currentFmt = DetectFormat(inputPath);

            string candidate = Path.Combine(dir, baseName + "_clean" + ext);
            int count = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, string.Format("{0}_clean ({1}){2}", baseName, count, ext));
                count++;
            }

            string tempPath = Path.Combine(dir, string.Format(".veqlo_strip_{0}_{1}", Guid.NewGuid().ToString("N"), ext));
            try
            {
                using (Bitmap src = LoadBitmap(inputPath))
                {
                    PixelFormat pfmt = (currentFmt == ImageFormatType.Jpeg || currentFmt == ImageFormatType.Bmp)
                        ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

                    using (Bitmap cleanBmp = new Bitmap(src.Width, src.Height, pfmt))
                    {
                        cleanBmp.SetResolution(src.HorizontalResolution, src.VerticalResolution);
                        using (Graphics g = Graphics.FromImage(cleanBmp))
                        {
                            if (pfmt == PixelFormat.Format24bppRgb) g.Clear(Color.White);
                            else g.Clear(Color.Transparent);
                            g.DrawImage(src, 0, 0, src.Width, src.Height);
                        }

                        // Save without PropertyItems (completely clean metadata)
                        if (currentFmt == ImageFormatType.Jpeg) SaveAsJpeg(cleanBmp, tempPath, 92);
                        else if (currentFmt == ImageFormatType.Png) SaveAsPng(cleanBmp, tempPath);
                        else if (currentFmt == ImageFormatType.WebP) SaveAsWebP(cleanBmp, tempPath, 85);
                        else if (currentFmt == ImageFormatType.Bmp) SaveAsBmp(cleanBmp, tempPath);
                        else if (currentFmt == ImageFormatType.Tiff) SaveAsTiff(cleanBmp, tempPath);
                        else SaveAsPng(cleanBmp, tempPath);
                    }
                }

                File.Move(tempPath, candidate);
                tempPath = null;
                return candidate;
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        public static Bitmap LoadBitmap(string filePath)
        {
            ImageFormatType fmt = DetectFormat(filePath);
            if (fmt == ImageFormatType.WebP)
            {
                string tempPng = Path.Combine(Path.GetTempPath(), "veqlo_dwebp_" + Guid.NewGuid().ToString("N") + ".png");
                try
                {
                    string dwebp = GetDWebpPath();
                    if (!File.Exists(dwebp))
                        throw new FileNotFoundException("dwebp.exe not found to decode WebP image.");

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = dwebp,
                        Arguments = string.Format("\"{0}\" -o \"{1}\"", filePath, tempPng),
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit(10000);
                        if (p.ExitCode != 0)
                            throw new InvalidOperationException("Failed to decode WebP image via dwebp.");
                    }

                    byte[] pngBytes = File.ReadAllBytes(tempPng);
                    using (MemoryStream ms = new MemoryStream(pngBytes))
                    {
                        return new Bitmap(ms);
                    }
                }
                finally
                {
                    if (File.Exists(tempPng)) try { File.Delete(tempPng); } catch { }
                }
            }
            else
            {
                byte[] bytes;
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    bytes = new byte[fs.Length];
                    int offset = 0;
                    while (offset < bytes.Length)
                    {
                        int r = fs.Read(bytes, offset, bytes.Length - offset);
                        if (r == 0) break;
                        offset += r;
                    }
                }

                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    return new Bitmap(ms);
                }
            }
        }

        public static string Convert(string inputPath, ImageFormatType targetFormat)
        {
            return Convert(inputPath, targetFormat, 90, true, null);
        }

        public static string Convert(string inputPath, ImageFormatType targetFormat, int quality, bool preserveAlpha, string outputDir)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Source file does not exist: " + inputPath);

            string dir = string.IsNullOrEmpty(outputDir) ? Path.GetDirectoryName(inputPath) : outputDir;
            string baseName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = GetDefaultExtension(targetFormat);
            string candidate = Path.Combine(dir, baseName + ext);

            int count = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, string.Format("{0} ({1}){2}", baseName, count, ext));
                count++;
            }

            string tempPath = Path.Combine(dir, string.Format(".veqlo_tmp_{0}_{1}", Guid.NewGuid().ToString("N"), ext));

            try
            {
                using (Bitmap srcBmp = LoadBitmap(inputPath))
                {
                    switch (targetFormat)
                    {
                        case ImageFormatType.Jpeg:
                            SaveAsJpeg(srcBmp, tempPath, quality);
                            break;

                        case ImageFormatType.Png:
                            SaveAsPng(srcBmp, tempPath);
                            break;

                        case ImageFormatType.WebP:
                            SaveAsWebP(srcBmp, tempPath, quality);
                            break;

                        case ImageFormatType.Bmp:
                            SaveAsBmp(srcBmp, tempPath);
                            break;

                        case ImageFormatType.Tiff:
                            SaveAsTiff(srcBmp, tempPath);
                            break;

                        case ImageFormatType.Ico:
                            SaveAsIco(srcBmp, tempPath);
                            break;

                        default:
                            throw new NotSupportedException("Unsupported target format: " + targetFormat);
                    }
                }

                FileInfo fi = new FileInfo(tempPath);
                if (!fi.Exists || fi.Length == 0)
                    throw new InvalidOperationException("Generated output file is empty.");

                ImageFormatType createdFmt = DetectFormat(tempPath);
                if (createdFmt != targetFormat)
                {
                    throw new InvalidOperationException(string.Format(
                        "Format signature verification failed: expected {0}, got {1}.", targetFormat, createdFmt));
                }

                File.Move(tempPath, candidate);
                tempPath = null;
                return candidate;
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        public static string Compress(string inputPath, int qualityLevel, out long originalSize, out long newSize)
        {
            return Compress(inputPath, qualityLevel, false, out originalSize, out newSize, null);
        }

        public static string Compress(string inputPath, int qualityLevel, bool stripMetadata, out long originalSize, out long newSize, string outputDir)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Source file does not exist: " + inputPath);

            FileInfo fiOrig = new FileInfo(inputPath);
            originalSize = fiOrig.Length;

            string dir = string.IsNullOrEmpty(outputDir) ? Path.GetDirectoryName(inputPath) : outputDir;
            string baseName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath).ToLowerInvariant();
            ImageFormatType currentFmt = DetectFormat(inputPath);

            string candidate = Path.Combine(dir, string.Format("{0}_compressed{1}", baseName, ext));
            int count = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, string.Format("{0}_compressed ({1}){2}", baseName, count, ext));
                count++;
            }

            string tempPath = Path.Combine(dir, string.Format(".veqlo_comp_{0}_{1}", Guid.NewGuid().ToString("N"), ext));

            try
            {
                using (Bitmap srcBmp = LoadBitmap(inputPath))
                {
                    if (currentFmt == ImageFormatType.WebP)
                    {
                        SaveAsWebP(srcBmp, tempPath, qualityLevel);
                    }
                    else if (currentFmt == ImageFormatType.Jpeg)
                    {
                        SaveAsJpeg(srcBmp, tempPath, qualityLevel);
                    }
                    else if (currentFmt == ImageFormatType.Png)
                    {
                        SaveAsOptimizedPng(srcBmp, tempPath, qualityLevel);
                    }
                    else if (currentFmt == ImageFormatType.Tiff)
                    {
                        SaveAsTiff(srcBmp, tempPath);
                    }
                    else
                    {
                        SaveAsJpeg(srcBmp, tempPath, qualityLevel);
                    }
                }

                FileInfo fiComp = new FileInfo(tempPath);
                newSize = fiComp.Length;

                if (newSize >= originalSize)
                {
                    int aggressiveQuality = Math.Max(25, qualityLevel - 25);
                    using (Bitmap srcBmp = LoadBitmap(inputPath))
                    {
                        if (currentFmt == ImageFormatType.WebP)
                            SaveAsWebP(srcBmp, tempPath, aggressiveQuality);
                        else if (currentFmt == ImageFormatType.Jpeg)
                            SaveAsJpeg(srcBmp, tempPath, aggressiveQuality);
                        else
                            SaveAsOptimizedPng(srcBmp, tempPath, aggressiveQuality);
                    }

                    fiComp.Refresh();
                    newSize = fiComp.Length;

                    if (newSize >= originalSize)
                    {
                        throw new InvalidOperationException(
                            "The file is already highly optimized. Re-encoding at this quality did not reduce the file size.");
                    }
                }

                File.Move(tempPath, candidate);
                tempPath = null;
                return candidate;
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        public static string CompressToTargetSize(string inputPath, long maxTargetBytes, bool stripMetadata, out long originalSize, out long newSize, string outputDir = null)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Source file does not exist: " + inputPath);

            FileInfo fiOrig = new FileInfo(inputPath);
            originalSize = fiOrig.Length;

            int low = 15;
            int high = 95;
            string bestPath = null;
            long bestSize = originalSize;

            for (int iter = 0; iter < 4; iter++)
            {
                int mid = (low + high) / 2;
                long curOrig, curNew;
                try
                {
                    string candidate = Compress(inputPath, mid, stripMetadata, out curOrig, out curNew, outputDir);
                    if (curNew <= maxTargetBytes)
                    {
                        if (bestPath != null && File.Exists(bestPath)) try { File.Delete(bestPath); } catch { }
                        bestPath = candidate;
                        bestSize = curNew;
                        low = mid + 1;
                    }
                    else
                    {
                        if (bestPath == null || curNew < bestSize)
                        {
                            if (bestPath != null && File.Exists(bestPath)) try { File.Delete(bestPath); } catch { }
                            bestPath = candidate;
                            bestSize = curNew;
                        }
                        else
                        {
                            if (File.Exists(candidate)) try { File.Delete(candidate); } catch { }
                        }
                        high = mid - 1;
                    }
                }
                catch
                {
                    high = mid - 1;
                }

                if (low > high) break;
            }

            if (bestPath != null && File.Exists(bestPath))
            {
                newSize = bestSize;
                return bestPath;
            }

            return Compress(inputPath, 50, stripMetadata, out originalSize, out newSize, outputDir);
        }

        public static string Resize(string inputPath, int targetWidth, int targetHeight, out int resultWidth, out int resultHeight)
        {
            return Resize(inputPath, targetWidth, targetHeight, ResizeFitMode.Stretch, false, false, out resultWidth, out resultHeight, null);
        }

        public static string Resize(string inputPath, int targetWidth, int targetHeight, ResizeFitMode fitMode, bool autoSwapOrientation, bool preventUpscale, out int resultWidth, out int resultHeight, string outputDir = null)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Source file does not exist: " + inputPath);

            if (targetWidth <= 0 || targetHeight <= 0)
                throw new ArgumentException("Dimensions must be positive numbers.");

            string dir = string.IsNullOrEmpty(outputDir) ? Path.GetDirectoryName(inputPath) : outputDir;
            string baseName = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath);
            ImageFormatType currentFmt = DetectFormat(inputPath);

            string candidate = Path.Combine(dir, string.Format("{0}_{1}x{2}{3}", baseName, targetWidth, targetHeight, ext));
            int count = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, string.Format("{0}_{1}x{2} ({3}){4}", baseName, targetWidth, targetHeight, count, ext));
                count++;
            }

            string tempPath = Path.Combine(dir, string.Format(".veqlo_res_{0}_{1}", Guid.NewGuid().ToString("N"), ext));

            try
            {
                using (Bitmap srcBmp = LoadBitmap(inputPath))
                {
                    // 1. Orientation check
                    if (autoSwapOrientation)
                    {
                        bool isSrcPortrait = srcBmp.Height > srcBmp.Width;
                        bool isTargetLandscape = targetWidth > targetHeight;
                        if (isSrcPortrait && isTargetLandscape)
                        {
                            int tmp = targetWidth;
                            targetWidth = targetHeight;
                            targetHeight = tmp;
                        }
                    }

                    // 2. Prevent upscale check
                    if (preventUpscale && srcBmp.Width <= targetWidth && srcBmp.Height <= targetHeight)
                    {
                        targetWidth = srcBmp.Width;
                        targetHeight = srcBmp.Height;
                    }

                    int finalW = targetWidth;
                    int finalH = targetHeight;

                    // 3. Compute bounding dimensions based on fit mode
                    if (fitMode == ResizeFitMode.Fit)
                    {
                        double ratio = Math.Min((double)targetWidth / srcBmp.Width, (double)targetHeight / srcBmp.Height);
                        finalW = Math.Max(1, (int)Math.Round(srcBmp.Width * ratio));
                        finalH = Math.Max(1, (int)Math.Round(srcBmp.Height * ratio));
                    }

                    PixelFormat pfmt = (currentFmt == ImageFormatType.Jpeg || currentFmt == ImageFormatType.Bmp) ?
                        PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

                    using (Bitmap resizedBmp = new Bitmap(finalW, finalH, pfmt))
                    {
                        resizedBmp.SetResolution(srcBmp.HorizontalResolution, srcBmp.VerticalResolution);
                        using (Graphics g = Graphics.FromImage(resizedBmp))
                        {
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            if (pfmt == PixelFormat.Format24bppRgb)
                                g.Clear(Color.White);
                            else
                                g.Clear(Color.Transparent);

                            if (fitMode == ResizeFitMode.Fill)
                            {
                                double ratio = Math.Max((double)targetWidth / srcBmp.Width, (double)targetHeight / srcBmp.Height);
                                int scaledW = (int)Math.Round(srcBmp.Width * ratio);
                                int scaledH = (int)Math.Round(srcBmp.Height * ratio);
                                int cropX = (scaledW - targetWidth) / 2;
                                int cropY = (scaledH - targetHeight) / 2;

                                using (ImageAttributes wrapMode = new ImageAttributes())
                                {
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    g.DrawImage(srcBmp, new Rectangle(0, 0, targetWidth, targetHeight), cropX, cropY, (int)(targetWidth / ratio), (int)(targetHeight / ratio), GraphicsUnit.Pixel, wrapMode);
                                }
                            }
                            else
                            {
                                using (ImageAttributes wrapMode = new ImageAttributes())
                                {
                                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                                    g.DrawImage(srcBmp, new Rectangle(0, 0, finalW, finalH), 0, 0, srcBmp.Width, srcBmp.Height, GraphicsUnit.Pixel, wrapMode);
                                }
                            }
                        }

                        switch (currentFmt)
                        {
                            case ImageFormatType.Jpeg:
                                SaveAsJpeg(resizedBmp, tempPath, 92);
                                break;
                            case ImageFormatType.Png:
                                SaveAsPng(resizedBmp, tempPath);
                                break;
                            case ImageFormatType.WebP:
                                SaveAsWebP(resizedBmp, tempPath, 85);
                                break;
                            case ImageFormatType.Bmp:
                                SaveAsBmp(resizedBmp, tempPath);
                                break;
                            case ImageFormatType.Tiff:
                                SaveAsTiff(resizedBmp, tempPath);
                                break;
                            case ImageFormatType.Ico:
                                SaveAsIco(resizedBmp, tempPath);
                                break;
                            default:
                                SaveAsPng(resizedBmp, tempPath);
                                break;
                        }

                        resultWidth = finalW;
                        resultHeight = finalH;
                    }
                }

                File.Move(tempPath, candidate);
                tempPath = null;
                return candidate;
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempPath) && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        // --- Low-level encoder helpers ---

        private static void SaveAsJpeg(Bitmap bmp, string outPath, int quality)
        {
            using (Bitmap rgbBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb))
            {
                rgbBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
                using (Graphics g = Graphics.FromImage(rgbBmp))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                }

                ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                using (FileStream fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    rgbBmp.Save(fs, jpegEncoder, ep);
                    fs.Flush(true);
                }
            }
        }

        private static void SaveAsPng(Bitmap bmp, string outPath)
        {
            using (Bitmap destBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb))
            {
                destBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
                using (Graphics g = Graphics.FromImage(destBmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                }

                using (FileStream fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    destBmp.Save(fs, ImageFormat.Png);
                    fs.Flush(true);
                }
            }
        }

        private static void SaveAsOptimizedPng(Bitmap bmp, string outPath, int quality)
        {
            SaveAsPng(bmp, outPath);
        }

        private static void SaveAsWebP(Bitmap bmp, string outPath, int quality)
        {
            string tempPng = Path.Combine(Path.GetTempPath(), "veqlo_webp_encode_" + Guid.NewGuid().ToString("N") + ".png");
            try
            {
                SaveAsPng(bmp, tempPng);

                string cwebp = GetCWebpPath();
                if (!File.Exists(cwebp))
                    throw new FileNotFoundException("cwebp.exe not found to encode WebP image.");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = cwebp,
                    Arguments = string.Format("-q {0} \"{1}\" -o \"{2}\"", quality, tempPng, outPath),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process p = Process.Start(psi))
                {
                    p.WaitForExit(10000);
                    if (p.ExitCode != 0)
                        throw new InvalidOperationException("Failed to encode WebP image via cwebp.");
                }
            }
            finally
            {
                if (File.Exists(tempPng)) try { File.Delete(tempPng); } catch { }
            }
        }

        private static void SaveAsBmp(Bitmap bmp, string outPath)
        {
            using (Bitmap rgbBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb))
            {
                rgbBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
                using (Graphics g = Graphics.FromImage(rgbBmp))
                {
                    g.Clear(Color.White);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                }

                using (FileStream fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    rgbBmp.Save(fs, ImageFormat.Bmp);
                    fs.Flush(true);
                }
            }
        }

        private static void SaveAsTiff(Bitmap bmp, string outPath)
        {
            using (Bitmap destBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb))
            {
                destBmp.SetResolution(bmp.HorizontalResolution, bmp.VerticalResolution);
                using (Graphics g = Graphics.FromImage(destBmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
                }

                ImageCodecInfo tiffEncoder = GetEncoder(ImageFormat.Tiff);
                EncoderParameters ep = new EncoderParameters(1);
                ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, (long)EncoderValue.CompressionLZW);

                using (FileStream fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    destBmp.Save(fs, tiffEncoder, ep);
                    fs.Flush(true);
                }
            }
        }

        private static void SaveAsIco(Bitmap bmp, string outPath)
        {
            int[] standardSizes = new int[] { 256, 48, 32, 16 };
            List<int> validSizes = new List<int>();

            foreach (int sz in standardSizes)
            {
                if (bmp.Width >= sz && bmp.Height >= sz)
                {
                    validSizes.Add(sz);
                }
            }

            if (validSizes.Count == 0) validSizes.Add(32);

            List<byte[]> pngImages = new List<byte[]>();
            List<int> iconSizes = new List<int>();

            foreach (int sz in validSizes)
            {
                using (Bitmap iconBmp = new Bitmap(sz, sz, PixelFormat.Format32bppArgb))
                {
                    using (Graphics g = Graphics.FromImage(iconBmp))
                    {
                        g.Clear(Color.Transparent);
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(bmp, 0, 0, sz, sz);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        iconBmp.Save(ms, ImageFormat.Png);
                        pngImages.Add(ms.ToArray());
                        iconSizes.Add(sz);
                    }
                }
            }

            using (FileStream fs = new FileStream(outPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write((ushort)0); // reserved
                bw.Write((ushort)1); // icon type
                bw.Write((ushort)pngImages.Count); // number of images

                uint offset = (uint)(6 + (16 * pngImages.Count));

                for (int i = 0; i < pngImages.Count; i++)
                {
                    int sz = iconSizes[i];
                    bw.Write((byte)(sz >= 256 ? 0 : sz)); // width (0 = 256)
                    bw.Write((byte)(sz >= 256 ? 0 : sz)); // height (0 = 256)
                    bw.Write((byte)0); // color count
                    bw.Write((byte)0); // reserved
                    bw.Write((ushort)1); // color planes
                    bw.Write((ushort)32); // bits per pixel
                    bw.Write((uint)pngImages[i].Length); // size of image data
                    bw.Write((uint)offset); // offset

                    offset += (uint)pngImages[i].Length;
                }

                for (int i = 0; i < pngImages.Count; i++)
                {
                    bw.Write(pngImages[i]);
                }

                bw.Flush();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        public static string GetFormatDisplayName(ImageFormatType type)
        {
            switch (type)
            {
                case ImageFormatType.Jpeg: return "JPEG";
                case ImageFormatType.Png: return "PNG";
                case ImageFormatType.WebP: return "WebP";
                case ImageFormatType.Bmp: return "BMP";
                case ImageFormatType.Tiff: return "TIFF";
                case ImageFormatType.Ico: return "ICO";
                default: return "Unknown";
            }
        }

        public static string GetDefaultExtension(ImageFormatType type)
        {
            switch (type)
            {
                case ImageFormatType.Jpeg: return ".jpg";
                case ImageFormatType.Png: return ".png";
                case ImageFormatType.WebP: return ".webp";
                case ImageFormatType.Bmp: return ".bmp";
                case ImageFormatType.Tiff: return ".tiff";
                case ImageFormatType.Ico: return ".ico";
                default: return ".dat";
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("0.#") + " KB";
            return (bytes / (1024.0 * 1024.0)).ToString("0.##") + " MB";
        }
    }
}
