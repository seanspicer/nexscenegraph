//
// Copyright 2018-2020 Sean Spicer 
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

using System.Linq;
using System.Numerics;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph
{
    public interface IAntiSquish : ITransform
    {
    }


    /// <summary>
    ///     This node "un-transforms" everything below it making the scaling uniform along all axes
    /// </summary>
    public class AntiSquish : Transform, IAntiSquish
    {
        private Matrix4x4 _cache;

        private bool _cacheDirty;
        private Matrix4x4 _cacheLocalToWorld;
        private readonly Vector3 _pivot;
        private readonly Vector3 _position;
        private readonly bool _usePivot;

        private readonly bool _usePosition;

        protected AntiSquish()
        {
            _usePivot = false;
            _usePosition = false;
            _cacheDirty = true;
        }

        protected AntiSquish(Vector3 pivot)
        {
            _pivot = pivot;
            _usePivot = true;
            _usePosition = false;
            _cacheDirty = true;
        }

        protected AntiSquish(Vector3 pivot, Vector3 position)
        {
            _pivot = pivot;
            _usePivot = true;

            _position = position;
            _usePosition = true;

            _cacheDirty = true;
        }

        public override bool ComputeLocalToWorldMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            var unsquishedMatrix = Matrix4x4.Identity;
            if (false == ComputeUnsquishedMatrix(ref unsquishedMatrix)) return false;

            matrix = ReferenceFrame == ReferenceFrameType.Relative
                ? matrix.PreMultiply(unsquishedMatrix)
                : unsquishedMatrix;

            return true;
        }

        public override bool ComputeWorldToLocalMatrix(ref Matrix4x4 matrix, NodeVisitor visitor)
        {
            var unsquishedMatrix = Matrix4x4.Identity;
            if (false == ComputeUnsquishedMatrix(ref unsquishedMatrix)) return false;

            if (false == Matrix4x4.Invert(unsquishedMatrix, out var inverse)) return false;
            ;

            matrix = ReferenceFrame == ReferenceFrameType.Relative ? matrix.PostMultiply(inverse) : inverse;

            return true;
        }


        public new static IAntiSquish Create()
        {
            return new AntiSquish();
        }

        public static IAntiSquish Create(Vector3 pivot)
        {
            return new AntiSquish(pivot);
        }

        public static IAntiSquish Create(Vector3 pivot, Vector3 position)
        {
            return new AntiSquish(pivot, position);
        }

        // This method is to enable unit testing
        protected virtual Matrix4x4 GetLocalToWorld(NodePath np)
        {
            return ComputeLocalToWorld(np);
        }

        protected bool ComputeUnsquishedMatrix(ref Matrix4x4 unsquished)
        {
            var nodePaths = GetParentalNodePaths();
            if (false == nodePaths.Any()) return false;

            var np = nodePaths.First();
            if (false == np.Any()) return false;

            // Remove last node in the path, which is the AntiSquish itself.
            np.RemoveLast();

            // Get the accululated modeling matrix
            var localToWorld = GetLocalToWorld(np);

            // Reuse cached value 
            if (false == _cacheDirty && _cacheLocalToWorld == localToWorld)
            {
                unsquished = _cache;
                return true;
            }

            localToWorld.DecomposeAffine(out var s, out var r, out var t, out var so);

            var av = (s.X + s.Y + s.Z) / 3.0f;
            s.X = av;
            s.Y = av;
            s.Z = av;

            if (System.Math.Abs(av) < 1e-6) return false;

            // Pivot
            if (_usePivot)
            {
                unsquished = unsquished.PostMultiplyTranslate(-_pivot);

                var tmps = Matrix4x4.CreateFromQuaternion(so);
                if (!Matrix4x4.Invert(tmps, out var inv_tmps))
                    return false;

                unsquished = unsquished.PostMultiply(inv_tmps);
                
                unsquished = unsquished.PostMultiplyScale(s);

                unsquished = unsquished.PostMultiply(tmps);
                
                unsquished = unsquished.PostMultiplyRotate(r);
                unsquished = unsquished.PostMultiplyTranslate(t);

                if (false == Matrix4x4.Invert(localToWorld, out var invltw)) return false;

                unsquished = unsquished.PostMultiply(invltw);

                if (_usePosition)
                    unsquished = unsquished.PostMultiplyTranslate(_position);
                else
                    unsquished = unsquished.PostMultiplyTranslate(_pivot);
            }
            // No Pivot
            else
            {
                if (!Matrix4x4.Invert(Matrix4x4.CreateFromQuaternion(so), out var inv_so))
                    return false;

                var tmps = Matrix4x4.CreateFromQuaternion(so);
                if (!Matrix4x4.Invert(tmps, out var inv_tmps))
                    return false;

                unsquished = unsquished.PostMultiply(inv_tmps);
                
                unsquished = unsquished.PostMultiplyScale(s);

                unsquished = unsquished.PostMultiply(tmps);
                unsquished = unsquished.PostMultiplyRotate(r);
                unsquished = unsquished.PostMultiplyTranslate(t);

                if (false == Matrix4x4.Invert(localToWorld, out var invltw)) return false;
            }

            _cache = unsquished;
            _cacheLocalToWorld = localToWorld;
            _cacheDirty = false;

            DirtyBound();

            return true;
        }
    }
}