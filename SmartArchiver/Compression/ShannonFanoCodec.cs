using System.Collections.Generic;
using System.IO;

namespace SmartArchiver.Compression
{
    internal static class ShannonFanoCodec
    {
        public static void CompressFile(string inputPath, BinaryWriter writer, System.Threading.CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            byte[] data = File.ReadAllBytes(inputPath);
            var tree = new ShannonFanoTree();
            var freq = tree.Build(data);
            var compressed = tree.Encode(data, out int bitLength);

            writer.Write(Path.GetFileName(inputPath));
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
            byte[] data = tree.Decode(compData, bitLength, freq);
            string outPath = Path.Combine(outputDirectory, name);
            File.WriteAllBytes(outPath, data);
        }
    }
}
