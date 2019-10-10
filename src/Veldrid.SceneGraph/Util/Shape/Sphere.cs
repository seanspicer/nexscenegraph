namespace Veldrid.SceneGraph.Util.Shape
{
    public interface ISphere : IShape
    {
        
    }
    
    public class Sphere : ISphere
    {
        public void Accept(IShapeVisitor shapeVisitor)
        {
            throw new System.NotImplementedException();
        }
    }
}