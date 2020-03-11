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
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Veldrid.SceneGraph
{    
    public interface IGeometry<T> : IDrawable where T : struct, IPrimitiveElement
    {
        T[] VertexData { get; set; }
        uint[] IndexData { get; set; }
    }
    
    public class Geometry<T> : Drawable, IGeometry<T> where T : struct, IPrimitiveElement
    {
        public T[] VertexData { get; set; }
        private int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public uint[] IndexData { get; set; }
        
        private Dictionary<GraphicsDevice, DeviceBuffer> _vertexBufferCache 
            = new Dictionary<GraphicsDevice, DeviceBuffer>();
        
        private Dictionary<GraphicsDevice, DeviceBuffer> _indexBufferCache 
            = new Dictionary<GraphicsDevice, DeviceBuffer>();
        
        protected Geometry()
        {
        }

        public static IGeometry<T> Create()
        {
            return new Geometry<T>();
        }

        public override void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            if (_vertexBufferCache.ContainsKey(device) && _indexBufferCache.ContainsKey(device)) return;
            
            var vtxBufferDesc =
                new BufferDescription((uint) (VertexData.Length * SizeOfVertexData), BufferUsage.VertexBuffer);
            var vbo = factory.CreateBuffer(vtxBufferDesc);
            device.UpdateBuffer(vbo, 0, VertexData);

            
            
            var idxBufferDesc =
                new BufferDescription((uint) (IndexData.Length * sizeof(uint)), BufferUsage.IndexBuffer);
            var ibo = factory.CreateBuffer(idxBufferDesc);
            device.UpdateBuffer(ibo, 0, IndexData);

            _vertexBufferCache.Add(device, vbo);
            _indexBufferCache.Add(device, ibo);
        }


        protected override Type GetVertexType()
        {
            return typeof(T);
        }

        protected override void DrawImplementation(GraphicsDevice device, List<Tuple<uint, ResourceSet>> resourceSets, CommandList commandList)
        {
            foreach (var primitiveSet in PrimitiveSets)
            {                
                primitiveSet.Draw(commandList);
            }
        }
        
        protected override IBoundingBox ComputeBoundingBox()
        {
            var bb = BoundingBox.Create();
            foreach (var pset in PrimitiveSets)
            {
                bb.ExpandBy(pset.GetBoundingBox());
            }

            return bb;
        }

        public override DeviceBuffer GetVertexBufferForDevice(GraphicsDevice device)
        {
            if (_vertexBufferCache.ContainsKey(device))
            {
                return _vertexBufferCache[device];
            }
            else
            {
                throw new Exception("No vertex buffer for device");
            }
        }
        
        public override DeviceBuffer GetIndexBufferForDevice(GraphicsDevice device)
        {
            if (_indexBufferCache.ContainsKey(device))
            {
                return _indexBufferCache[device];
            }
            else
            {
                throw new Exception("No index buffer for device");
            }
        }
    }
}