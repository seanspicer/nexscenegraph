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
using System.Collections.Generic;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public static class PerspectiveCameraOperations
    {
        private static readonly Dictionary<ICamera, Tuple<float, float>> NearFarPlaneCache;

        static PerspectiveCameraOperations()
        {
            NearFarPlaneCache = new Dictionary<ICamera, Tuple<float, float>>();
        }

        public static void GuardPerspective(ICamera camera)
        {
            if (camera.Projection != ProjectionMatrixType.Perspective)
            {
                throw new ArgumentException("Expected Perspective Camera, but got Orthographic Camera");
            }
        }
        
        public static float GetVerticalFov(ICamera camera)
        {
            GuardPerspective(camera);
            return (float) System.Math.Atan2(camera.Height / 2.0f, camera.Distance) * 2.0f;
        }
        
        public static void SetProjectionMatrixAsPerspective(ICamera camera, float vfov, float aspectRatio, float zNear, float zFar)
        {
            GuardPerspective(camera);
            camera.SetProjectionMatrix(Matrix4x4.CreatePerspectiveFieldOfView(vfov, aspectRatio, zNear, zFar));

            if (NearFarPlaneCache.ContainsKey(camera))
            {
                NearFarPlaneCache[camera] = Tuple.Create(zNear, zFar);
            }
            else
            {
                NearFarPlaneCache.Add(camera, Tuple.Create(zNear, zFar));
            }
        }

        public static bool GetProjectionMatrixAsFrustum(ICamera camera, ref float left, ref float right, ref float bottom, ref float top,
            ref float zNear,
            ref float zFar)
        {
            GuardPerspective(camera);
            return camera.ProjectionMatrix.GetFrustum(ref left, ref right, ref bottom, ref top, ref zNear, ref zFar);
        }

        public static ICamera CreatePerspectiveCamera(uint width, uint height, float distance)
        {
            var newCamera = new Camera(width, height, distance, ProjectionMatrixType.Perspective);
            
            var fov = GetVerticalFov(newCamera);
            
            SetProjectionMatrixAsPerspective(newCamera, fov, (float)width / height, 1.0f, 100.0f);

            return newCamera;
        }

        public static void ConvertFromOrthographicToPerspective(ICamera camera)
        {
            if (camera.Projection == ProjectionMatrixType.Perspective)
            {
                throw new ArgumentException("Expected Orthographic Camera, but got Perspective Camera");
            }
            
            camera.SetProjection(ProjectionMatrixType.Perspective);
            //var fov = GetVerticalFov(camera);
            
            //SetProjectionMatrixAsPerspective(camera, fov, (float)camera.Width / camera.Height, 1.0f, 100.0f);
        }
        
        internal static void ResizeProjection(ICamera camera, int width, int height,
            ResizeMask resizeMask = ResizeMask.ResizeDefault)
        {
            GuardPerspective(camera);
            
            double previousWidth = camera.Viewport.Width;
            double previousHeight = camera.Viewport.Height;
            double newWidth = width;
            double newHeight = height;
            
            float zNear = 0.0f;
            float zFar = 100.0f;
            if (NearFarPlaneCache.ContainsKey(camera))
            {
                (zNear, zFar) = NearFarPlaneCache[camera];
            }
            
            // TODO -- THIS NEEDS TO BE MOVED, it shouldn't be necessary.
            if (System.Math.Abs(previousWidth) < 1e-6 && System.Math.Abs(previousHeight) < 1e-6)
            {
                // TODO: This is tricky - need to fix when ViewAll implemented
                var vfov = (float) System.Math.Atan2(height / 2.0f, camera.Distance) * 2.0f;

                var aspectRatio = width / (float) height;
                SetProjectionMatrixAsPerspective(camera, vfov, aspectRatio, zNear, zFar);


                if ((resizeMask & ResizeMask.ResizeViewport) != 0) camera.SetViewport(0, 0, width, height);
            }

            if (previousWidth > 1e-6 && System.Math.Abs(previousWidth - newWidth) > 1e-6 ||
                previousHeight > 1e-6 && System.Math.Abs(previousHeight - newHeight) > 1e-6)
            {
                if ((resizeMask & ResizeMask.ResizeProjectionMatrix) != 0)
                {
                    var widthChangeRatio = newWidth / previousWidth;
                    var heightChangeRatio = newHeight / previousHeight;
                    var aspectRatioChange = widthChangeRatio / heightChangeRatio;

                    if (System.Math.Abs(aspectRatioChange - 1.0) > 1e-6)
                        switch (camera.ProjectionResizePolicy)
                        {
                            case ProjectionResizePolicy.Horizontal:
                                camera.SetProjectionMatrix(camera.ProjectionMatrix *
                                                           Matrix4x4.CreateScale(1.0f / (float) aspectRatioChange, 1.0f,
                                                               1.0f));
                                break;
                            case ProjectionResizePolicy.Vertical:
                                camera.SetProjectionMatrix(camera.ProjectionMatrix *
                                                           Matrix4x4.CreateScale(1.0f, (float) aspectRatioChange, 1.0f));
                                break;
                            case ProjectionResizePolicy.Fixed:

                                var aspectRatio = width / (float) height;
                                SetProjectionMatrixAsPerspective(camera, GetVerticalFov(camera), aspectRatio, zNear, zFar);

                                break;
                        }
                }

                if ((resizeMask & ResizeMask.ResizeViewport) != 0) camera.SetViewport(0, 0, width, height);
            }
        }
    }
}