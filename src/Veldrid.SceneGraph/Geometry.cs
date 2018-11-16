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
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Veldrid.SceneGraph
{    
    public class Geometry<T> : Drawable, IGeometry<T> where T : struct, IPrimitiveElement
    {
        public T[] VertexData { get; set; }
        public int SizeOfVertexData => Marshal.SizeOf(default(T));
        
        public ushort[] IndexData { get; set; }
        
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
                new BufferDescription((uint) (IndexData.Length * sizeof(ushort)), BufferUsage.IndexBuffer);
            var ibo = factory.CreateBuffer(idxBufferDesc);
            device.UpdateBuffer(ibo, 0, IndexData);

            _vertexBufferCache.Add(device, vbo);
            _indexBufferCache.Add(device, ibo);
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