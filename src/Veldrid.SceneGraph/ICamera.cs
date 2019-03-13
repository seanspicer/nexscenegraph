//
// Copyright 2018 Sean Spicer 
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
using System.Numerics;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    public interface ICamera : ITransform
    {
        View View { get; set; }
        Matrix4x4 ProjectionMatrix { get; set; }
        Matrix4x4 ViewMatrix { get; set; }
        
        Vector3 Up { get; }
        Vector3 Look { get; }
        Vector3 Position { get; }
        
        float AspectRatio { get; }
        float Fov { get; }
        
        float Near { get; }
        float Far { get; }
        
        float Yaw { get; set; }
        float Pitch { get; set; }
        IGraphicsDeviceOperation Renderer { get; set; }

        void HandleResizeEvent(IResizedEvent resizedEvent);
        
        void SetViewMatrixToLookAt(Vector3 position, Vector3 target, Vector3 upDirection);
        
        /// <summary>
        /// Create a symmetrical perspective projection. 
        /// </summary>
        /// <param name="vfov"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        void SetProjectionMatrixAsPerspective(float vfov, float aspectRatio, float zNear, float zFar);

        Vector3 NormalizedScreenToWorld(Vector3 screenCoords);
    }
}