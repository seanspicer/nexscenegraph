namespace Veldrid.SceneGraph
{
    public interface IGroup : INode
    {
        bool AddChild(Node child);
        bool InsertChild(int index, Node child);
        bool RemoveChild(Node child);
        bool RemoveChildren(int pos, int numChildrenToRemove);
        void ChildInserted(int index);
        void ChildRemoved(int index, int count);
    }
}