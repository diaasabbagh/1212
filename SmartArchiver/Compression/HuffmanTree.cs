using System;
using System.Linq;
using System.Collections.Generic;

namespace SmartArchiver.Compression
{
    internal class HuffmanTree
    {
        private HuffmanNode _root;
        private Dictionary<byte, string> _codes;

        public Dictionary<byte, int> Build(byte[] data)
        {
            var freq = new Dictionary<byte, int>();
            foreach (byte b in data)
            {
                if (!freq.ContainsKey(b)) freq[b] = 0;
                freq[b]++;
            }
            var comparer = Comparer<HuffmanNode>.Create((a, b) => a.CompareTo(b));
            var pq = new SortedSet<HuffmanNode>(comparer);
            foreach (var kv in freq)
            {
                pq.Add(new HuffmanNode { Symbol = kv.Key, Frequency = kv.Value });
            }
            while (pq.Count > 1)
            {
                var left = pq.Min; pq.Remove(left);
                var right = pq.Min; pq.Remove(right);
                var parent = new HuffmanNode
                {
                    Symbol = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };
                pq.Add(parent);
            }
            _root = pq.Min;
            _codes = new Dictionary<byte, string>();
            BuildCodes(_root, "");
            return freq;
        }

        private void BuildCodes(HuffmanNode node, string prefix)
        {
            if (node == null) return;
            if (node.IsLeaf && node.Symbol.HasValue)
            {
                _codes[node.Symbol.Value] = prefix.Length > 0 ? prefix : "0";
                return;
            }
            BuildCodes(node.Left, prefix + "0");
            BuildCodes(node.Right, prefix + "1");
        }

        public Dictionary<byte, string> Codes => _codes;

        public byte[] Encode(byte[] data, out int bitLength, System.Threading.CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var bits = new List<bool>();
            foreach (byte b in data)
            {
                token.ThrowIfCancellationRequested();
                string code = _codes[b];
                foreach (char c in code)
                {
                    token.ThrowIfCancellationRequested();
                    bits.Add(c == '1');
                }
            }
            token.ThrowIfCancellationRequested();
            bitLength = bits.Count;
            int padding = (8 - bits.Count % 8) % 8;
            for (int i = 0; i < padding; i++)
            {
                token.ThrowIfCancellationRequested();
                bits.Add(false);
            }
            byte[] bytes = new byte[bits.Count / 8];
            for (int i = 0; i < bytes.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                byte val = 0;
                for (int j = 0; j < 8; j++)
                {
                    token.ThrowIfCancellationRequested();
                    if (bits[i * 8 + j]) val |= (byte)(1 << (7 - j));
                }
                bytes[i] = val;
            }
            return bytes;
        }

        public byte[] Decode(byte[] data, int bitLength, Dictionary<byte, int> freq, System.Threading.CancellationToken token)
        {
            if (_root == null)
            {
                BuildTreeFromFreq(freq);
            }
            var bits = new List<bool>();
            for (int i = 0; i < data.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                for (int j = 0; j < 8; j++)
                {
                    bool bit = (data[i] & (1 << (7 - j))) != 0;
                    bits.Add(bit);
                }
            }
            if (bits.Count > bitLength)
                bits.RemoveRange(bitLength, bits.Count - bitLength);
            var result = new List<byte>();
            HuffmanNode current = _root;
            foreach (bool bit in bits)
            {
                token.ThrowIfCancellationRequested();
                current = bit ? current.Right : current.Left;
                if (current.IsLeaf)
                {
                    result.Add(current.Symbol.Value);
                    current = _root;
                    if (result.Count == freq.Values.Sum()) break;
                }
            }
            return result.ToArray();
        }

        private void BuildTreeFromFreq(Dictionary<byte, int> freq)
        {
            var comparer = Comparer<HuffmanNode>.Create((a, b) => a.CompareTo(b));
            var pq = new SortedSet<HuffmanNode>(comparer);
            foreach (var kv in freq)
            {
                pq.Add(new HuffmanNode { Symbol = kv.Key, Frequency = kv.Value });
            }
            while (pq.Count > 1)
            {
                var left = pq.Min; pq.Remove(left);
                var right = pq.Min; pq.Remove(right);
                var parent = new HuffmanNode
                {
                    Symbol = null,
                    Frequency = left.Frequency + right.Frequency,
                    Left = left,
                    Right = right
                };
                pq.Add(parent);
            }
            _root = pq.Min;
        }
    }
}
