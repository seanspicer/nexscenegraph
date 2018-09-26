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
using System.Collections.Generic;
using System.Data;
using System.Numerics;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class RenderGroupElement
    {
        public Drawable Drawable;
        public Matrix4x4 ModelMatrix;
        
        // TODO - do these really belong here?
        public DeviceBuffer VertexBuffer { get; set; }
        public DeviceBuffer IndexBuffer { get; set; }
    }
    
    public class RenderGroup
    {
        public bool HasDrawableElements()
        {
            return RenderGroupStateCache.Count > 0;
        }
        
        private Dictionary<Tuple<PipelineState, PrimitiveTopology, VertexLayoutDescription>, RenderGroupState> RenderGroupStateCache;

        public RenderGroup()
        {
            RenderGroupStateCache = new Dictionary<Tuple<PipelineState, PrimitiveTopology, VertexLayoutDescription>, RenderGroupState>();
        }

        public void Clear()
        {
            RenderGroupStateCache.Clear();
        }
        
        public IEnumerable<RenderGroupState> GetStateList()
        {
            return RenderGroupStateCache.Values;
        }
        
        public RenderGroupState GetOrCreateState(PipelineState pso, PrimitiveTopology pt, VertexLayoutDescription vl)
        {
            var key = new Tuple<PipelineState, PrimitiveTopology, VertexLayoutDescription>(pso, pt, vl);
            if (RenderGroupStateCache.TryGetValue(key, out var renderGroupState)) return renderGroupState;
            
            renderGroupState = new RenderGroupState(pso, pt, vl);
            RenderGroupStateCache.Add(key, renderGroupState);

            return renderGroupState;
        }
    }
}