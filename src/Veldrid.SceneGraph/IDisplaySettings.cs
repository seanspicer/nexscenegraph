namespace Veldrid.SceneGraph
{
    public interface IDisplaySettings
    {
        float ScreenWidth { get; set; }
        float ScreenHeight { get; set; }
        float ScreenDistance { get; set; }
        GraphicsBackend GraphicsBackend { get; }
    }
}