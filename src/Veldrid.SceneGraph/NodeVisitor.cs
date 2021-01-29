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
  
    
    public class NodePath : LinkedList<INode>
    {
        public NodePath Copy()
        {
            var result = new NodePath();
            foreach (var node in this)
            {
                result.AddLast(node);
            }

            return result;
        }
    }
    
    public class NodePathList : List<NodePath> {}

    public class NodeVisitorEventArgs : EventArgs
    {
        public NodeVisitor NodeVisitor { get; set; }
    }

        public interface INodeVisitor : IObject
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

        void Apply(IDrawable drawable);

        void Traverse(INode node);
    }
    
    /// <summary>
    /// Base class for all visitors
    /// </summary>
    public class NodeVisitor : Object, INodeVisitor
    {
        
        public enum TraversalModeType
        {
            TraverseNone,
            TraverseParents,
            TraverseAllChildren,
            TraverseActiveChildren
        };
        
        [Flags]
        public enum VisitorType
        {
            NodeVisitor,
            UpdateVisitor,
            EventVisitor,
            IntersectionVisitor,
            CullVisitor,
            AssembleVisitor,
            CullAndAssembleVisitor = CullVisitor | AssembleVisitor
        };

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
        public uint TraversalMask { get; set; } = 0xffffffff;
        
        /// <summary>
        /// Set the NodeMaskOverride mask.
        /// Used in ValidNodeMask() to determine whether to operate on a node or its
        /// subgraph, by OR'ing NodeVisitor.NodeMaskOverride with the Node's own Node.NodeMask.
        /// Typically used to force on nodes which may have
        /// been switched off by their own Node.NodeMask.*/
        /// </summary>
        public uint NodeMaskOverride { get; set; } = 0x0;

        public VisitorType Type { get; set; }
        
        public TraversalModeType TraversalMode { get; set; }
        
        public NodePath NodePath { get; } = new NodePath();

        protected NodeVisitor(TraversalModeType traversalModeType = TraversalModeType.TraverseNone)
        {
            Type = VisitorType.NodeVisitor;
            TraversalMode = traversalModeType;
        }
        
        protected NodeVisitor(VisitorType type, TraversalModeType traversalMode = TraversalModeType.TraverseNone)
        {
            Type = type;
            TraversalMode = traversalMode;
        }

        public void PushOntoNodePath(INode node)
        {
            if (TraversalMode != TraversalModeType.TraverseParents)
            {
                NodePath.AddLast(node);
            }
            else
            {
                NodePath.AddFirst(node);
            }
        }

        public void PopFromNodePath(INode node)
        {
            if (TraversalMode != TraversalModeType.TraverseParents)
            {
                NodePath.RemoveLast();
            }
            else
            {
                NodePath.RemoveFirst();
            }
        }

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
        public bool ValidNodeMask(INode node)
        {
            return (TraversalMask & (NodeMaskOverride | node.NodeMask)) != 0;
        }
        
        public void Traverse(INode node)
        {
            if (TraversalMode == TraversalModeType.TraverseParents) node.Ascend(this);
            else if(TraversalMode != TraversalModeType.TraverseNone) node.Traverse(this);
        }
        
        //
        // Default implementation for Generic Node
        //
        public virtual void Apply(INode node)
        {
            Traverse(node);
        }
        
        //
        // Default implementation for Geometry Node
        // 
        public virtual void Apply(IGeode geode)
        {
            Apply((INode)geode);
        }

        /// <summary>
        /// Default Implementation for Billboard
        /// </summary>
        /// <param name="billboard"></param>
        public virtual void Apply(IBillboard billboard)
        {
            Apply((IGeode)billboard);
        }

        // 
        // Default implementation for Transform node
        // 
        public virtual void Apply(ITransform transform)
        {
            Apply((INode)transform);
        }

        public virtual void Apply(IDrawable drawable)
        {
            Apply((INode) drawable);
        }
    }
}