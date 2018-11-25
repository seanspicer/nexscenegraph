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