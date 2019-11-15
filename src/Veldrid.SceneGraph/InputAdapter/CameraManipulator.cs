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
//using Common.Logging;

namespace Veldrid.SceneGraph.InputAdapter
{
    public abstract class CameraManipulator : InputEventHandler, ICameraManipulator
    {
        protected abstract Matrix4x4 InverseMatrix { get; }

        private bool _autoComputeHomePosition;

        private Vector3 _homeEye;
        private Vector3 _homeCenter;
        private Vector3 _homeUp;
        
        protected CameraManipulator()
        {
            _autoComputeHomePosition = true;

            _homeEye = -Vector3.UnitY;
            _homeCenter = Vector3.Zero;
            _homeUp = Vector3.UnitZ;
        }

        public void SetNode(INode node)
        {
        }

        public INode GetNode()
        {
            return null;
        }

        public void ViewAll()
        {
            ViewAll(20);
        }

        public abstract void ViewAll(float slack);
        
        // Update a camera
        public virtual void UpdateCamera(ref ICamera camera)
        {
            camera.SetViewMatrix(InverseMatrix);
        }
        
        public void ComputeHomePosition(ICamera camera, bool useBoundingBox)
        {
            if (null != GetNode())
            {
                IBoundingSphere boundingSphere;
                if (useBoundingBox)
                {
                    // TODO - need to implement ComputeBoundsVisitor
                    throw new NotImplementedException();
                }

                else
                {
                    boundingSphere = GetNode().ComputeBound();
                }
                
                System.Diagnostics.Debug.WriteLine($"    boundingSphere.Center= {boundingSphere.Center}");
                System.Diagnostics.Debug.WriteLine($"    boundingSphere.Radius= {boundingSphere.Radius}");
                
                var radius = Math.Max(boundingSphere.Radius, 1e-6);
                
                var dist = 3.5f * radius;

                if (null != camera)
                {
                    float left = 0, right = 0, bottom = 0, top = 0, zNear = 0, zFar = 0;
                    if (camera.GetProjectionMatrixAsFrustum(
                        ref left, ref right,
                        ref bottom, ref top,
                        ref zNear, ref zFar))
                    {
                        var vertical2 = Math.Abs(right - left) / zNear / 2f;
                        var horizontal2 = Math.Abs(top - bottom) / zNear / 2f;
                        var dim = horizontal2 < vertical2 ? horizontal2 : vertical2;
                        var viewAngle = Math.Atan2(dim,1f);
                        dist = radius / Math.Sin(viewAngle);
                    }
                    else
                    {
                        // Compute dist from ortho
                        if (camera.GetProjectionMatrixAsOrtho(
                            ref left, ref right,
                            ref bottom, ref top,
                            ref zNear, ref zFar))
                        {
                            dist = Math.Abs(zFar - zNear) / 2f;
                        }
                    }
                }
            }
        }
        
        public void SetHomePosition(Vector3 eye, Vector3 center, Vector3 up, bool autoComputeHomePosition=false)
        {
            SetAutoComputeHomePosition(autoComputeHomePosition);
            _homeEye = eye;
            _homeCenter = center;
            _homeUp = up;
        }

        public void GetHomePosition(out Vector3 eye, out Vector3 center, out Vector3 up)
        {
            eye = _homeEye;
            center = _homeCenter;
            up = _homeUp;
        }

        public void SetAutoComputeHomePosition(bool flag)
        {
            _autoComputeHomePosition = flag;
        }

        public bool GetAutoComputeHomePosition()
        {
            return _autoComputeHomePosition;
        }
    }
}