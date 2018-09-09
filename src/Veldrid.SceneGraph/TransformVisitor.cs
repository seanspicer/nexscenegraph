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

using System.Numerics;

namespace Veldrid.SceneGraph
{
    public class TransformVisitor : NodeVisitor
    {
        public enum CoordMode
        {
            WorldToLocal,
            LocalToWorld
        }

        private CoordMode _coordMode;
        private Matrix4x4 _matrix;
        private bool _ignoreCameras;
        
        public TransformVisitor(Matrix4x4 matrix, CoordMode coordMode, bool ignoreCameras) 
            : base(VisitorType.NodeVisitor)
        {
            _matrix = matrix;
            _coordMode = coordMode;
            _ignoreCameras = ignoreCameras;
        }

        public override void Apply(Transform transform)
        {
            if (_coordMode==CoordMode.LocalToWorld)
            {
                transform.ComputeLocalToWorldMatrix(ref _matrix, this);
            }
            else // WorldToLocal
            {
                transform.ComputeWorldToLocalMatrix(ref _matrix, this);
            }
        }

        public void Accumulate(NodePath nodePath)
        {
            if (0 == nodePath.Count) return;

            var elt = nodePath.First;
            if (_ignoreCameras)
            {
                // We need to find the last absolute Camera in NodePath and
                // set the i index to after it so the final accumulation set ignores it.
                elt = nodePath.Last;
                while (elt != null)
                {
                    if (elt.Value is Camera camera &&
                        (camera.ReferenceFrame != Transform.ReferenceFrameType.Relative || camera.NumParents == 0))
                    {
                        break;
                    }

                    elt = elt.Previous;
                }
            }

            while (elt != null)
            {
                elt.Value.Accept(this);
                elt = elt.Next;
            }
        }
    }
}