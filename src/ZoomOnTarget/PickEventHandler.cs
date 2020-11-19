using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using System.Windows.Input;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;
using InputEventHandler = Veldrid.SceneGraph.InputAdapter.InputEventHandler;
using MouseButton = Veldrid.MouseButton;

namespace ZoomOnTarget
{
    public class PickEventHandler : InputEventHandler
    {
        public override void HandleInput(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            base.HandleInput(snapshot, uiActionAdapter);
            
            if (snapshot.MouseEvents.Any(x => x.Down && x.MouseButton == MouseButton.Left) &&
                Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                DoPick(snapshot, uiActionAdapter as IView);
            }
        }

        private void DoPick(IInputStateSnapshot snapshot, IView view)
        {
            var norm = GetNormalizedMousePosition();
            
            var startPos = view.Camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 0.0f)); // Near plane
            var endPos = view.Camera.NormalizedScreenToWorld(new Vector3(norm.X, norm.Y, 1.0f)); // Far plane
            var intersector = LineSegmentIntersector.Create(startPos, endPos);
            
            var intersectionVisitor = IntersectionVisitor.Create(intersector);
            
            view.SceneData?.Accept(intersectionVisitor);

            if (intersector.Intersections.Any())
            {
                foreach (var intersection in intersector.Intersections)
                {
                    var node = intersection.NodePath.FirstOrDefault();
                    if (node != null)
                    {
                        var point = node.GetBound().Center;
                        view.CameraManipulator.ZoomOnTarget(snapshot);
                    }
                }
                
            }
        }
    }
}