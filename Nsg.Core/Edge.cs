namespace Nsg.Core
{
    public class Edge
    {
        public Node Parent { get; private set; }
        public Node Child { get; private set; }

        public Edge(Node parent, Node child)
        {
            Parent = parent;
            Child = child;
        }
    }
}