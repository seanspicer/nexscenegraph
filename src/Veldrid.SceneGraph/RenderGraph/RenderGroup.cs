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
    public struct RenderGroupElement
    {
        public List<IPrimitiveSet> PrimitiveSets;
        public Matrix4x4 ModelViewMatrix;
        public DeviceBuffer VertexBuffer;
        public DeviceBuffer IndexBuffer;
    }
    
    public class RenderGroup : IRenderGroup
    {
        public bool HasDrawableElements()
        {
            return RenderGroupStateCache.Count > 0;
        }
        
        private Dictionary<Tuple<IPipelineState, PrimitiveTopology, VertexLayoutDescription>, IRenderGroupState> RenderGroupStateCache;

        public static IRenderGroup Create()
        {
            return new RenderGroup();
        }
        
        protected RenderGroup()
        {
            RenderGroupStateCache = new Dictionary<Tuple<IPipelineState, PrimitiveTopology, VertexLayoutDescription>, IRenderGroupState>();
        }

        public void Reset()
        {
            foreach (var rgs in RenderGroupStateCache.Values)
            {
                rgs.Elements.Clear();
            }
        }
        
        public IEnumerable<IRenderGroupState> GetStateList()
        {
            return RenderGroupStateCache.Values;
        }
        
        public IRenderGroupState GetOrCreateState(IPipelineState pso, PrimitiveTopology pt, VertexLayoutDescription vl)
        {
            var key = new Tuple<IPipelineState, PrimitiveTopology, VertexLayoutDescription>(pso, pt, vl);
            if (RenderGroupStateCache.TryGetValue(key, out var renderGroupState)) return renderGroupState;
            
            renderGroupState = RenderGroupState.Create(pso, pt, vl);
            RenderGroupStateCache.Add(key, renderGroupState);

            return renderGroupState;
        }
    }
}