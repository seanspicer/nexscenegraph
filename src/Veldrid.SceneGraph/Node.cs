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
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    public class CollectParentPaths : NodeVisitor
    {
        private INode _haltTraversalAtNode;
        private List<LinkedList<INode>> _nodePaths;
        
        public IReadOnlyList<LinkedList<INode>> NodePaths
        {
            get => _nodePaths;
        }
        
        public CollectParentPaths(INode haltTraversalAtNode = null) :
            base(VisitorType.NodeVisitor, TraversalModeType.TraverseParents)
        {
            _haltTraversalAtNode = haltTraversalAtNode;
            _nodePaths = new List<LinkedList<INode>>();
        }

        public override void Apply(INode node)
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
   
    public abstract class Node : Object, INode
    {
        // Public Fields
        public Guid Id { get; private set; }
        public uint NodeMask { get; set; } = 0xffffffff;

        public string NameString { get; set; } = string.Empty;
        
        public int NumParents => _parents.Count;

        public bool CullingActive { get; set; } = true;
        public int NumChildrenWithCullingDisabled { get; set; } = 0;

        public bool IsCullingActive => NumChildrenWithCullingDisabled == 0 && CullingActive && GetBound().Valid();
           
        private IPipelineState _pipelineState = null;
        public IPipelineState PipelineState
        {
            get => _pipelineState ?? (_pipelineState = Veldrid.SceneGraph.PipelineState.Create());
            set => _pipelineState = value;
        }
        
        public bool HasPipelineState
        {
            get => null != _pipelineState;
        }

        public int GetNumChildrenRequiringEventTraversal()
        {
            throw new NotImplementedException();
        }

        private int _numChildrentRequiringUpdateTraversal = 0;
        public int GetNumChildrenRequiringUpdateTraversal()
        {
            return _numChildrentRequiringUpdateTraversal;
        }

        public void SetNumChildrenRequiringEventTraversal(int i)
        {
            throw new NotImplementedException();
        }

        public void SetNumChildrenRequiringUpdateTraversal(int i)
        {            
            _numChildrentRequiringUpdateTraversal = i;
        }

        // Protected/Private fields
        private List<IGroup> _parents;
        protected bool _boundingSphereComputed = false;
        protected IBoundingSphere _boundingSphere = BoundingSphere.Create();

       
        private IBoundingSphere _initialBound = BoundingSphere.Create();
        public IBoundingSphere InitialBound
        {
            get { return _initialBound; }
            set
            {
                _initialBound = value;
                DirtyBound();
            }
        } 

        public event Func<Node, BoundingSphere> ComputeBoundCallback;

        private Action<INodeVisitor, INode> _updateCallback;
        
        public virtual void SetUpdateCallback(Action<INodeVisitor, INode> callback)
        {
            _updateCallback = callback;

//            var collectParentsVisitor = new CollectParentPaths();
//            Accept(collectParentsVisitor);
//            
//            foreach (var parentNodePath in collectParentsVisitor.NodePaths)
//            {
//                foreach(var parentNode in parentNodePath)
//                {
//                    parentNode.SetNumChildrenRequiringUpdateTraversal(parentNode.GetNumChildrenRequiringUpdateTraversal()+1);
//                }
//            }
        }
        
        public virtual Action<INodeVisitor, INode> GetUpdateCallback()
        {
            return _updateCallback;
        }
        
        private Action<INodeVisitor, INode> _cullCallback;
        
        public virtual void SetCullCallback(Action<INodeVisitor, INode> callback)
        {
            _cullCallback = callback;
        }
        
        public virtual Action<INodeVisitor, INode> GetCullCallback()
        {
            return _cullCallback;
        }

        protected Node()
        {
            Id = Guid.NewGuid();
            _updateCallback = null;
            
            _parents = new List<IGroup>();
        }

        public void AddParent(IGroup parent)
        {
            _parents.Add(parent);
        }

        public void RemoveParent(IGroup parent)
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
        public IBoundingSphere GetBound()
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
        public virtual IBoundingSphere ComputeBound()
        {
            return BoundingSphere.Create();
        }
        
        public virtual void Accept(INodeVisitor nv)
        {
            if (nv.ValidNodeMask(this))
            {
                nv.PushOntoNodePath(this);
                nv.Apply(this);
                nv.PopFromNodePath(this);
            };
        }

        public virtual void Ascend(INodeVisitor nv)
        {
            foreach (var parent in _parents)
            {
                parent.Accept(nv);
            }
        }

        // Traverse downward - call children's accept method with Node Visitor
        public virtual void Traverse(INodeVisitor nv)
        {
            // Do nothing by default
            
            
        }
    }
}