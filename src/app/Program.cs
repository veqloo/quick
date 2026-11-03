using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace VeqloQuick
{
    public class Program : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Program app = new Program();

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
                else if (arg.Equals("inspect", StringComparison.OrdinalIgnoreCase) || arg.Equals("details", StringComparison.OrdinalIgnoreCase) || arg.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    action = "inspect";
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
                    "Veqlo Quick is a Windows File Explorer context-menu shell utility.\n\n" +
                    "To use it, right-click any image file (JPG, PNG, WebP, BMP, TIFF, ICO) in File Explorer and select Veqlo Quick > Convert, Compress, or Resize.",
                    "Veqlo Quick",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            Window mainWin;
            if (action == "compress")
            {
                mainWin = new CompressWindow(inputFiles);
            }
            else if (action == "resize")
            {
                mainWin = new ResizeWindow(inputFiles);
            }
            else if (action == "inspect")
            {
                mainWin = new InspectWindow(inputFiles);
            }
            else
            {
                mainWin = new ConvertWindow(inputFiles);
            }

            app.Run(mainWin);
        }
    }
}
