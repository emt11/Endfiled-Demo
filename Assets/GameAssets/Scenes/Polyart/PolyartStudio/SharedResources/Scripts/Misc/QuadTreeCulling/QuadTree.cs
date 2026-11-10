namespace Polyart
{
    public struct QuadTree
    {
        private static readonly int[] treeLength = { 1, 5, 21, 85, 341, 1365, 5461, 21845 };
        private static readonly int[] siblingsCount = { 1, 4, 16, 64, 256, 1024, 4096, 16384 };
        public const int maxDepth = 8;

        public static int GetTreeLengthAtDepthIndex(int depthIndex)
        {
            return treeLength[depthIndex];
        }

        public static int GetSiblingCountAtDepthIndex(int depthIndex)
        {
            return siblingsCount[depthIndex];
        }

        public static int GetFirstIndexAtDepthIndex(int depthIndex)
        {
            if (depthIndex == 0) return 0;
            else return GetTreeLengthAtDepthIndex(depthIndex - 1);
        }

        public static int GetDepthIndex(int nodeIndex)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (nodeIndex < GetTreeLengthAtDepthIndex(i))
                    return i;
            }
            return -1;
        }

        public static int GetFirstChildIndexOf(int nodeIndex)
        {
            int depthIndex = GetDepthIndex(nodeIndex);
            int firstIndexAtDepthIndex = GetFirstIndexAtDepthIndex(depthIndex);
            int offset = nodeIndex - firstIndexAtDepthIndex;
            int firstIndexAtNextDepthIndex = GetFirstIndexAtDepthIndex(depthIndex + 1);

            return firstIndexAtNextDepthIndex + offset * 4;
        }
    }
}