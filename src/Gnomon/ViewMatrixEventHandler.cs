

using System.Numerics;
using Examples.Common.Wpf;
using Veldrid;
using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace Gnomon
{
    public class ViewMatrixEventHandler : FrameCaptureEventHandler
    {
        
        private readonly SceneViewModel _viewModel;
        
        public ViewMatrixEventHandler(SceneViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter uiActionAdapter)
        {
            if (base.Handle(eventAdapter, uiActionAdapter))
            {
                return true;
            }
            
            if(uiActionAdapter is IView view)
            {
                _viewModel.MainViewMatrix = view.Camera.ViewMatrix;
                return true;
            }

            return false;
        }
    }
}