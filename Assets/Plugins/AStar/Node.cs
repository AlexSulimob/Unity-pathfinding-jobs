using Unity.Mathematics;

namespace Client
{
    public struct Node 
    {
        public int index;
        public int2 pos;
        public int mDistance;
        public Node(int index, int2 pos)
        {
            this.index = index;
            this.pos = pos;
            mDistance = int.MaxValue;
        }

    }
}
