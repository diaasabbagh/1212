using System.Collections.Generic;
using System.IO;

namespace SmartArchiver.Compression
{
    internal static class ShannonFanoCodec
    {
        public static void CompressFile(string inputPath, string entryName, BinaryWriter writer, System.Threading.CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            byte[] data = File.ReadAllBytes(inputPath);
            token.ThrowIfCancellationRequested();
            var tree = new ShannonFanoTree();
            var freq = tree.Build(data);
            token.ThrowIfCancellationRequested();
            var compressed = tree.Encode(data, out int bitLength, token);

            writer.Write(entryName);
            writer.Write(data.Length);
            writer.Write(freq.Count);
            foreach (var kv in freq)
            {
                writer.Write(kv.Key);
                writer.Write(kv.Value);
            }
            writer.Write(bitLength);
            writer.Write(compressed.Length);
            writer.Write(compressed);
        }

        public static void DecompressFile(BinaryReader reader, string outputDirectory, string expectedName, System.Threading.CancellationToken token)
        {
            string name = reader.ReadString();
            int originalLength = reader.ReadInt32();
            int freqCount = reader.ReadInt32();
            var freq = new Dictionary<byte, int>();
            for (int i = 0; i < freqCount; i++)
            {
                byte symbol = reader.ReadByte();
                int f = reader.ReadInt32();
                freq[symbol] = f;
            }
            int bitLength = reader.ReadInt32();
            int compLength = reader.ReadInt32();
            byte[] compData = reader.ReadBytes(compLength);

            if (expectedName != null && name != expectedName)
            {
                return;
            }
            var tree = new ShannonFanoTree();
            byte[] data = tree.Decode(compData, bitLength, freq, token);
            string outPath = Path.Combine(outputDirectory, name);
            string dir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(outPath, data);
        }
    }
}
