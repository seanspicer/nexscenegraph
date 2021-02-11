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
using SixLabors.ImageSharp.Processing;
using Veldrid.SceneGraph.Util;

namespace Veldrid.SceneGraph.NodeKits.DirectVolumeRendering
{
    public interface ILocator : IObject
    {
        public interface ILocatorCallback
        {
            void LocatorModified(ILocator locator);
        }
        
        Matrix4x4 Transform { get; }
        
        void ComputeLocalBounds(ref Vector3 left, ref Vector3 right);

        void AddCallback(ILocatorCallback callback);
        void RemoveCallback(ILocatorCallback callback);
    }
    
    
    public class Locator : Object, ILocator
    {
        public class LocatorCallback : ILocator.ILocatorCallback
        {
            public virtual void LocatorModified(ILocator locator)
            {
            }
        }

        protected HashSet<ILocator.ILocatorCallback> LocatorCallbacks { get; } =
            new HashSet<ILocator.ILocatorCallback>();
        
        protected Matrix4x4 _transform = Matrix4x4.Identity;
        protected Matrix4x4 _inverse = Matrix4x4.Identity;

        void SetTransformAsExtents(float minX, float minY, float maxX, float maxY, float minZ, float maxZ)
        {
            _transform = Matrix4x4.Identity;
            _transform.M11 = maxX - minX;
            _transform.M22 = maxY - minY;
            _transform.M33 = maxZ - minZ;
            _transform.M41 = minX;
            _transform.M42 = minY;
            _transform.M43 = minZ;

            if (Matrix4x4.Invert(_transform, out var inverse))
            {
                _inverse = inverse;
            }

            LocatorModified();
        }
        
        public Vector3 ConvertLocalToModel(Vector3 local)
        {
            return _transform.PreMultiply(local);
        }
        
        public Vector3 ConvertModelToLocal(Vector3 world)
        {
            return _inverse.PreMultiply(world);

        }

        public Matrix4x4 Transform => _transform;

        public void ComputeLocalBounds(ref Vector3 bottomLeft, ref Vector3 topRight)
        {
            var corners = new List<Vector3>();

            corners.Add(ConvertLocalToModel(new Vector3(0.0f, 0.0f, 0.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(1.0f, 0.0f, 0.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(0.0f, 1.0f, 0.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(1.0f, 1.0f, 0.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(0.0f, 0.0f, 1.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(1.0f, 0.0f, 1.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(0.0f, 1.0f, 1.0f)));
            corners.Add(ConvertLocalToModel(new Vector3(1.0f, 1.0f, 1.0f)));

            foreach (var corner in corners)
            {
                bottomLeft.X = System.Math.Min(bottomLeft.X, corner.X);
                bottomLeft.Y = System.Math.Min(bottomLeft.Y, corner.Y);
                bottomLeft.Z = System.Math.Min(bottomLeft.Z, corner.Z);
                topRight.X = System.Math.Max(topRight.X, corner.X);
                topRight.Y = System.Math.Max(topRight.Y, corner.Y);
                topRight.Z = System.Math.Max(topRight.Z, corner.Z);
            }
        }

        public void AddCallback(ILocator.ILocatorCallback callback)
        {
            LocatorCallbacks.Add(callback);
        }

        public void RemoveCallback(ILocator.ILocatorCallback callback)
        {
            LocatorCallbacks.Remove(callback);
        }

        protected void LocatorModified()
        {
            foreach (var callback in LocatorCallbacks)
            {
                callback.LocatorModified(this);
            }
        }
    }

    public class TransformLocatorCallback : Locator.LocatorCallback
    {
        protected IMatrixTransform _transform;
        
        public static ILocator.ILocatorCallback Create(IMatrixTransform transform)
        {
            return new TransformLocatorCallback(transform);
        }

        protected TransformLocatorCallback(IMatrixTransform transform)
        {
            _transform = transform;
        }

        public override void LocatorModified(ILocator locator)
        {
            if (null != _transform)
            {
                _transform.Matrix = locator.Transform;
            }
        }
    }
    
}