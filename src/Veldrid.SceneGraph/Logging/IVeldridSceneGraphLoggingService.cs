namespace Veldrid.SceneGraph.Logging
{
    public interface IVeldridSceneGraphLoggingService
    {
        IVeldridSceneGraphLogger GetLogger();
        void RegisterLogger(IVeldridSceneGraphLogger logger);
    }
}