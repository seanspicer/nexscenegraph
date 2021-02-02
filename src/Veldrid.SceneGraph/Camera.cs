//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Numerics;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;
using Vulkan;
using Math = System.Math;

namespace Veldrid.SceneGraph
{
    
    public enum ProjectionResizePolicy
    {
        Horizontal, 
        Vertical, 
        Fixed
    }

    [Flags]
    public enum ResizeMask
    {
        ResizeViewport = 1,
        ResizeAttachments = 2,
        ResizeProjectionMatrix = 4,
        ResizeDefault = ResizeViewport | ResizeAttachments
    }
    
    public interface ICamera : ITransform
    {
        IView View { get; }

        void SetView(IView view);
        
        Matrix4x4 ProjectionMatrix { get; }
        Matrix4x4 ViewMatrix { get;  }
        
        ProjectionResizePolicy ProjectionResizePolicy { get; }

        void SetProjectionResizePolicy(ProjectionResizePolicy policy);
        
        IViewport Viewport { get; }
        
        void SetViewport(int x, int y, int width, int height);
        
        void SetViewport(IViewport viewport);
        
        IGraphicsDeviceOperation Renderer { get; }
        
        void SetRenderer(IGraphicsDeviceOperation renderer);

        void Resize(int width, int height, ResizeMask resizeMask = ResizeMask.ResizeDefault);

        void SetProjectionMatrix(Matrix4x4 matrix);
        
        void SetViewMatrix(Matrix4x4 matrix);
        
        void SetViewMatrixToLookAt(Vector3 position, Vector3 target, Vector3 upDirection);
        
        Vector3 NormalizedScreenToWorld(Vector3 screenCoords);
        
        RgbaFloat ClearColor { get; }

        void SetClearColor(RgbaFloat color);
        
        IGraphicsContext GraphicsContext { get; set; }

    }

    public interface IPerspectiveCamera : ICamera
    {
        float VerticalFov { get; }

        /// <summary>
        /// Create a symmetrical perspective projection. 
        /// </summary>
        /// <param name="vfov"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        void SetProjectionMatrixAsPerspective(float vfov, float aspectRatio, float zNear, float zFar);
        
        bool GetProjectionMatrixAsFrustum( 
            ref float left, ref float right, 
            ref float bottom, ref float top,
            ref float zNear, ref float zFar);
    }

    public interface IOrthographicCamera : ICamera
    {
        void SetProjectionMatrixAsOrthographicOffCenter(
            float left,
            float right,
            float bottom,
            float top,
            float zNear,
            float zFar);

        void SetProjectionMatrixAsOrthographic(
            float width,
            float height,
            float zNearPlane,
            float zFarPlane);
        
        bool GetProjectionMatrixAsOrtho( 
            ref float left, ref float right, 
            ref float bottom, ref float top,
            ref float zNear, ref float zFar);
    }
    
    public abstract class Camera : Transform, ICamera
    {
        protected uint Width { get; set; }
        protected uint Height { get; set; }
        protected float Distance { get; set; }

        private IGraphicsContext _graphicsContext;

        public IGraphicsContext GraphicsContext
        {
            get => _graphicsContext;
            set
            {
                if (value == GraphicsContext) return;

                if (null != _graphicsContext)
                {
                    _graphicsContext.RemoveCamera(this);
                }

                _graphicsContext = value;

                if (null != _graphicsContext)
                {
                    _graphicsContext.AddCamera(this);
                }
            }
        }
        
        
        public IView View { get; private set; }
        
        public void SetView(IView view)
        {
            View = view;
        }

        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
        
        public ProjectionResizePolicy ProjectionResizePolicy { get; private set; }

        public void SetProjectionResizePolicy(ProjectionResizePolicy policy)
        {
            ProjectionResizePolicy = policy;
        }

        public IViewport Viewport { get; private set; }

        public void SetViewport(int x, int y, int width, int height)
        {
            Viewport = SceneGraph.Viewport.Create(x, y, width, height);
        }
        
        public void SetViewport(IViewport viewport)
        {
            Viewport = viewport;
        }

        public IGraphicsDeviceOperation Renderer { get; private set; }
        
        protected Camera(uint width, uint height, float distance)
        {
            Width = width;
            Height = height;
            Distance = distance;
            
            ClearColor = RgbaFloat.Grey;
            Viewport = null;
            ProjectionMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
            ProjectionResizePolicy = ProjectionResizePolicy.Fixed;
        }


        public void SetRenderer(IGraphicsDeviceOperation renderer)
        {
            Renderer = renderer;
        }

//        public void HandleResizeEvent(IResizedEvent resizedEvent)
//        {
//            Resize(resizedEvent.Width, resizedEvent.Height);
//        }

        protected abstract void ResizeProjection(int width, int height, ResizeMask resizeMask = ResizeMask.ResizeDefault);
        
        public void Resize(int width, int height, ResizeMask resizeMask = ResizeMask.ResizeDefault)
        {
            if (null != Viewport)
            {
                ResizeProjection(width, height, resizeMask);
            }

            if ((resizeMask & ResizeMask.ResizeAttachments) != 0)
            {
                // TODO: resize attached framebuffer.
            }
        }

        public void SetProjectionMatrix(Matrix4x4 matrix)
        {
            ProjectionMatrix = matrix;
        }
        
        // public bool IsOrthographicCamera()
        // {
        //     float left = 0, right = 0, bottom = 0, top = 0, zNear = 0, zFar = 0;
        //     return GetProjectionMatrixAsOrtho(
        //         ref left, ref right,
        //         ref bottom, ref top,
        //         ref zNear, ref zFar);
        //
        // }

        public void SetViewMatrix(Matrix4x4 matrix)
        {
            ViewMatrix = matrix;
        }
        
        public void SetViewMatrixToLookAt(Vector3 position, Vector3 target, Vector3 upDirection)
        {
            SetViewMatrix(Matrix4x4.CreateLookAt(position, target, upDirection));
        }

        public Vector3 NormalizedScreenToWorld(Vector3 screenCoords)
        {
            var viewProjectionMatrix = ProjectionMatrix.PreMultiply(ViewMatrix);

            Matrix4x4 vpi;
            
            if (Matrix4x4.Invert(viewProjectionMatrix, out vpi))
            {
                var nc = new Vector3(screenCoords.X, screenCoords.Y, screenCoords.Z);
                var pc = vpi.PreMultiply(nc);

                return pc;
                
            }
            else
            {
                throw new Exception("Cannot invert view-projection matrix");
            }
            
        }

        public RgbaFloat ClearColor { get; private set; }

        public void SetClearColor(RgbaFloat color)
        {
            ClearColor = color;
        }
    }
}