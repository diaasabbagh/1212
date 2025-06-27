using System;

namespace SmartArchiver.Compression
{
    internal class HuffmanNode : IComparable<HuffmanNode>
    {
        public byte? Symbol { get; set; }
        public int Frequency { get; set; }
        public HuffmanNode Left { get; set; }
        public HuffmanNode Right { get; set; }

        public int CompareTo(HuffmanNode other)
        {
            if (other == null) return 1;
            int freqCompare = Frequency.CompareTo(other.Frequency);
            if (freqCompare == 0)
                return (Symbol ?? 0).CompareTo(other.Symbol ?? 0);
            return freqCompare;
        }

        public bool IsLeaf => Left == null && Right == null;
    }
}
