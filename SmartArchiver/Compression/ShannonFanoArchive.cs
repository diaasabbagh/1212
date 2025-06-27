using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SmartArchiver.Compression
{
    internal static class ShannonFanoArchive
    {
        public static double CompressFiles(IEnumerable<string> filePaths, string archivePath, CancellationToken token)
        {
            var allFiles = filePaths.ToList();
            long originalTotal = allFiles.Sum(f => new FileInfo(f).Length);
            using (var fs = new FileStream(archivePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write(allFiles.Count);
                foreach (var file in allFiles)
                {
                    ShannonFanoCodec.CompressFile(file, writer, token);
                }
            }
            long archiveSize = new FileInfo(archivePath).Length;
            if (archiveSize == 0) return 0;
            double ratio = 100.0 - (archiveSize * 100.0 / originalTotal);
            return ratio;
        }

        public static void ExtractAll(string archivePath, string outputDirectory, CancellationToken token)
        {
            using (var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    ShannonFanoCodec.DecompressFile(reader, outputDirectory, null, token);
                }
            }
        }

        public static void ExtractFile(string archivePath, string fileName, string outputDirectory, CancellationToken token)
        {
            using (var fs = new FileStream(archivePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    long posBefore = fs.Position;
                    string name = reader.ReadString();
                    fs.Position = posBefore;
                    if (name == fileName)
                    {
                        ShannonFanoCodec.DecompressFile(reader, outputDirectory, fileName, token);
                        return;
                    }
                    else
                    {
                        SkipEntry(reader);
                    }
                }
            }
        }

        private static void SkipEntry(BinaryReader reader)
        {
            reader.ReadString();
            int origLen = reader.ReadInt32();
            int freqCount = reader.ReadInt32();
            for (int i = 0; i < freqCount; i++)
            {
                reader.ReadByte();
                reader.ReadInt32();
            }
            int bitLength = reader.ReadInt32();
            int compLength = reader.ReadInt32();
            reader.BaseStream.Seek(compLength, SeekOrigin.Current);
        }
    }
}
