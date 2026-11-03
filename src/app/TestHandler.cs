using System;
using System.IO;
using System.Windows.Forms;

namespace VeqloQuick
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string requestedAction = null;
            string inputPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("convert", StringComparison.OrdinalIgnoreCase))
                {
                    requestedAction = "convert";
                }
                else if (args[i].Equals("--input", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    inputPath = args[i + 1];
                    i++;
                }
            }

            if (string.IsNullOrEmpty(inputPath))
            {
                MessageBox.Show(
                    "Veqlo Quick se inici\u00F3 sin un archivo especificado.\nUso esperado: VeqloQuick.exe convert --input <ruta_archivo>",
                    "Veqlo Quick PoC",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(inputPath))
            {
                MessageBox.Show(
                    string.Format("El archivo no existe:\n{0}", inputPath),
                    "Veqlo Quick - Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Inspect magic bytes for JPEG
            bool isRealJpeg = false;
            try
            {
                using (var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Length >= 3)
                    {
                        byte b1 = (byte)fs.ReadByte();
                        byte b2 = (byte)fs.ReadByte();
                        byte b3 = (byte)fs.ReadByte();
                        if (b1 == 0xFF && b2 == 0xD8 && b3 == 0xFF)
                        {
                            isRealJpeg = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error al leer el archivo:\n{0}", ex.Message), "Veqlo Quick", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fileName = Path.GetFileName(inputPath);
            string fileExt = Path.GetExtension(inputPath).ToUpperInvariant();
            string status = isRealJpeg ? "Formato v\u00E1lido (JPEG SOI detectado)" : "Advertencia: Firma de archivo no coincide con JPEG";

            string message = string.Format(
                "Veqlo Quick recibi\u00F3 la acci\u00F3n: {0}\n\n" +
                "Archivo: {1}\n" +
                "Ruta completa: {2}\n" +
                "Extensi\u00F3n: {3}\n" +
                "Validaci\u00F3n de bytes: {4}\n\n" +
                "Handoff completado exitosamente desde el men\u00FA contextual.",
                requestedAction, fileName, inputPath, fileExt, status);

            MessageBox.Show(
                message,
                "Veqlo Quick - Integraci\u00F3n Shell Verificada",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
