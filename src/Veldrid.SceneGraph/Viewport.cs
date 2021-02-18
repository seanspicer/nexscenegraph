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

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public interface IViewport
    {
        int X { get; }
        int Y { get; }
        int Width { get; }
        int Height { get; }

        float AspectRatio { get; }
        
        bool Valid();
        
        Matrix4x4 ComputeWindowMatrix4X4();
    }
    
    public class Viewport : IViewport
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        public static IViewport Create(int x, int y, int width, int height)
        {
            return new Viewport(x, y, width, height);
        }

        protected Viewport(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Valid()
        {
            return Width > 0 && Height > 0;
        }

        public float AspectRatio
        {
            get
            {
                if (Height != 0)
                {
                    return (float) ((double) Width / (double) Height);
                }

                return 1.0f;
            }
        }

        public Matrix4x4 ComputeWindowMatrix4X4()
        {
            return Matrix4x4.CreateTranslation(1f, -1f, 1f) *
                   Matrix4x4.CreateScale(0.5f * Width, -0.5f * Height, 0.5f) *
                   Matrix4x4.CreateTranslation(X, Y, 0);
        }
    }
}