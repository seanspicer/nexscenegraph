

using System.Numerics;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;

namespace Gnomon
{
    public class ViewMatrixEventHandler : InputEventHandler
    {
        
        private readonly SceneViewModel _viewModel;
        
        public ViewMatrixEventHandler(SceneViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void HandleInput(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            base.HandleInput(snapshot, uiActionAdapter);
            _viewModel.MainViewMatrix = snapshot.ViewMatrix;
        }
    }
}