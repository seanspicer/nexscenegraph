using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.InputAdapter;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ICompositeDragger : IDragger
    {
    }

    public class CompositeDragger : Dragger, ICompositeDragger
    {
        protected CompositeDragger(Matrix4x4 matrix) : base(matrix)
        {
        }

        protected List<IDragger> DraggerList { get; set; } = new List<IDragger>();

        public override IDragger ParentDragger
        {
            get => base.ParentDragger;
            set
            {
                foreach (var dragger in DraggerList) dragger.ParentDragger = value;

                base.ParentDragger = value;
            }
        }

        public override uint IntersectionNodeMask
        {
            get => base.IntersectionNodeMask;
            set
            {
                foreach (var dragger in DraggerList) dragger.IntersectionNodeMask = value;

                base.IntersectionNodeMask = value;
            }
        }

        public override bool Handle(IPointerInfo pointerInfo, IUiEventAdapter eventAdapter,
            IUiActionAdapter actionAdapter)
        {
            if (!pointerInfo.Contains(this)) return false;

            return DraggerList.Any(dragger => dragger.Handle(pointerInfo, eventAdapter, actionAdapter));
        }
    }
}