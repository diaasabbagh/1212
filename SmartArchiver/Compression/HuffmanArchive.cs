using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SmartArchiver.Compression
{
    internal static class HuffmanArchive
    {
        public static double CompressFiles(IEnumerable<(string path, string entryName)> files, string archivePath, CancellationToken token, string password = null)
        {
            var allFiles = files.ToList();
            long originalTotal = allFiles.Sum(f => new FileInfo(f.path).Length);
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(allFiles.Count);
                foreach (var file in allFiles)
                {
                    token.ThrowIfCancellationRequested();
                    HuffmanCodec.CompressFile(file.path, file.entryName, writer, token);
                }
                writer.Flush();
                EncryptionUtils.WriteData(archivePath, ms.ToArray(), password);
            }
            long archiveSize = new FileInfo(archivePath).Length;
            if (archiveSize == 0) return 0;
            double ratio = 100.0 - (archiveSize * 100.0 / originalTotal);
            return ratio;
        }

        public static void ExtractAll(string archivePath, string outputDirectory, CancellationToken token, string password = null)
        {
            byte[] data = EncryptionUtils.ReadData(archivePath, password);
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    HuffmanCodec.DecompressFile(reader, outputDirectory, null, token);
                }
            }
        }

        public static void ExtractFile(string archivePath, string fileName, string outputDirectory, CancellationToken token, string password = null)
        {
            byte[] data = EncryptionUtils.ReadData(archivePath, password);
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    long posBefore = ms.Position;
                    string name = reader.ReadString();
                    ms.Position = posBefore;
                    if (name == fileName)
                    {
                        HuffmanCodec.DecompressFile(reader, outputDirectory, fileName, token);
                        return;
                    }
                    else
                    {
                        // skip this entry
                        SkipEntry(reader);
                    }
                }
            }
        }
        public static List<string> GetFileNames(string archivePath, string password = null)
        {
            var names = new List<string>();
            byte[] data = EncryptionUtils.ReadData(archivePath, password);
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    long posBefore = ms.Position;
                    string name = reader.ReadString();
                    names.Add(name);
                    ms.Position = posBefore;
                    SkipEntry(reader);
                }
            }
            return names;
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
