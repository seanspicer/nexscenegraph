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
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Vulkan;

namespace Veldrid.SceneGraph
{
    public interface IGeometry : IDrawable
    {
        bool IndexOfVertex(Vector3 vertex, out uint index);
    }
    
    public interface IGeometry<T> : IGeometry where T : struct, IPrimitiveElement
    {
        T[] VertexData { get; set; }
        uint[] IndexData { get; set; }

        IVertexBuffer InstanceVertexBuffer { get; set; }
    }
    
    public class Geometry<T> : Drawable, IGeometry<T> where T : struct, IPrimitiveElement
    {
        private T[] _vertexData;

        public T[] VertexData
        {
            get => _vertexData;
            set
            {
                _vertexBufferCache.Clear();
                _vertexData = value;
            }
        }
        private int SizeOfVertexData => Marshal.SizeOf(default(T));

        public IVertexBuffer InstanceVertexBuffer { get; set; }

        private uint[] _indexData;
        public uint[] IndexData
        {
            get => _indexData;
            set
            {
                _indexBufferCache.Clear();
                _indexData = value;
            }
        }
        
        private Dictionary<GraphicsDevice, List<DeviceBuffer>> _vertexBufferCache 
            = new Dictionary<GraphicsDevice, List<DeviceBuffer>>();
        
        private Dictionary<GraphicsDevice, DeviceBuffer> _indexBufferCache 
            = new Dictionary<GraphicsDevice, DeviceBuffer>();
        
        protected Geometry()
        {
        }

        public static IGeometry<T> Create()
        {
            return new Geometry<T>();
        }

        // public T[] GetVertexArray()
        // {
        //     return VertexData;
        //     
        //     var result = new Vector3[VertexData.Length];
        //     for(var i=0; i<VertexData.Length; ++i)
        //     {
        //         result[i] = VertexData[i].VertexPosition;
        //     }
        //
        //     return result;
        // }

        public bool IndexOfVertex(Vector3 vertex, out uint index)
        {
            index = 0;
            foreach (var val in VertexData)
            {
                if (val.VertexPosition == vertex)
                {
                    return true;
                }
                index++;
            }

            return false;
        }
        
        public override void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            if (_vertexBufferCache.ContainsKey(device) && _indexBufferCache.ContainsKey(device))
            {
                return;
            }
            
            var vertexBuffers = new List<DeviceBuffer>();
            
            var vtxBufferDesc =
                new BufferDescription((uint) (VertexData.Length * SizeOfVertexData), BufferUsage.VertexBuffer);
            var vbo = factory.CreateBuffer(vtxBufferDesc);
            device.UpdateBuffer(vbo, 0, VertexData);

            vertexBuffers.Add(vbo);
            
            if (null != InstanceVertexBuffer)
            {
                InstanceVertexBuffer.ConfigureDeviceBuffers(device, factory);
                var ivbo = InstanceVertexBuffer.GetVertexBufferForDevice(device);
                vertexBuffers.Add(ivbo);
            }


            _vertexBufferCache.Add(device, vertexBuffers);
              
            
            var idxBufferDesc =
                new BufferDescription((uint) (IndexData.Length * sizeof(uint)), BufferUsage.IndexBuffer);
            var ibo = factory.CreateBuffer(idxBufferDesc);
            device.UpdateBuffer(ibo, 0, IndexData);

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

        public override List<DeviceBuffer> GetVertexBufferForDevice(GraphicsDevice device)
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

        public override void UpdateDeviceBuffers(GraphicsDevice device)
        {
            if (null != InstanceVertexBuffer)
            {
                InstanceVertexBuffer.UpdateDeviceBuffers(device);
            }
        }

        public override IPrimitiveFunctor CreateTemplatePrimitiveFunctor(IPrimitiveFunctorDelegate pfd)
        {
            return new TemplatePrimitiveFunctor<T>(pfd, this);
        }
        
        public override bool Supports(IPrimitiveFunctor functor) { return true; }

        public override void Accept(IPrimitiveFunctor functor)
        {
            base.Accept(functor);
            foreach (var pSet in PrimitiveSets)
            {
                pSet.Accept(functor);
            }
        }
        public override bool Supports(IPrimitiveIndexFunctor functor) { return true; }
        public override void Accept(IPrimitiveIndexFunctor functor) {}
    }
}