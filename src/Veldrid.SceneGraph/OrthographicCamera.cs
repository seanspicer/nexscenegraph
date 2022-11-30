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
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public static class OrthographicCameraOperations
    {
        public static void GuardOrthographic(ICamera camera)
        {
            if (camera.Projection != ProjectionMatrixType.Orthographic)
            {
                throw new ArgumentException("Expected Orthographic Camera, but got Perspective Camera");
            }
        }
        
        public static void SetProjectionMatrixAsOrthographicOffCenter(ICamera camera, float left, float right, float bottom, float top,
            float zNear, float zFar)
        {
            GuardOrthographic(camera);
            camera.SetProjectionMatrix(Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar));
        }

        public static void SetProjectionMatrixAsOrthographic(ICamera camera, float width, float height, float zNear, float zFar)
        {
            GuardOrthographic(camera);
            
            var left = -width / 2.0f;
            var right = width / 2.0f;
            var top = height / 2.0f;
            var bottom = -height / 2.0f;

            SetProjectionMatrixAsOrthographicOffCenter(camera, left, right, bottom, top, zNear, zFar);
        }

        public static bool GetProjectionMatrixAsOrtho(ICamera camera, ref float left, ref float right, ref float bottom, ref float top,
            ref float zNear,
            ref float zFar)
        {
            GuardOrthographic(camera);
            return camera.ProjectionMatrix.GetOrtho(ref left, ref right, ref bottom, ref top, ref zNear, ref zFar);
        }

        public static ICamera CreateOrthographicCamera(uint width, uint height, float distance)
        {
            var newCamera = new Camera(width, height, distance, ProjectionMatrixType.Orthographic);
            SetProjectionMatrixAsOrthographic(newCamera, width, height, distance, -distance);
            return newCamera;
        }

        public static void ConvertFromPerspectiveToOrthographic(ICamera camera)
        {
            if (camera.Projection == ProjectionMatrixType.Orthographic)
            {
                throw new ArgumentException("Expected Perspective Camera, but got Orthographic Camera");
            }
            
            camera.SetProjection(ProjectionMatrixType.Orthographic);
            //SetProjectionMatrixAsOrthographic(camera, camera.Width, camera.Height, -camera.Distance, camera.Distance);
        }
        
        public static void ResizeProjection(ICamera camera, int width, int height,
            ResizeMask resizeMask = ResizeMask.ResizeDefault)
        {
            GuardOrthographic(camera);
            
            double previousWidth = camera.Viewport.Width;
            double previousHeight = camera.Viewport.Height;
            double newWidth = width;
            double newHeight = height;


            // TODO -- THIS NEEDS TO BE MOVED, it shouldn't be necessary.
            if (System.Math.Abs(previousWidth) < 1e-6 && System.Math.Abs(previousHeight) < 1e-6)
            {
                SetProjectionMatrixAsOrthographic(camera, width / (camera.Distance / 10), height / (camera.Distance / 10), camera.Distance / 10,
                    -camera.Distance / 10);

                if ((resizeMask & ResizeMask.ResizeViewport) != 0)
                {
                    camera.SetViewport(0, 0, width, height);
                }
            }

            if (previousWidth > 1e-6 && System.Math.Abs(previousWidth - newWidth) > 1e-6 ||
                previousHeight > 1e-6 && System.Math.Abs(previousHeight - newHeight) > 1e-6)
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

                                camera.SetProjectionMatrix(camera.ProjectionMatrix *
                                                           Matrix4x4.CreateScale(1.0f * (float) widthChangeRatio,
                                                               1.0f * (float) heightChangeRatio, 1.0f));
                                break;
                        }
                }
        }
    }
}