using System;
using System.Collections.Generic;
using System.IO;

namespace SmartArchiver
{
    internal static class FileUtils
    {
        public static List<(string path, string entryName)> ExpandFileList(List<string> items)
        {
            var result = new List<(string path, string entryName)>();
            foreach (var item in items)
            {
                if (Directory.Exists(item))
                {
                    string baseName = Path.GetFileName(item);
                    foreach (var file in Directory.GetFiles(item, "*", SearchOption.AllDirectories))
                    {
                        string relativePart = file.Substring(item.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        string relative = Path.Combine(baseName, relativePart);
                        result.Add((file, relative));
                    }
                }
                else if (File.Exists(item))
                {
                    result.Add((item, Path.GetFileName(item)));
                }
            }
            return result;
        }
    }
}