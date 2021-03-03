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

namespace Veldrid.SceneGraph.Util
{
    public class UpdateVisitor : NodeVisitor, IUpdateVisitor
    {
        protected UpdateVisitor() :
            base(VisitorType.UpdateVisitor, TraversalModeType.TraverseAllChildren)
        {
        }

        public override void Apply(INode node)
        {
            HandleCallbacksAndTraverse(node);
        }

        public override void Apply(IGeode geode)
        {
            base.Apply(geode);
        }

        public override void Apply(ITransform transform)
        {
            base.Apply(transform);
        }

        public override void Apply(IBillboard billboard)
        {
            base.Apply(billboard);
        }

        public override void Apply(IDrawable drawable)
        {
            base.Apply(drawable);
        }

        public static IUpdateVisitor Create()
        {
            return new UpdateVisitor();
        }

        protected void HandleCallbacks(IPipelineState state)
        {
            // TODO Handle state updates.
        }

        protected void HandleCallbacksAndTraverse(INode node)
        {
            if (node.HasPipelineState) HandleCallbacks(node.PipelineState);

            var callback = node.GetUpdateCallback();
            if (null != callback)
                callback.Run(node, this);
            else
                // TODO - this should be:
                // if(node.GetNumChildrenRequiringUpdateTraversal() > 0) Traverse(node);
                Traverse(node);
        }
    }
}