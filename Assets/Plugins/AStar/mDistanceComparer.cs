using System.Collections.Generic;

namespace Client
{
    public struct mDistanceComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            return x.mDistance.CompareTo(y.mDistance);
        }
    }
}
