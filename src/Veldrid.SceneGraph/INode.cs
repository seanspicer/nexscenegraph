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
        StateSet StateSet { get; set; }
        PipelineState PipelineState { get; set; }
        bool HasPipelineState { get; }
        BoundingSphere InitialBound { get; set; }
        int GetNumChildrenRequiringEventTraversal();
        int GetNumChildrenRequiringUpdateTraversal();
        void SetNumChildrenRequiringEventTraversal(int i);
        void SetNumChildrenRequiringUpdateTraversal(int i);
        event Func<Node, BoundingSphere> ComputeBoundCallback;
        StateSet GetOrCreateStateSet();
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
        BoundingSphere GetBound();

        /// <summary>
        /// Compute the bounding sphere of this geometry
        /// </summary>
        /// <returns></returns>
        BoundingSphere ComputeBound();

        void Accept(NodeVisitor nv);
        void Ascend(NodeVisitor nv);
        void Traverse(NodeVisitor nv);
    }
}