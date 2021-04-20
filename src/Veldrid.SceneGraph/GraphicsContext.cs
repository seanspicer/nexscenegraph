using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public interface IGraphicsContext : IObject
    {
        void AddCamera(ICamera camera);

        void RemoveCamera(ICamera camera);

        public interface ITraits
        {
        }
    }

    public abstract class GraphicsContext : Object, IGraphicsContext
    {
        protected CameraList Cameras { get; set; } = new CameraList();

        public void AddCamera(ICamera camera)
        {
            Cameras.Add(camera);
        }

        public void RemoveCamera(ICamera camera)
        {
            // TODO -- probably need to iterate the camera's children and release graphics resources
            Cameras.Remove(camera);
        }

        protected abstract IGraphicsContext CreateGraphicsContext(IGraphicsContext.ITraits traits);

        public class CameraList : List<ICamera>
        {
        }
    }
}