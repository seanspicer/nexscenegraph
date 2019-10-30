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
using System.Collections.Generic;

namespace Veldrid.SceneGraph
{

    public class Geode : Group, IGeode
    {
        //private List<IDrawable> _drawables = new List<IDrawable>();
        //public IReadOnlyList<IDrawable> Drawables => _drawables;
        
        protected IBoundingBox _boundingBox;
        protected IBoundingBox _initialBoundingBox = BoundingBox.Create();

        //public event Func<INode, IBoundingBox> ComputeBoundingBoxCallback;

        public new static IGeode Create()
        {
            return new Geode();
        }
        
        protected Geode()
        {
            
        }
        
        public override void Accept(INodeVisitor nv)
        {
            if (nv.ValidNodeMask(this))
            {
                nv.PushOntoNodePath(this);
                nv.Apply(this);
                nv.PopFromNodePath(this);
            };
        }

        public virtual bool AddDrawable(IDrawable drawable)
        {
            return AddChild(drawable);
            //_drawables.Add(drawable);
        }

        public int GetNumDrawables()
        {
            return _children.Count;
        }

        public IDrawable GetDrawable(int index)
        {
            if (index < 0 || index > _children.Count)
            {
                throw new ArgumentException("Index out of bounds");
            }
            
            return _children[(int)index].Item1 as IDrawable;
        }

        public IBoundingBox GetBoundingBox()
        {
            if (!_boundingSphereComputed) GetBound();
            return _boundingBox;
            
            
//            if (_boundingSphereComputed) return _boundingBox;
//            
//            _boundingBox = _initialBoundingBox;
//
//            _boundingBox.ExpandBy(null != ComputeBoundingBoxCallback
//                ? ComputeBoundingBoxCallback(this)
//                : ComputeBoundingBox());
//
//            if (_boundingBox.Valid())
//            {
//                _boundingSphere.Set(_boundingBox.Center, _boundingBox.Radius);
//            }
//            else
//            {
//                _boundingSphere.Init();
//            }
//
//            _boundingSphereComputed = true;
//
//            return _boundingBox;
        }

        public override IBoundingSphere ComputeBound()
        {
            var boundingSphere = SceneGraph.BoundingSphere.Create();
            var bb = BoundingBox.Create();
            foreach (var child in _children)
            {
                if(child.Item1 is IDrawable drawable)
                {
                    bb.ExpandBy(drawable.GetBoundingBox());
                }
            }

            _boundingBox = bb;

            if (_boundingBox.Valid())
            {
                boundingSphere.ExpandBy(_boundingBox);
            }

            return boundingSphere;
        }

//        protected IBoundingBox ComputeBoundingBox()
//        {
//            var bb = BoundingBox.Create();
//            foreach (var drawable in _children)
//            {
//                bb.ExpandBy(drawable.GetBoundingBox());
//            }
//
//            return bb;
//        }
    }
}