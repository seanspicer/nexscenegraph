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

namespace Veldrid.SceneGraph
{
    /// <summary>
    /// This is the base interface for all nodes in Veldrid.SceneGraph
    /// </summary>
    public interface INode : IObject
    {
        /// <summary>
        /// This nodes globally unique identifier
        /// </summary>
        Guid Id { get; }
        
        /// <summary>
        /// This node's NodeMask
        /// </summary>
        uint NodeMask { get; }
        
        /// <summary>
        /// This node's NameString'
        /// </summary>
        string NameString { get; }
        
        /// <summary>
        /// Get the number of parents of this node
        /// </summary>
        int NumParents { get; }
        
        /// <summary>
        /// Is Culling Active for this node?
        /// </summary>
        bool CullingActive { get; }
        
        /// <summary>
        /// This checks the culling active property and child properties.
        /// </summary>
        bool IsCullingActive { get; }
        
        /// <summary>
        /// Get the number of children of this node with culling disabled
        /// </summary>
        int NumChildrenWithCullingDisabled { get;}
        
        /// <summary>
        /// Get the pipeline state for this node
        /// </summary>
        IPipelineState PipelineState { get; }
        
        /// <summary>
        /// Determine if this node has a pipeline state attached
        /// </summary>
        bool HasPipelineState { get; }
        
        /// <summary>
        /// The initial bound of this node
        /// </summary>
        IBoundingSphere InitialBound { get;}
        
        /// <summary>
        /// Get the number of children requiring event traversal
        /// </summary>
        /// <returns></returns>
        int GetNumChildrenRequiringEventTraversal();
        
        /// <summary>
        /// Get the number of children requiring update traversal
        /// </summary>
        /// <returns></returns>
        int GetNumChildrenRequiringUpdateTraversal();
        
        [Obsolete("This needs to be changed")]
        event Func<Node, BoundingSphere> ComputeBoundCallback;
        
        /// <summary>
        /// Get this node's update callback
        /// </summary>
        /// <returns></returns>
        ICallback GetUpdateCallback();
        
        /// <summary>
        /// Get this node's cull callback
        /// </summary>
        /// <returns></returns>
        ICallback GetCullCallback();
        
        /// <summary>
        /// Get the bounding sphere for this node.
        /// </summary>
        /// <returns></returns>
        IBoundingSphere GetBound();
        
        /// <summary>
        /// Accept a node visitor
        /// </summary>
        /// <param name="nv"></param>
        void Accept(INodeVisitor nv);
        
        /// <summary>
        /// Ascend the node tree
        /// </summary>
        /// <param name="nv"></param>
        void Ascend(INodeVisitor nv);
        
        /// <summary>
        /// Traverse the node tree
        /// </summary>
        /// <param name="nv"></param>
        void Traverse(INodeVisitor nv);
        
        /// <summary>
        /// Get a mutable object for this node
        /// </summary>
        /// <returns></returns>
        IMutableNode GetMutable();
    }
    
    public interface IMutableNode : IMutableObject
    {
        /// <summary>
        /// Set this node's nodemask'
        /// </summary>
        /// <param name="nodeMask"></param>
        void SetNodeMask(uint nodeMask);
        
        /// <summary>
        /// Set this node's name string
        /// </summary>
        /// <param name="nameString"></param>
        void SetNameString(string nameString);
        
        /// <summary>
        /// Set culling active for this node
        /// </summary>
        /// <param name="cullingActive"></param>
        void SetCullingActive(bool cullingActive);
        
        /// <summary>
        /// Set the pipeline state for this node
        /// </summary>
        /// <param name="pipelineState"></param>
        void SetPipelineState(IPipelineState pipelineState);

        /// <summary>
        /// Set this node's initial bound
        /// </summary>
        /// <param name="boundingSphere"></param>
        void SetInitialBound(IBoundingSphere boundingSphere);
        
        /// <summary>
        /// Set the number of children requiring event traversal
        /// </summary>
        /// <param name="i"></param>
        void SetNumChildrenRequiringEventTraversal(int i);
        
        /// <summary>
        /// Set the number of children requiring update traversal
        /// </summary>
        /// <param name="i"></param>
        void SetNumChildrenRequiringUpdateTraversal(int i);
        
        /// <summary>
        /// Set the update callback for this node
        /// </summary>
        /// <param name="callback"></param>
        void SetUpdateCallback(ICallback callback);
        
        /// <summary>
        /// Set the cull callback for this node
        /// </summary>
        /// <param name="callback"></param>
        void SetCullCallback(ICallback callback);
        
        /// <summary>
        /// Add a parent to this node
        /// </summary>
        /// <param name="parent"></param>
        void AddParent(IGroup parent);
        
        /// <summary>
        /// Remove a parent from this node
        /// </summary>
        /// <param name="parent"></param>
        void RemoveParent(IGroup parent);
        
        /// <summary>
        /// Mark this node's bounding sphere dirty.  Forcing it to be computed on the next call
        /// to GetBound();
        /// </summary>
        void DirtyBound();
    }
}