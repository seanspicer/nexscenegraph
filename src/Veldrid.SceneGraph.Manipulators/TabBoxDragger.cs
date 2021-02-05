

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
        protected List<ITabPlaneDragger> PlaneDraggers { get; set; } = new List<ITabPlaneDragger>();
        
        public new static ITabBoxDragger Create()
        {
            return new TabBoxDragger(Matrix4x4.Identity);
        }
        
        protected TabBoxDragger(Matrix4x4 matrix) : base(matrix)
        {
            for (var i = 0; i < 6; ++i)
            {
                var planeDragger = TabPlaneDragger.Create();
                PlaneDraggers.Add(planeDragger);
                AddChild(planeDragger);
            }

            PlaneDraggers.ElementAt(0).Matrix = Matrix4x4.CreateTranslation(0.0f, 0.5f, 0.0f);

            {
                var quat = QuaternionExtensions.MakeRotate(-1 * Vector3.UnitY, Vector3.UnitY);
                PlaneDraggers.ElementAt(1).Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.0f, -0.5f, 0.0f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitZ, Vector3.UnitY);
                PlaneDraggers.ElementAt(2).Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.0f, 0.0f, -0.5f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitY, Vector3.UnitZ);
                PlaneDraggers.ElementAt(3).Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.0f, 0.0f, 0.5f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitX, Vector3.UnitY);
                PlaneDraggers.ElementAt(4).Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(-0.5f, 0.0f, 0.0f));
            }
            {
                var quat = QuaternionExtensions.MakeRotate(Vector3.UnitY, Vector3.UnitX);
                PlaneDraggers.ElementAt(5).Matrix = Matrix4x4.CreateFromQuaternion(quat)
                    .PostMultiply(Matrix4x4.CreateTranslation(0.5f, 0.0f, 0.0f));
            }
        }

        public override void SetupDefaultGeometry()
        {
            foreach (var dragger in PlaneDraggers)
            {
                dragger.SetupDefaultGeometry();
            }
        }
    }
}