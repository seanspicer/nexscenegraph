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
    public interface IBillboard : IGeode
    {
        Billboard.Modes Mode { get; set; }
        Billboard.SizeModes SizeMode { get; set; }
        Matrix4x4 ComputeMatrix(Matrix4x4 modelView, Matrix4x4 projection, Vector3 eyeLocal);
    }

    public class Billboard : Geode, IBillboard
    {
        public enum Modes
        {
            Screen
        }

        public enum SizeModes
        {
            ObjectCoords,
            ScreenCoords
        }

        protected Billboard()
        {
            Mode = Modes.Screen;
            SizeMode = SizeModes.ObjectCoords;
        }

        public Modes Mode { get; set; }

        public SizeModes SizeMode { get; set; }

        public override void Accept(INodeVisitor visitor)
        {
            visitor.Apply(this);
        }


        public Matrix4x4 ComputeMatrix(Matrix4x4 modelView, Matrix4x4 projection, Vector3 eyeLocal)
        {
            var matrix = Matrix4x4.Identity;

            var rotationMatrix = Matrix4x4.Identity;

            var tmp = modelView.SetTranslation(Vector3.Zero);
            var canInvert = Matrix4x4.Invert(tmp, out rotationMatrix);
            if (false == canInvert) rotationMatrix = Matrix4x4.Identity;

            if (SizeMode != SizeModes.ObjectCoords)
            {
                var width = 960.0f;
                var height = 540.0f;

                var mvpw = rotationMatrix * modelView * projection *
                           Matrix4x4.CreateScale(width / 2.0f, height / 2.0f, 1.0f);

                var origin = Vector3.Transform(new Vector3(0.0f, 0.0f, 0.0f), mvpw);
                var left = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), mvpw) - origin;
                var up = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), mvpw) - origin;

                // compute the pixel size vector.
                var length_x = left.Length();
                var scale_x = length_x > 0.0f ? 1.0f / length_x : 1.0f;

                var length_y = up.Length();
                var scale_y = length_y > 0.0f ? 1.0f / length_y : 1.0f;

                if (SizeMode == SizeModes.ScreenCoords)
                {
                    var scaleVec = new Vector3(scale_x, scale_y, scale_x);
                    matrix = matrix.PostMultiply(Matrix4x4.CreateScale(150f * scaleVec));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            if (Mode == Modes.Screen) matrix = matrix.PostMultiply(rotationMatrix);

            return matrix;
        }

        public new static IBillboard Create()
        {
            return new Billboard();
        }
    }
}