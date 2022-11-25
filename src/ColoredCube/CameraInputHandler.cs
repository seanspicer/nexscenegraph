using Veldrid.SceneGraph.InputAdapter;
using Veldrid.SceneGraph.Viewer;

namespace ColoredCube;

public class CameraInputHandler : UiEventHandler
{
    private readonly IViewer _viewer;
    private bool _isOrthoGraphic = true;

    public CameraInputHandler(IViewer viewer)
    {
        _viewer = viewer;
    }

    public override bool Handle(IUiEventAdapter eventAdapter, IUiActionAdapter actionAdapter)
    {
        switch (eventAdapter.Key)
        {
            case IUiEventAdapter.KeySymbol.KeyO:
                if (!_isOrthoGraphic)
                {
                    _viewer.SetCameraOrthographic();
                    _isOrthoGraphic = true;
                }
                return true;
            case IUiEventAdapter.KeySymbol.KeyP:
                if (_isOrthoGraphic)
                {
                    _viewer.SetCameraPerspective();
                    _isOrthoGraphic = false;
                }
                return true;
            default:
                return false;
        }
    }
}