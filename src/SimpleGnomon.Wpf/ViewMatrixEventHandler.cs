
using Veldrid.SceneGraph.InputAdapter;

namespace SimpleGnomon.Wpf
{
    public class ViewMatrixEventHandler : InputEventHandler
    {
        private readonly GnomonSceneViewModel _viewModel;
        
        public ViewMatrixEventHandler(GnomonSceneViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void HandleInput(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            _viewModel.MainViewMatrix = snapshot.ViewMatrix;
        }
    }
}