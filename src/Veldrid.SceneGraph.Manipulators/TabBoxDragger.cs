using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.Manipulators
{
    public interface ITabBoxDragger : ICompositeDragger
    {
    }

    public class TabBoxDragger : CompositeDragger, ITabBoxDragger
    {
        protected TabBoxDragger(Matrix4x4 matrix) : base(matrix)
        {
            for (var i = 0; i < 6; ++i)
            {
                var planeDragger = TabPlaneDragger.Create();
                PlaneDraggers.Add(planeDragger);
                AddChild(planeDragger);
                DraggerList.Add(planeDragger);
            }

            {
                var dragger = PlaneDraggers.ElementAt(0);
                dragger.NameString = "Front Tab Plane Dragger";
                dragger.Matrix = Matrix4x4.CreateTranslation(0.0f, 0.5f, 0.0f);
            }
            {
                var quat = QuaternionExtensions.MakeRotate(-1 * Vector3.UnitY, Vector3.UnitY);
                var dragger = PlaneDraggers.ElementAt(1);
                dragger.NameString = "Back Tab Plane Dragger";
                dragger.Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.0f, -0.5f, 0.0f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitZ, Vector3.UnitY);
                var dragger = PlaneDraggers.ElementAt(2);
                dragger.NameString = "Bottom Tab Plane Dragger";
                dragger.Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.0f, 0.0f, -0.5f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitY, Vector3.UnitZ);
                var dragger = PlaneDraggers.ElementAt(3);
                dragger.NameString = "Top Tab Plane Dragger";
                dragger.Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.5f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitX, Vector3.UnitY);
                var dragger = PlaneDraggers.ElementAt(4);
                dragger.NameString = "LEft Tab Plane Dragger";
                dragger.Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(-0.5f, 0.0f, 0.0f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitY, Vector3.UnitX);
                var dragger = PlaneDraggers.ElementAt(5);
                dragger.NameString = "Right Tab Plane Dragger";
                dragger.Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.5f, 0.0f, 0.0f));
            }

            foreach (var dragger in DraggerList) dragger.ParentDragger = this;
        }

        protected List<ITabPlaneDragger> PlaneDraggers { get; set; } = new List<ITabPlaneDragger>();

        public override void SetupDefaultGeometry()
        {
            foreach (var dragger in PlaneDraggers) dragger.SetupDefaultGeometry();
        }

        public new static ITabBoxDragger Create()
        {
            return new TabBoxDragger(Matrix4x4.Identity);
        }
    }
}