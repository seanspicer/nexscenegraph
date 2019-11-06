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
using Veldrid.SceneGraph.RenderGraph;
using Veldrid.SceneGraph.Util;
using Veldrid.SceneGraph.Viewer;

namespace Veldrid.SceneGraph
{
    public interface INode : IObject
    {
        Guid Id { get; }
        uint NodeMask { get; set; }
        string NameString { get; set; }
        int NumParents { get; }
        bool CullingActive { get; set; }
        int NumChildrenWithCullingDisabled { get; set; }
        bool IsCullingActive { get; }
        IPipelineState PipelineState { get; set; }
        bool HasPipelineState { get; }
        IBoundingSphere InitialBound { get; set; }
        int GetNumChildrenRequiringEventTraversal();
        int GetNumChildrenRequiringUpdateTraversal();
        void SetNumChildrenRequiringEventTraversal(int i);
        void SetNumChildrenRequiringUpdateTraversal(int i);
        event Func<Node, BoundingSphere> ComputeBoundCallback;

        void SetUpdateCallback(Action<INodeVisitor, INode> callback);
        Action<INodeVisitor, INode> GetUpdateCallback();
        
        void SetCullCallback(Action<INodeVisitor, INode> callback);
        Action<INodeVisitor, INode> GetCullCallback();
        
        
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
}