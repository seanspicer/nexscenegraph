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
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;
using Math = System.Math;

namespace Veldrid.SceneGraph
{
    
    public class Camera : Transform, ICamera
    {
        public View View { get; set; }
        
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

        public IGraphicsDeviceOperation Renderer { get; set; }

        public static ICamera Create()
        {
            return new Camera();
        }
        
        protected Camera()
        {
            ClearColor = RgbaFloat.Grey;
            Viewport = null;
            ProjectionMatrix = Matrix4x4.Identity;
            ViewMatrix = Matrix4x4.Identity;
            ProjectionResizePolicy = ProjectionResizePolicy.Fixed;
        }

        public void HandleResizeEvent(IResizedEvent resizedEvent)
        {
            Resize(resizedEvent.Width, resizedEvent.Height);
        }
        
        public void Resize(int width, int height, ResizeMask resizeMask = ResizeMask.ResizeDefault)
        {
            if (null != Viewport)
            {
                double previousWidth = Viewport.Width;
                double previousHeight = Viewport.Height;
                double newWidth = width;
                double newHeight = height;

                if (Math.Abs(previousWidth - newWidth) > 1e-6 &&
                    Math.Abs(previousHeight - newHeight) > 1e-6)
                {
                    if ((resizeMask & ResizeMask.ResizeProjectionMatrix) != 0 &&
                        ProjectionResizePolicy != ProjectionResizePolicy.Fixed)
                    {
                        var widthChangeRatio = newWidth / previousWidth;
                        var heigtChangeRatio = newHeight / previousHeight;
                        var aspectRatioChange = widthChangeRatio / heigtChangeRatio;

                        if (Math.Abs(aspectRatioChange - 1.0) > 1e-6)
                        {
                            switch (ProjectionResizePolicy)
                            {
                                case ProjectionResizePolicy.Horizontal:
                                    SetProjectionMatrix(ProjectionMatrix *
                                                        Matrix4x4.CreateScale(1.0f / (float) aspectRatioChange, 1.0f, 1.0f));
                                    break;
                                case ProjectionResizePolicy.Vertical:
                                    SetProjectionMatrix(ProjectionMatrix *
                                                        Matrix4x4.CreateScale(1.0f, (float) aspectRatioChange, 1.0f));
                                    break;
                                case ProjectionResizePolicy.Fixed:
                                    break;
                            }
                        }
                    }
                    
                    if((resizeMask & ResizeMask.ResizeViewport) != 0)
                    {
                        SetViewport(0, 0, width, height);
                    }
                }
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

        public void SetProjectionMatrixAsOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            SetProjectionMatrix(Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zNear, zFar));
        }

        public void SetProjectionMatrixAsOrthographic(float width, float height, float zNearPlane, float zFarPlane)
        {
            SetProjectionMatrix(Matrix4x4.CreateOrthographic(width, height, zNearPlane, zFarPlane));
        }
        
        public void SetProjectionMatrixAsPerspective(float vfov, float aspectRatio, float zNear, float zFar)
        {
            SetProjectionMatrix(Matrix4x4.CreatePerspectiveFieldOfView(vfov, aspectRatio, zNear, zFar));
        }

        public bool GetProjectionMatrixAsFrustum(ref float left, ref float right, ref float bottom, ref float top, ref float zNear,
            ref float zFar)
        {
            return ProjectionMatrix.GetFrustum(ref left, ref right, ref bottom, ref top, ref zNear, ref zFar);
        }

        public bool GetProjectionMatrixAsOrtho(ref float left, ref float right, ref float bottom, ref float top, ref float zNear,
            ref float zFar)
        {
            return ProjectionMatrix.GetOrtho(ref left, ref right, ref bottom, ref top, ref zNear, ref zFar);
        }

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