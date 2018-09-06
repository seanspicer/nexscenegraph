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

using System;
using System.Collections.Generic;

namespace Veldrid.SceneGraph
{
    public class CollectParentPaths : NodeVisitor
    {
        private Node _haltTraversalAtNode;
        private List<LinkedList<Node>> _nodePaths;
        
        public CollectParentPaths(Node haltTraversalAtNode = null) :
            base(VisitorType.NodeVisitor, TraversalModeType.TraverseParents)
        {
            _haltTraversalAtNode = haltTraversalAtNode;
            _nodePaths = new List<LinkedList<Node>>();
        }

        public override void Apply(Node node)
        {
            if (node.NumParents == 0 || node == _haltTraversalAtNode)
            {
                _nodePaths.Add(NodePath);
            }
            else
            {
                Traverse(node);
            }
        }
    }
   
    public abstract class Node : Object
    {
        public Guid Id { get; private set; }
        public uint NodeMask { get; set; } = 0xffffffff;
        
        public int NumParents => _parents.Count;

        private List<Group> _parents;
        protected bool _boundingSphereComputed = false;
        protected BoundingSphere _boundingSphere;

        private BoundingSphere _initialBound = new BoundingSphere();
        public BoundingSphere InitialBound
        {
            get { return _initialBound; }
            set
            {
                _initialBound = value;
                DirtyBound();
            }
        } 

        public event Func<Node, BoundingSphere> ComputeBoundCallback;
        
        public Node()
        {
            Id = Guid.NewGuid();

            _parents = new List<Group>();
        }

        protected internal void AddParent(Group parent)
        {
            _parents.Add(parent);
        }

        protected internal void RemoveParent(Group parent)
        {
            _parents.RemoveAll(x => x.Id == parent.Id);
        }
 
        /// <summary>
        /// Mark this node's bounding sphere dirty.  Forcing it to be computed on the next call
        /// to GetBound();
        /// </summary>
        public void DirtyBound()
        {
            if (!_boundingSphereComputed) return;
            
            _boundingSphereComputed = false;
                
            foreach (var parent in _parents)
            {
                parent.DirtyBound();
            }
        }

        /// <summary>
        /// Get the bounding sphere for this node.
        /// </summary>
        /// <returns></returns>
        public BoundingSphere GetBound()
        {
            if (_boundingSphereComputed) return _boundingSphere;
            
            _boundingSphere = _initialBound;

            _boundingSphere.ExpandBy(null != ComputeBoundCallback ? ComputeBoundCallback(this) : ComputeBound());

            _boundingSphereComputed = true;

            return _boundingSphere;
        }

        /// <summary>
        /// Compute the bounding sphere of this geometry
        /// </summary>
        /// <returns></returns>
        public virtual BoundingSphere ComputeBound()
        {
            return new BoundingSphere();
        }
        
        public virtual void Accept(NodeVisitor nv)
        {

            if (nv.ValidNodeMask(this))
            {
                nv.PushOntoNodePath(this);
                nv.Apply(this);
                nv.PopFromNodePath(this);
            };
        }

        public virtual void Ascend(NodeVisitor nv)
        {
            foreach (var parent in _parents)
            {
                parent.Accept(nv);
            }
        }

        // Traverse downward - call children's accept method with Node Visitor
        public virtual void Traverse(NodeVisitor nv)
        {
            // Do nothing by default
        }
       
    }
}