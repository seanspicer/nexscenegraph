//
// Copyright 2018-2021 Sean Spicer 
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
using System.Linq;
using System.Numerics;

namespace Veldrid.SceneGraph.RenderGraph
{
    public struct RenderGroupElement
    {
        public List<IPrimitiveSet> PrimitiveSets;
        public Matrix4x4 ModelViewMatrix;
        public List<DeviceBuffer> VertexBuffers;
        public DeviceBuffer IndexBuffer;
    }

    public interface IRenderGroup
    {
        bool HasDrawableElements();
        void Reset();
        IEnumerable<IRenderGroupState> GetStateList();

        IRenderGroupState GetOrCreateState(GraphicsDevice device, IPipelineState pso, PrimitiveTopology pt,
            IDrawable drawable);
    }

    public class RenderGroup : IRenderGroup
    {
        private readonly Dictionary<Tuple<IPipelineState, PrimitiveTopology, string>, List<IRenderGroupState>>
            RenderGroupStateCache;

        protected RenderGroup()
        {
            RenderGroupStateCache =
                new Dictionary<Tuple<IPipelineState, PrimitiveTopology, string>, List<IRenderGroupState>>();
        }

        public bool HasDrawableElements()
        {
            return RenderGroupStateCache.Count > 0;
        }

        public void Reset()
        {
            // TODO - maybe implement an LRU cache here so that the size of the cache doesn't
            // Grow indefinitely as a user navigates a scene.
            foreach (var rgs in GetStateList()) rgs.Elements.Clear();

            // This call is *crazy* expensive
            //RenderGroupStateCache.Clear();
        }

        public IEnumerable<IRenderGroupState> GetStateList()
        {
            foreach (var renderGroupStateList in RenderGroupStateCache.Values)
            foreach (var renderGroupState in renderGroupStateList)
                yield return renderGroupState;
        }

        public IRenderGroupState GetOrCreateState(GraphicsDevice device, IPipelineState pso, PrimitiveTopology pt,
            IDrawable drawable)
        {
            var modelOffset = 64u;
            //TODO: fixme!
            //if (device.UniformBufferMinOffsetAlignment > 64) 
            modelOffset = 256;

            var maxAllowedDrawables = 65536u / modelOffset;

            var key = new Tuple<IPipelineState, PrimitiveTopology, string>(pso, pt, drawable.VertexLayoutsDescription);
            if (RenderGroupStateCache.TryGetValue(key, out var renderGroupStateList))
            {
                // Check to see if this state list can accept any more drawables, if not, allocate a new one
                foreach (var renderGroupState in renderGroupStateList)
                    if (renderGroupState.Elements.Count < maxAllowedDrawables)
                        return renderGroupState;

                renderGroupStateList.Add(RenderGroupState.Create(pso, pt, drawable.VertexLayouts));
                return renderGroupStateList.Last();
            }

            renderGroupStateList = new List<IRenderGroupState>
                {RenderGroupState.Create(pso, pt, drawable.VertexLayouts)};
            RenderGroupStateCache.Add(key, renderGroupStateList);

            return renderGroupStateList.Last();
        }

        public static IRenderGroup Create()
        {
            return new RenderGroup();
        }
    }
}