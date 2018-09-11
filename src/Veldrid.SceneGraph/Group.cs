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

using System.Collections.Generic;
using System.Linq;

namespace Veldrid.SceneGraph
{
    public class Group : Node
    {
        private List<Node> _children = new List<Node>();
        
        public Group() {}
        
        public virtual bool AddChild(Node child)
        {
            return InsertChild(_children.Count, child);
        }

        public virtual bool InsertChild(int index, Node child)
        {
            if (null == child) return false;
            
            if (_children.Exists(x => x.Id == child.Id))
            {
                // TODO Insert logging message
                return false;
            }

            if (index >= _children.Count)
            {
                index = _children.Count;
                _children.Add(child);
            }
            else
            {
                _children.Add(child);
            }

            child.AddParent(this);

            ChildInserted(index);
            
            DirtyBound();
            
            return true;
        }

        public virtual bool RemoveChild(Node child)
        {
            var pos = _children.FindIndex(x => x.Id == child.Id);
            return pos < _children.Count && RemoveChildren(pos, 1);
        }

        public virtual bool RemoveChildren(int pos, int numChildrenToRemove)
        {
            if (pos > _children.Count || numChildrenToRemove <= 0) return false;

            var endOfRemoveRange = pos + numChildrenToRemove;
            if (endOfRemoveRange > _children.Count)
            {
                // TODO add logging
                endOfRemoveRange = _children.Count;
            }

            for (var i = pos; i < endOfRemoveRange; ++i)
            {
                var child = _children[i];
                child.RemoveParent(this);
            }

            _children.RemoveRange(pos, numChildrenToRemove);
            
            ChildRemoved(pos, endOfRemoveRange - pos);
            
            DirtyBound();

            return true;
        }
        

        protected virtual void ChildInserted(int index)
        {
            // Do nothing by default
        }

        protected virtual void ChildRemoved(int index, int count)
        {
            // Do nothing by default
        }
        
        public override void Traverse(NodeVisitor nv)
        {
            foreach (var child in _children)
            {
                child.Accept(nv);
            }
        }

        public override BoundingSphere ComputeBound()
        {
            var bsphere = new BoundingSphere();
            if (0 == _children.Count)
            {
                return bsphere;
            }

            // note, special handling of the case when a child is an Transform,
            // such that only Transforms which are relative to their parents coordinates frame (i.e this group)
            // are handled, Transform relative to and absolute reference frame are ignored.

            var bb = new BoundingBox();
            bb.Init();
            foreach(var child in _children)
            {
                switch (child)
                {
                    case Transform transform when transform.ReferenceFrame != Transform.ReferenceFrameType.Relative:
                        continue;
                    case Drawable drawable:
                        bb.ExpandBy(drawable.GetBoundingBox());
                        break;
                    default:
                        var bs = child.GetBound();
                        bb.ExpandBy(bs);
                        break;
                }
            }

            if (!bb.Valid())
            {
                return bsphere;
            }

            bsphere.Center = bb.Center;
            bsphere.Radius = 0.0f;
            foreach(var child in _children)
            {
                if (child is Transform transform &&
                    transform.ReferenceFrame != Transform.ReferenceFrameType.Relative) continue;
                var bs = child.GetBound();
                bsphere.ExpandRadiusBy(bs);
            }

            return bsphere;
        }
    }
}