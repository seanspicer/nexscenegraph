
using Veldrid.SceneGraph.InputAdapter;

namespace SceneInScene.Wpf
{
    public class ViewMatrixEventHandler : InputEventHandler
    {
        private readonly SceneInSceneViewModel _viewModel;
        
        public ViewMatrixEventHandler(SceneInSceneViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void HandleInput(IInputStateSnapshot snapshot, IUiActionAdapter uiActionAdapter)
        {
            _viewModel.MainViewMatrix = snapshot.ViewMatrix;
        }
    }
}