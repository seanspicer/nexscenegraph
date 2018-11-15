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
using System.Data;
using System.Numerics;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    public class Camera : Transform
    {
        public View View { get; set; }
        
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }

        // Perspective info
        private float _fov = 1f;
        private float _near = 0.1f;
        private float _far = 10000.0f;
        private float _dist = 5.0f;
        private float _windowWidth;
        private float _windowHeight;
        
        private float _yaw = 0.45f;
        private float _pitch = -0.55f;
        
        // View info
        private Vector3 _position = new Vector3(0, 0, 5.0f);
        private Vector3 _lookDirection = Vector3.UnitZ;
        private Vector3 _upDirection = Vector3.UnitY;
        private float _moveSpeed = 10.0f;
        
        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }
        
        public IGraphicsDeviceOperation Renderer { get; set; }
        
        public Camera(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            ProjectionMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
            UpdateProjectionMatrix();
            UpdateViewMatrix();

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
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(vfov, aspectRatio, zNear, zFar);
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
        
        private Vector3 GetLookDir()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            return lookDir;
        }

        private void UpdateViewMatrix()
        {
            ViewMatrix = Matrix4x4.CreateLookAt(_position, new Vector3(0, 0, 0), _upDirection);
        }

        private void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _windowWidth/_windowHeight, _near, _far);
        }
    }
}