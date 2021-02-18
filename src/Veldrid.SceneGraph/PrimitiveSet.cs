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

namespace Veldrid.SceneGraph
{
    public interface IPrimitiveSet : IObject
    {
        IBoundingBox InitialBoundingBox { get; set; }
        IDrawable Drawable { get; }
        PrimitiveTopology PrimitiveTopology { get; set; }
        event Func<PrimitiveSet, IBoundingBox> ComputeBoundingBoxCallback;
        void DirtyBound();
        IBoundingBox GetBoundingBox();
        float GetEyePointDistance(Vector3 eyeLocal);
        void Draw(CommandList commandList);
        void Accept(IPrimitiveFunctor functor);
    }
    
    public abstract class PrimitiveSet : Object, IPrimitiveSet
    {
        protected bool _boundingSphereComputed = false;
        protected IBoundingSphere _boundingSphere = BoundingSphere.Create();
        
        protected IBoundingBox _boundingBox;
        protected IBoundingBox _initialBoundingBox = BoundingBox.Create();
        public IBoundingBox InitialBoundingBox
        {
            get => _initialBoundingBox;
            set
            {
                _initialBoundingBox = value;
                DirtyBound();
            }
        } 
        
        public event Func<PrimitiveSet, IBoundingBox> ComputeBoundingBoxCallback;
        
        public IDrawable Drawable { get; }

        public virtual float GetEyePointDistance(Vector3 eyeLocal)
        {
            return ComputeDistance(eyeLocal);
        }

        public PrimitiveTopology PrimitiveTopology { get; set; }
        
        protected PrimitiveSet(IDrawable drawable, PrimitiveTopology primitiveTopology)
        {
            PrimitiveTopology = primitiveTopology;
            Drawable = drawable;
        }
        
        public void DirtyBound()
        {
            if (!_boundingSphereComputed) return;
            
            _boundingSphereComputed = false;
        }
        
        public IBoundingBox GetBoundingBox()
        {
            if (_boundingSphereComputed) return _boundingBox;
            
            _boundingBox = _initialBoundingBox;

            _boundingBox.ExpandBy(null != ComputeBoundingBoxCallback
                ? ComputeBoundingBoxCallback(this)
                : ComputeBoundingBox());

            if (_boundingBox.Valid())
            {
                _boundingSphere.Set(_boundingBox.Center, _boundingBox.Radius);
            }
            else
            {
                _boundingSphere.Init();
            }

            _boundingSphereComputed = true;

            return _boundingBox;
        }
        
        public abstract void Draw(CommandList commandList);

        protected abstract IBoundingBox ComputeBoundingBox();

        protected abstract float ComputeDistance(Vector3 point);

        public virtual void Accept(IPrimitiveFunctor functor) {}

    }
}