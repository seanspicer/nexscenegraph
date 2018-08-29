//
// Copyright (c) 2018 Sean Spicer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;

namespace Veldrid.SceneGraph.Viewer
{
    public class Renderer : IGraphicsDeviceOperation
    {
        private DrawVisitor _drawVisitor;
        private Camera _camera;
        
        private DeviceBuffer _projectionBuffer;
        private DeviceBuffer _viewBuffer;
        private DeviceBuffer _worldBuffer;
        private CommandList _commandList;
        
        private bool _initialized = false;
        
        public Renderer(Camera camera)
        {
            _camera = camera;
            _drawVisitor = new DrawVisitor();
        }

        private void Initialize(GraphicsDevice device)
        {
            var factory = device.ResourceFactory;
            _projectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _viewBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _worldBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            
            _commandList = factory.CreateCommandList();

            _initialized = true;
        }
        
        private void Draw(GraphicsDevice device)
        {
            if (!_initialized)
            {
                Initialize(device);
            }
            
            _drawVisitor.GraphicsDevice = device;
            _drawVisitor.CommandList = _commandList;
            
            // Begin() must be called before commands can be issued.
            _commandList.Begin();

            // We want to render directly to the output window.
            _commandList.SetFramebuffer(device.SwapchainFramebuffer);
            
            // TODO Set from Camera color ?
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            
            _drawVisitor.BeginDraw();

            if (_camera.View.GetType() != typeof(Viewer.View))
            {
                throw new InvalidCastException("Camera View type is not correct");
            }

            var view = (Viewer.View) _camera.View;
            view.SceneData?.Accept(_drawVisitor);

            _drawVisitor.EndDraw();
            
            _commandList.End();
            
            device.SubmitCommands(_commandList);
        }

        private void UpdateUniforms(GraphicsDevice device)
        {
            if (!_initialized)
            {
                Initialize(device);
            }
            
            device.UpdateBuffer(_projectionBuffer, 0, _camera.ProjectionMatrix);
            device.UpdateBuffer(_viewBuffer, 0, _camera.ViewMatrix);
            
        }

        private void SwapBuffers(GraphicsDevice device)
        {
            device.SwapBuffers();
        }

        public void HandleOperation(object sender, GraphicsDevice device)
        {
            UpdateUniforms(device);
            Draw(device);
            SwapBuffers(device);
        }
        
        #region IDisposable
        //
        // IDisposable Pattern
        //
        private void ReleaseUnmanagedResources()
        {
            _drawVisitor.DisposeResources();
            _commandList.Dispose();
            _projectionBuffer.Dispose();
            _viewBuffer.Dispose();
            _worldBuffer.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Renderer()
        {
            Dispose(false);
        }
        #endregion

    }
}