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
using System.Numerics;
using Vulkan.Win32;

namespace Veldrid.SceneGraph
{
    public interface IPrimitiveFunctor
    {
        IPrimitiveElement[] VertexData { get; set; }
        void Draw(
            PrimitiveTopology topology, 
            uint indexCount, 
            uint instanceCount, 
            uint indexStart, 
            int vertexOffset, 
            uint instanceStart);
    }
    
    public abstract class PrimitiveFunctor : IPrimitiveFunctor
    {
        public IPrimitiveElement[] VertexData { get; set; }
        
        public abstract void Draw(
            PrimitiveTopology topology, 
            uint indexCount, 
            uint instanceCount, 
            uint indexStart, 
            int vertexOffset, 
            uint instanceStart);
    }

    public interface IPrimitiveFunctorDelegate
    {
        void Handle(Vector3 v0, Vector3 v1, Vector3 v2);
    }

    public class TemplatePrimitiveFunctor : PrimitiveFunctor
    {
        private IPrimitiveFunctorDelegate _pfd;

        public IPrimitiveFunctor Create(IPrimitiveFunctorDelegate pfd)
        {
            return new TemplatePrimitiveFunctor(pfd);
        }

        protected TemplatePrimitiveFunctor(IPrimitiveFunctorDelegate pfd)
        {
            _pfd = pfd;
        }
        
        public override void Draw(PrimitiveTopology topology, uint indexCount, uint instanceCount, uint indexStart, int vertexOffset,
            uint instanceStart)
        {
            if (null == _pfd) return;

            switch (topology)
            {
                case PrimitiveTopology.TriangleList:
                {
                    throw new NotImplementedException("TODO: Implement me!");
                    break;
                }
                    
                default:
                    break;
            }
        }
    }
}