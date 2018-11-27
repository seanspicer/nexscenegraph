//
// Copyright 2018 Sean Spicer 
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
    public class TransformVisitor : NodeVisitor, ITransformVisitor
    {
        public enum CoordMode
        {
            WorldToLocal,
            LocalToWorld
        }

        private readonly CoordMode _coordMode;
        private Matrix4x4 _matrix;
        private readonly bool _ignoreCameras;

        public static ITransformVisitor Create(Matrix4x4 matrix, CoordMode coordMode, bool ignoreCameras)
        {
            return new TransformVisitor(matrix, coordMode, ignoreCameras);
        }
        
        protected TransformVisitor(Matrix4x4 matrix, CoordMode coordMode, bool ignoreCameras) 
            : base(VisitorType.NodeVisitor)
        {
            _matrix = matrix;
            _coordMode = coordMode;
            _ignoreCameras = ignoreCameras;
        }

        public override void Apply(ITransform transform)
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