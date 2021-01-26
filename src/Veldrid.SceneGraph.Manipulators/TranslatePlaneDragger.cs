

using System.Drawing;
using System.Numerics;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITranslatePlaneDragger : ICompositeDragger
    {
        public Translate1DDragger Translate1DDragger { get; }
        public Translate2DDragger Translate2DDragger { get; }
        void SetColor(Color color);
    }
    
    public class TranslatePlaneDragger : CompositeDragger, ITranslatePlaneDragger
    {
        public Translate1DDragger Translate1DDragger { get; protected set; }
        public Translate2DDragger Translate2DDragger { get; protected set; }
        protected bool UsingTranslate1DDragger { get; set; }

        public new static ITranslatePlaneDragger Create()
        {
            return new TranslatePlaneDragger(Matrix4x4.Identity);
        }
        
        protected TranslatePlaneDragger(Matrix4x4 matrix) : base(matrix)
        {
        }

        public override void SetupDefaultGeometry()
        {
            throw new System.NotImplementedException();
        }

        public void SetColor(Color color)
        {
            
        }
    }
}