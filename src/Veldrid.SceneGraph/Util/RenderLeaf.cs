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

using System.Numerics;

namespace Veldrid.SceneGraph.Util
{
    public class RenderLeaf
    {
        public StateGraph Parent { get; set; } = null;
        private Drawable Drawable { get; set; } = null;
        private Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        private Matrix4x4 ModelView { get; set; } = Matrix4x4.Identity;
        private float Depth { get; set; } = 0.0f;
        public bool Dynamic { get; set; } = false;
        private uint TraversalOrderNumber { get; set; } = 0;

        private RenderLeaf()
        {
            // Disallow default construction (not useful)
        }

        private RenderLeaf(RenderLeaf rl)
        {
            // Disallow Copy Construction
        }

        public RenderLeaf(Drawable drawable, Matrix4x4 projection, Matrix4x4 modelView, float depth = 0.0f,
            uint traversalOrderNumber = 0)
        {
            Parent = null;
            Drawable = drawable;
            Projection = projection;
            ModelView = modelView;
            Depth = depth;
            Dynamic = (drawable.DataVariance == Object.DataVarianceType.Dynamic);
            TraversalOrderNumber = traversalOrderNumber;
        }

        public void Set(Drawable drawable, Matrix4x4 projection, Matrix4x4 modelView, float depth = 0.0f,
            uint traversalOrderNumber = 0)
        {
            Parent = null;
            Drawable = drawable;
            Projection = projection;
            ModelView = modelView;
            Depth = depth;
            Dynamic = (drawable.DataVariance == Object.DataVarianceType.Dynamic);
            TraversalOrderNumber = traversalOrderNumber;
        }

        public void Reset()
        {
            Parent = null;
            Drawable = null;
            Projection = Matrix4x4.Identity;
            ModelView = Matrix4x4.Identity;
            Depth = 0.0f;
            Dynamic = false;
            TraversalOrderNumber = 0;
        }

        public virtual void Render(RenderInfo renderInfo, RenderLeaf previous)
        {
            var state = renderInfo.State;

            if (state.AbortRendering)
            {
                return;
            }

            if (null != previous)
            {
                state.ApplyProjectionMatrix(Projection);
                state.ApplyModelViewMatrix(ModelView);

                var previousRenderGraph = previous.Parent;
                var previousRenderGraphParent = previousRenderGraph.Parent;
                var renderGraph = Parent;
                
                if (previousRenderGraphParent != renderGraph.Parent)
                {
                    StateGraph.MoveStateGraph(state, previousRenderGraphParent, renderGraph.Parent);
                    
                    state.Apply(renderGraph.StateSet);
                }
                else if (renderGraph != previousRenderGraph)
                {
                    state.Apply(renderGraph.StateSet);
                }
                
                state.ApplyModelViewAndProjectionUniformsIfRequired();

                // Issue Drawing Commands
                //Drawable.Draw(renderInfo);
            }
            else
            {
                state.ApplyProjectionMatrix(Projection);
                state.ApplyModelViewMatrix(ModelView);
                
                StateGraph.MoveStateGraph(state, null, Parent.Parent);
                
                state.Apply(Parent.StateSet);
                
                state.ApplyModelViewAndProjectionUniformsIfRequired();
                
                // Issue Drawing Commands
                //Drawable.Draw(renderInfo);
                
            }

            if (Dynamic)
            {
                state.DecrementDynamicObjectCount();
            }
        }
    }
}