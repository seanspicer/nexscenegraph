
using System.Collections.Generic;
using SharpDX.Direct3D11;

namespace Veldrid.SceneGraph
{
    public interface IGraphicsContext : IObject
    {
        public interface ITraits {}

        void AddCamera(ICamera camera);

        void RemoveCamera(ICamera camera);
    }

    public abstract class GraphicsContext : Object, IGraphicsContext
    {
        public class CameraList : List<ICamera> {}

        protected CameraList Cameras { get; set; } = new CameraList();

        protected abstract IGraphicsContext CreateGraphicsContext(IGraphicsContext.ITraits traits);

        public void AddCamera(ICamera camera)
        {
            Cameras.Add(camera);
        }

        public void RemoveCamera(ICamera camera)
        {
            // TODO -- probably need to iterate the camera's children and release graphics resources
            Cameras.Remove(camera);
        }
    }

}