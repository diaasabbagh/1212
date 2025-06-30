using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartArchiver.Compression
{
    internal class ShannonFanoTree
    {
        private ShannonFanoNode _root;
        private Dictionary<byte, string> _codes;

        public Dictionary<byte, int> Build(byte[] data)
        {
            var freq = new Dictionary<byte, int>();
            foreach (byte b in data)
            {
                if (!freq.ContainsKey(b)) freq[b] = 0;
                freq[b]++;
            }
            var nodes = freq
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new ShannonFanoNode { Symbol = kv.Key, Frequency = kv.Value })
                .ToList();
            _root = BuildTree(nodes);
            _codes = new Dictionary<byte, string>();
            BuildCodes(_root, "");
            return freq;
        }

        private ShannonFanoNode BuildTree(List<ShannonFanoNode> nodes)
        {
            if (nodes.Count == 1) return nodes[0];
            int total = nodes.Sum(n => n.Frequency);
            int running = 0;
            int split = 0;
            int bestDiff = int.MaxValue;
            for (int i = 0; i < nodes.Count; i++)
            {
                running += nodes[i].Frequency;
                int diff = Math.Abs(total - 2 * running);
                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    split = i + 1;
                }
            }
            if (split <= 0) split = 1;
            var leftList = nodes.Take(split).ToList();
            var rightList = nodes.Skip(split).ToList();
            return new ShannonFanoNode
            {
                Left = BuildTree(leftList),
                Right = BuildTree(rightList),
                Frequency = total
            };
        }

        private void BuildCodes(ShannonFanoNode node, string prefix)
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
                var nodes = freq
                    .OrderByDescending(kv => kv.Value)
                    .Select(kv => new ShannonFanoNode { Symbol = kv.Key, Frequency = kv.Value })
                    .ToList();
                _root = BuildTree(nodes);
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
            ShannonFanoNode current = _root;
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
    }
}
