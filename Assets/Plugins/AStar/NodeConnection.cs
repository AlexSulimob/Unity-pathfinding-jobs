
namespace Client
{
    public struct NodeConnection 
    {
        public Node node;
        public int cost;
        public NodeConnection(Node node, int cost = 1)
        {
            this.node = node;
            this.cost = cost;
        }
    }
}

