//
// Copyright 2018 Sean Spicer 
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