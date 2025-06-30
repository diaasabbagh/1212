using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SmartArchiver.Compression;

namespace SmartArchiver
{
    internal static class Program
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            List<string> guiFiles = null;
            if (args.Length > 0)
            {
                try
                {
                    string cmd = args[0].ToLowerInvariant();
                    if (cmd == "cli-compress" && args.Length == 2)
                    {
                        string target = args[1];
                        var files = FileUtils.ExpandFileList(new List<string> { target });
                        string archive = target + ".huff";
                        HuffmanArchive.CompressFiles(files, archive, CancellationToken.None, null);                        return;
                    }
                    else if (cmd == "cli-decompress" && args.Length == 2)
                    {
                        string archive = args[1];
                        string outDir = Path.GetDirectoryName(Path.GetFullPath(archive)) ?? ".";
                        if (archive.EndsWith(".huff", StringComparison.OrdinalIgnoreCase))
                        {
                            HuffmanArchive.ExtractAll(archive, outDir, CancellationToken.None, null);                            return;
                        }
                        else if (archive.EndsWith(".shfn", StringComparison.OrdinalIgnoreCase))
                        {
                       ShannonFanoArchive.ExtractAll(archive, outDir, CancellationToken.None, null);                            return;
                        }
                    }
                    else
                    {
                        guiFiles = args.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (guiFiles != null)
            {
                Application.Run(new Form1(guiFiles));
            }
            else
            {
                Application.Run(new Form1());
            }
        }
    }
}
