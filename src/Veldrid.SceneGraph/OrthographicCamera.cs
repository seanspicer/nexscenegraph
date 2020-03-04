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
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public class OrthographicCamera : Camera, IOrthographicCamera
    {
        public new static IOrthographicCamera Create()
        {
            return new OrthographicCamera();
        }

        protected OrthographicCamera()
        {
            var height = DisplaySettings.Instance.ScreenHeight;
            var width = DisplaySettings.Instance.ScreenWidth;
            var dist = DisplaySettings.Instance.ScreenDistance;
            
            SetProjectionMatrixAsOrthographic(width, height, dist, -dist);
        }
        
        public void SetProjectionMatrixAsOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
        {
            SetProjectionMatrix(Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zFar, zNear));
        }

        public void SetProjectionMatrixAsOrthographic(float width, float height, float zNear, float zFar)
        {
            var left = -width / 2.0f;
            var right = width / 2.0f;
            var top = height / 2.0f;
            var bottom = -height / 2.0f;
            
            SetProjectionMatrixAsOrthographicOffCenter(left, right, bottom, top, zNear, zFar);
        }
        
        public bool GetProjectionMatrixAsOrtho(ref float left, ref float right, ref float bottom, ref float top, ref float zNear,
            ref float zFar)
        {
            return ProjectionMatrix.GetOrtho(ref left, ref right, ref bottom, ref top, ref zNear, ref zFar);
        }
        
        protected override void ResizeProjection(int width, int height, ResizeMask resizeMask = ResizeMask.ResizeDefault)
        {
            double previousWidth = Viewport.Width;
            double previousHeight = Viewport.Height;
            double newWidth = width;
            double newHeight = height;

            var dist = DisplaySettings.Instance.ScreenDistance;
            
            // TODO -- THIS NEEDS TO BE MOVED, it shouldn't be necessary.
            if (System.Math.Abs(previousWidth) < 1e-6 && System.Math.Abs(previousHeight) < 1e-6)
            {
                
                SetProjectionMatrixAsOrthographic(width/(dist/10), height/(dist/10), dist/10, -dist/10);
                
                if((resizeMask & ResizeMask.ResizeViewport) != 0)
                {
                    SetViewport(0, 0, width, height);
                }
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
                    {
                        switch (ProjectionResizePolicy)
                        {
                            case ProjectionResizePolicy.Horizontal:
                                SetProjectionMatrix(ProjectionMatrix *
                                                    Matrix4x4.CreateScale(1.0f / (float) aspectRatioChange, 1.0f,
                                                        1.0f));
                                break;
                            case ProjectionResizePolicy.Vertical:
                                SetProjectionMatrix(ProjectionMatrix *
                                                    Matrix4x4.CreateScale(1.0f, (float) aspectRatioChange, 1.0f));
                                break;
                            case ProjectionResizePolicy.Fixed:

                                SetProjectionMatrix(ProjectionMatrix *
                                                    Matrix4x4.CreateScale(1.0f / (float) widthChangeRatio,
                                                        1.0f / (float) heightChangeRatio, 1.0f));
                                break;
                        }
                    }
                }
            }
        }
    }
}