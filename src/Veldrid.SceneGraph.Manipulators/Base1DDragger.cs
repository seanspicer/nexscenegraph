
using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util.Shape;
using Veldrid.SceneGraph.VertexTypes;

namespace Veldrid.SceneGraph.Manipulators
{
    public abstract class Base1DDragger : Dragger
    {
        protected ILineProjector LineProjector { get; set; }

        protected Base1DDragger(Matrix4x4 matrix) : this(new Vector3(-0.5f, 0.0f, 0.0f), new Vector3(0.5f, 0.0f, 0.0f), matrix)
        {
        }
        
        protected Base1DDragger(Vector3 s, Vector3 e, Matrix4x4 matrix) : base(matrix) 
        {
            LineProjector = Veldrid.SceneGraph.Manipulators.LineProjector.Create(
                LineSegment.Create(s,e));
            
            Color = Color.Green;
            PickColor = Color.Magenta;
        }
    }
}