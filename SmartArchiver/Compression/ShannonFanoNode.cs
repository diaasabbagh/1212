using System;

namespace SmartArchiver.Compression
{
    internal class ShannonFanoNode
    {
        public byte? Symbol { get; set; }
        public int Frequency { get; set; }
        public ShannonFanoNode Left { get; set; }
        public ShannonFanoNode Right { get; set; }

        public bool IsLeaf => Left == null && Right == null;
    }
}
