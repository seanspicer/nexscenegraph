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
using Veldrid.SceneGraph.Viewer;

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
        View View { get; set; }
        Matrix4x4 ProjectionMatrix { get; }
        Matrix4x4 ViewMatrix { get;  }
        
        ProjectionResizePolicy ProjectionResizePolicy { get; }

        void SetProjectionResizePolicy(ProjectionResizePolicy policy);
        
        IViewport Viewport { get; }
        
        void SetViewport(int x, int y, int width, int height);
        
        IGraphicsDeviceOperation Renderer { get; set; }

        void HandleResizeEvent(IResizedEvent resizedEvent);

        void SetProjectionMatrix(Matrix4x4 matrix);

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
        
        bool GetProjectionMatrixAsOrtho( 
            ref float left, ref float right, 
            ref float bottom, ref float top,
            ref float zNear, ref float zFar);
        
        void SetViewMatrix(Matrix4x4 matrix);
        
        void SetViewMatrixToLookAt(Vector3 position, Vector3 target, Vector3 upDirection);
        

        Vector3 NormalizedScreenToWorld(Vector3 screenCoords);
        
        RgbaFloat ClearColor { get; }

        void SetClearColor(RgbaFloat color);
    }
}