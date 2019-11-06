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
using System.Data;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    internal class Viewport : IViewport
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }
    
    public class Camera : Transform, ICamera
    {
        public View View { get; set; }
        
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }

        // Perspective info
        private float _fov = 1f;
        private float _near = 0.1f;
        private float _far = 10000.0f;
        private float _aspectRatio = 1;
        
        private float _yaw = 0.45f;
        private float _pitch = -0.55f;
        
        // View info
        private Vector3 _position = new Vector3(0, 0, 5.0f);
        private Vector3 _target = new Vector3(0, 0, 0);
        private Vector3 _lookDirection = Vector3.UnitZ;
        private Vector3 _upDirection = Vector3.UnitY;

        public IViewport Viewport => new Viewport()
        {
            Height = (int)DisplaySettings.Instance.ScreenHeight,
            Width = (int)DisplaySettings.Instance.ScreenWidth
        };
        
        public Vector3 Up => _upDirection;

        public Vector3 Look => _lookDirection;

        public Vector3 Position => _position;

        public float Near => _near;

        public float Far => _far;

        public float AspectRatio => _aspectRatio;

        public float Fov => _fov;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }
        
        public IGraphicsDeviceOperation Renderer { get; set; }

        private float _width { get; set; }
        private float _height { get; set; }
        
        public static ICamera Create(float width, float height)
        {
            return new Camera(width, height);
        }
        
        protected Camera(float width, float height)
        {
            _width = width;
            _height = height;
            
            ClearColor = RgbaFloat.Grey;
            
            _aspectRatio = width / height;
            ProjectionMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
            UpdateProjectionMatrix();
            UpdateViewMatrix();

        }

        public void HandleResizeEvent(IResizedEvent resizedEvent)
        {
            _aspectRatio = (float)resizedEvent.Width / (float)resizedEvent.Height;
            
            UpdateProjectionMatrix();
        }

        /// <summary>
        /// Create a symmetrical perspective projection. 
        /// </summary>
        /// <param name="vfov"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="zNear"></param>
        /// <param name="zFar"></param>
        public void SetProjectionMatrixAsPerspective(float vfov, float aspectRatio, float zNear, float zFar)
        {
            _fov = vfov;
            _near = zNear;
            _far = zFar;
            _aspectRatio = aspectRatio;
            
            UpdateProjectionMatrix();
        }

        public void SetViewMatrixToLookAt(Vector3 position, Vector3 target, Vector3 upDirection)
        {
            _position = position;
            _target = target;
            _upDirection = upDirection;
            
            UpdateViewMatrix();
        }

        public Vector3 NormalizedScreenToWorld(Vector3 screenCoords)
        {
            var viewProjectionMatrix = Matrix4x4.Identity;

            viewProjectionMatrix = ProjectionMatrix.PreMultiply(ViewMatrix);

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

        public RgbaFloat ClearColor { get; set; }

        private Vector3 GetLookDir()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            return lookDir;
        }

        private void UpdateViewMatrix()
        {
            ViewMatrix = Matrix4x4.CreateLookAt(_position, _target, _upDirection);
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, _near, _far);
        }
    }
}