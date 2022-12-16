//
// Copyright 2018-2021 Sean Spicer 
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
using System.Diagnostics;
using System.Numerics;

//using Common.Logging;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface ICameraManipulator : IUiEventHandler
    {
        Matrix4x4 InverseMatrix { get; }
        
        float ZoomScale { get; }
        
        void SetNode(INode node);

        INode GetNode();

        void ViewAll(IUiActionAdapter aa, float slack = 20);

        void UpdateCamera(ICamera camera);

        void SetCameraOrthographic(ICamera camera);
        void SetCameraPerspective(ICamera camera);
        
        void ComputeHomePosition(ICamera camera = null, bool useBoundingBox = false);

        void SetHomePosition(Vector3 eye, Vector3 center, Vector3 up, bool autoComputeHomePosition = false);

        void GetHomePosition(out Vector3 eye, out Vector3 center, out Vector3 up);

        void SetAutoComputeHomePosition(bool flag);

        bool GetAutoComputeHomePosition();

        void Home(IUiActionAdapter aa);

        public interface ICoordinateFrameCallback
        {
            Matrix4x4 GetCoordinateFrame(Vector3 position);
        }
    }


    public abstract class CameraManipulator : UiEventHandler, ICameraManipulator
    {
        private bool _autoComputeHomePosition;
        protected Vector3 _homeCenter;

        protected Vector3 _homeEye;
        protected Vector3 _homeUp;

        protected CameraManipulator()
        {
            _autoComputeHomePosition = true;

            _homeEye = -Vector3.UnitY;
            _homeCenter = Vector3.Zero;
            _homeUp = Vector3.UnitZ;
        }

        public abstract Matrix4x4 InverseMatrix { get; }

        public abstract float ZoomScale { get; }

        public ICameraManipulator.ICoordinateFrameCallback CoordinateFrameCallback { get; set; } = null;

        public virtual void SetNode(INode node)
        {
        }

        public virtual INode GetNode()
        {
            return null;
        }

        public abstract void ViewAll(IUiActionAdapter aa, float slack = 20);

        public virtual void SetCameraOrthographic(ICamera camera)
        {
            var lookDistance = 1f;
            if (this is TrackballManipulator trackballManipulator)
            {
                lookDistance = trackballManipulator.Distance;
            }
            
            OrthographicCameraOperations.ConvertFromPerspectiveToOrthographic(camera);
            
            UpdateCameraOrthographic(camera, camera.Viewport.Width, camera.Viewport.Height, lookDistance);
        }

        public virtual void SetCameraPerspective(ICamera camera)
        {
            PerspectiveCameraOperations.ConvertFromOrthographicToPerspective(camera);
            
            var fov = PerspectiveCameraOperations.GetVerticalFov(camera);
            
            PerspectiveCameraOperations.SetProjectionMatrixAsPerspective(camera, 
                fov,
                (float)camera.Viewport.Width / camera.Viewport.Height, 
                1.0f, 
                100.0f);;
        }

        protected virtual void UpdateCameraOrthographic(ICamera camera, float width, float height, float dist)
        {
            OrthographicCameraOperations.SetProjectionMatrixAsOrthographic(camera, width, height, -dist/2,
                dist/2);
            
            UpdateCamera(camera);
        }
        
        // Update a camera
        public virtual void UpdateCamera(ICamera camera)
        {
            camera.SetViewMatrix(InverseMatrix);
        }

        public void ComputeHomePosition(ICamera camera, bool useBoundingBox)
        {
            if (null == GetNode()) return;

            var boundingSphere = BoundingSphere.Create();
            if (useBoundingBox)
            {
                var cbVisitor = ComputeBoundsVisitor.Create();
                GetNode().Accept(cbVisitor);

                var bb = cbVisitor.GetBoundingBox();
                if (bb.Valid())
                    boundingSphere.ExpandBy(bb);
                else
                    boundingSphere = GetNode().GetBound();
            }

            else
            {
                boundingSphere = GetNode().ComputeBound();
            }

            Debug.WriteLine($"    boundingSphere.Center= {boundingSphere.Center}");
            Debug.WriteLine($"    boundingSphere.Radius= {boundingSphere.Radius}");

            var radius = System.Math.Max(boundingSphere.Radius, 1e-6);

            var dist = 3.5f * radius;

            if (null != camera)
            {
                float left = 0, right = 0, bottom = 0, top = 0, zNear = 0, zFar = 0;
                switch (camera.Projection)
                {
                    case ProjectionMatrixType.Perspective:
                        PerspectiveCameraOperations.GetProjectionMatrixAsFrustum(
                            camera,
                            ref left, ref right,
                            ref bottom, ref top,
                            ref zNear, ref zFar);
                        break;

                    case ProjectionMatrixType.Orthographic:
                        OrthographicCameraOperations.GetProjectionMatrixAsOrtho(
                            camera,
                            ref left, ref right,
                            ref bottom, ref top,
                            ref zNear, ref zFar);
                        break;
                    default:
                        throw new Exception("Unknown Camera type detected");
                }

                var vertical2 = System.Math.Abs(right - left) / zNear / 2f;
                var horizontal2 = System.Math.Abs(top - bottom) / zNear / 2f;
                var dim = horizontal2 < vertical2 ? horizontal2 : vertical2;
                var viewAngle = System.Math.Atan2(dim, 1f);
                dist = radius / System.Math.Sin(viewAngle);
            }

            SetHomePosition(boundingSphere.Center - (float) dist * Vector3.UnitY,
                boundingSphere.Center,
                Vector3.UnitZ,
                _autoComputeHomePosition);
        }

        public void SetHomePosition(Vector3 eye, Vector3 center, Vector3 up, bool autoComputeHomePosition = false)
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

        public virtual void Home(IUiActionAdapter aa)
        {
        }

        public Matrix4x4 GetCoordinateFrame(Vector3 position)
        {
            return CoordinateFrameCallback?.GetCoordinateFrame(position) ?? Matrix4x4.Identity;
        }

        public Vector3 GetUpVector(Matrix4x4 coordinateFrame)
        {
            return new Vector3(coordinateFrame.M31, coordinateFrame.M32, coordinateFrame.M33);
        }
    }
}