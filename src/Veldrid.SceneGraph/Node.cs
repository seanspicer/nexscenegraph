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
using System.Linq;
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    public class CollectParentPaths : NodeVisitor
    {
        private INode _haltTraversalAtNode;
        private NodePathList _nodePaths;
        
        public NodePathList NodePaths
        {
            get => _nodePaths;
        }
        
        public CollectParentPaths(INode haltTraversalAtNode = null) :
            base(VisitorType.NodeVisitor, TraversalModeType.TraverseParents)
        {
            _haltTraversalAtNode = haltTraversalAtNode;
            _nodePaths = new NodePathList();
        }

        public override void Apply(INode node)
        {
            if (node.NumParents == 0 || node == _haltTraversalAtNode)
            {
                _nodePaths.Add(NodePath.Copy());
            }
            else
            {
                Traverse(node);
            }
        }
    }
    
    public interface INode : IObject
    {
        Guid Id { get; }
        uint NodeMask { get; set; }
        string NameString { get; set; }
        int NumParents { get; }
        bool CullingActive { get; set; }
        uint NumChildrenWithCullingDisabled { get; set; }
        bool IsCullingActive { get; }
        IPipelineState PipelineState { get; set; }
        bool HasPipelineState { get; }
        IBoundingSphere InitialBound { get; set; }
        int GetNumChildrenRequiringEventTraversal();
        int GetNumChildrenRequiringUpdateTraversal();
        void SetNumChildrenRequiringEventTraversal(int i);
        void SetNumChildrenRequiringUpdateTraversal(int i);
        NodePathList GetParentalNodePaths(INode haltTraversalAtNode=null);
        event Func<Node, BoundingSphere> ComputeBoundCallback;

        void SetUpdateCallback(ICallback callback);
        ICallback GetUpdateCallback();
        
        void SetCullCallback(ICallback callback);
        ICallback GetCullCallback();
        
        
        void AddParent(IGroup parent);
        void RemoveParent(IGroup parent);

        /// <summary>
        /// Mark this node's bounding sphere dirty.  Forcing it to be computed on the next call
        /// to GetBound();
        /// </summary>
        void DirtyBound();

        /// <summary>
        /// Get the bounding sphere for this node.
        /// </summary>
        /// <returns></returns>
        IBoundingSphere GetBound();

        /// <summary>
        /// Compute the bounding sphere of this geometry
        /// </summary>
        /// <returns></returns>
        IBoundingSphere ComputeBound();

        void Accept(INodeVisitor nv);
        void Ascend(INodeVisitor nv);
        void Traverse(INodeVisitor nv);
    }
   
    public abstract class Node : Object, INode
    {
        // Public Fields
        public Guid Id { get; private set; }
        public uint NodeMask { get; set; } = 0xffffffff;

        public string NameString { get; set; } = string.Empty;
        
        public int NumParents => _parents.Count;

        private bool _cullingActive = true;
        
        public bool CullingActive
        {
            get => _cullingActive;
            set
            {
                if (_cullingActive == value) return;
                
                // culling active has been changed, will need to update
                // both _cullActive and possibly the parents numChildrenWithCullingDisabled
                // if culling disabled changes.

                // update the parents _numChildrenWithCullingDisabled
                // note, if _numChildrenWithCullingDisabled!=0 then the
                // parents won't be affected by any app callback change,
                // so no need to inform them.
                if (_numChildrenWithCullingDisabled==0 && _parents.Any())
                {
                    var delta = 0u;
                    if (!_cullingActive) --delta;
                    if (!value) ++delta;
                    if (delta!=0)
                    {
                        // the number of callbacks has changed, need to pass this
                        // on to parents so they know whether app traversal is
                        // required on this subgraph.
                        foreach (var parent in _parents)
                        {
                            parent.NumChildrenWithCullingDisabled += delta;
                        }
                    }
                }
                
                _cullingActive = value;
            }
        }

        private uint _numChildrenWithCullingDisabled = 0;

        public uint NumChildrenWithCullingDisabled
        {
            get => _numChildrenWithCullingDisabled;
            set
            {
                // if no changes just return.
                if (_numChildrenWithCullingDisabled==value) return;

                // note, if _cullingActive is false then the
                // parents won't be affected by any changes to
                // _numChildrenWithCullingDisabled so no need to inform them.
                if (_cullingActive && _parents.Any())
                {

                    // need to pass on changes to parents.
                    uint delta = 0;
                    if (_numChildrenWithCullingDisabled>0) --delta;
                    if (value>0) ++delta;
                    if (delta!=0)
                    {
                        // the number of callbacks has changed, need to pass this
                        // on to parents so they know whether app traversal is
                        // required on this subgraph.
                        foreach (var parent in _parents)
                        {
                            parent.NumChildrenWithCullingDisabled += delta;
                        }
                    }
                }

                // finally update this objects value.
                _numChildrenWithCullingDisabled=value;
            }
        }

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

        private ICallback _updateCallback;
        
        public virtual void SetUpdateCallback(ICallback callback)
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

        public NodePathList GetParentalNodePaths(INode haltTraversalAtNode=null)
        {
            var collectParentsVisitor = new CollectParentPaths(haltTraversalAtNode);
            Accept(collectParentsVisitor);
            return collectParentsVisitor.NodePaths;
        }
        
        public virtual ICallback GetUpdateCallback()
        {
            return _updateCallback;
        }
        
        private ICallback _cullCallback;
        
        public virtual void SetCullCallback(ICallback callback)
        {
            _cullCallback = callback;
        }
        
        public virtual ICallback GetCullCallback()
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