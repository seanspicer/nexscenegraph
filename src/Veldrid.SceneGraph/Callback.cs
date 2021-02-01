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

using SixLabors.ImageSharp.Processing;
using Veldrid.SceneGraph.RenderGraph;

namespace Veldrid.SceneGraph
{
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 
    public interface ICallback
    {
        bool Run(IObject obj, IObject data);
        
        ICallback NestedCallback { get; set; }
    }

    public abstract class Callback : ICallback
    {
        public ICallback NestedCallback { get; set; } = null;
        
        public virtual bool Run(IObject obj, IObject data)
        {
            return Traverse(obj, data);
        }

        public bool Traverse(IObject obj, IObject data)
        {
            if (null != NestedCallback) return NestedCallback.Run(obj, data);
            
            if (obj is INode node && data is INodeVisitor nodeVisitor)
            {
                nodeVisitor.Traverse(node);
                return true;
            }

            return false;
        }
    }
    
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 
    public interface INodeCallback : ICallback
    {
        void Execute(INode node, INodeVisitor nodeVisitor);
    }
    public abstract class NodeCallback : Callback, INodeCallback
    {
        public override bool Run(IObject obj, IObject data)
        {
            if (obj is INode node && data is INodeVisitor nodeVisitor)
            {
                Execute(node, nodeVisitor);
                return true;
            }
            else
            {
                return Traverse(obj, data);
            }
        }

        public virtual void Execute(INode node, INodeVisitor nodeVisitor)
        {
            Traverse(node, nodeVisitor);
        }
    }
    
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 
    public interface IDrawableCullCallback : ICallback
    {
        bool Cull(INodeVisitor nodeVisitor, IDrawable drawable);
    }

    public abstract class DrawableCullCallback : Callback, IDrawableCullCallback
    {
        public virtual bool Cull(INodeVisitor nodeVisitor, IDrawable drawable)
        {
            return false;
        }
    }
    
    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@ 
    public interface IDrawableEventCallback : INodeCallback
    {
        void Event(INodeVisitor nodeVisitor, IDrawable drawable);
    }
    
    public abstract class DrawableEventCallback : NodeCallback, IDrawableEventCallback
    {
        public override bool Run(IObject obj, IObject data)
        {
            if (obj is IDrawable drawable && data is INodeVisitor nodeVisitor)
            {
                Event(nodeVisitor, drawable);
                return true;
            }
            else
            {
                return Traverse(obj, data);
            }
        }

        public void Event(INodeVisitor nodeVisitor, IDrawable drawable) {}
    }
}