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

namespace Veldrid.SceneGraph
{
    public interface INodeVisitor
    {
        /// <summary>
        /// Set the TraversalMask of this NodeVisitor.
        /// The TraversalMask is used by the NodeVisitor.ValidNodeMask() method
        /// to determine whether to operate on a node and its subgraph.
        /// ValidNodeMask() is called automatically in the Node.Accept() method before
        /// any call to NodeVisitor.Apply(), Apply() is only ever called if ValidNodeMask
        /// returns true. Note, if NodeVisitor.TraversalMask is 0 then all operations
        /// will be switched off for all nodes.  Whereas setting both TraversalMask and
        /// NodeMaskOverride to 0xffffffff will allow a visitor to work on all nodes
        /// regardless of their own Node.NodeMask state.
        /// </summary>
        uint TraversalMask { get; set; }

        /// <summary>
        /// Set the NodeMaskOverride mask.
        /// Used in ValidNodeMask() to determine whether to operate on a node or its
        /// subgraph, by OR'ing NodeVisitor.NodeMaskOverride with the Node's own Node.NodeMask.
        /// Typically used to force on nodes which may have
        /// been switched off by their own Node.NodeMask.*/
        /// </summary>
        uint NodeMaskOverride { get; set; }

        NodeVisitor.VisitorType Type { get; set; }
        NodeVisitor.TraversalModeType TraversalMode { get; set; }
        NodePath NodePath { get; }
        void PushOntoNodePath(INode node);
        void PopFromNodePath(INode node);

        /// <summary>
        /// Method called by Node and its subclass' Node.Accept() method, if the result is true
        /// it is used to cull operations of nodes and their subgraphs.
        /// 
        /// Return true if the result of a bit wise and of the NodeVisitor.TraversalMask
        /// with the bit or between NodeVistor.NodeMaskOverride and the Node.NodeMask.
        /// default values for TraversalMask is 0xffffffff, NodeMaskOverride is 0x0,
        /// and Node.NodeMask is 0xffffffff.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool ValidNodeMask(INode node);

        void Apply(INode node);
        void Apply(IGeode geode);

        /// <summary>
        /// Default Implementation for Billboard
        /// </summary>
        /// <param name="billboard"></param>
        void Apply(IBillboard billboard);

        void Apply(ITransform transform);
    }
}