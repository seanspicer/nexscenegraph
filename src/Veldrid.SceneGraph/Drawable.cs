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

namespace Veldrid.SceneGraph
{
    public abstract class Drawable : Object
    {
        protected bool _boundingSphereComputed = false;
        protected BoundingSphere _boundingSphere = new BoundingSphere();
        
        protected BoundingBox _boundingBox;
        protected BoundingBox _initialBoundingBox = new BoundingBox();
        public BoundingBox InitialBoundingBox
        {
            get => _initialBoundingBox;
            set
            {
                _initialBoundingBox = value;
                DirtyBound();
            }
        } 
        
        public PrimitiveTopology PrimitiveTopology { get; set; }
        
        public VertexLayoutDescription VertexLayout { get; set; }
        
        public List<PrimitiveSet> PrimitiveSets { get; } = new List<PrimitiveSet>();
        
        public event Func<Drawable, BoundingBox> ComputeBoundingBoxCallback;
        public event Action<CommandList, Drawable> DrawImplementationCallback;

        private PipelineState _pipelineState = null;
        public PipelineState PipelineState
        {
            get => _pipelineState ?? (_pipelineState = new PipelineState());
            set => _pipelineState = value;
        }
        
        public bool HasPipelineState
        {
            get => null != _pipelineState;
        }
        
        public void Draw(GraphicsDevice device, List<Tuple<uint, ResourceSet>> resourceSets, CommandList commandList)
        {
            if (null != DrawImplementationCallback)
            {
                DrawImplementationCallback(commandList, this);
            }
            else
            {
                DrawImplementation(device, resourceSets, commandList);
            }
        }

        protected abstract void DrawImplementation(GraphicsDevice device, List<Tuple<uint, ResourceSet>> resourceSets, CommandList commandList);

        public virtual void ConfigureDeviceBuffers(GraphicsDevice device, ResourceFactory factory)
        {
            // Nothing by default
        }

        public virtual void ConfigurePipelinesForDevice(GraphicsDevice device, ResourceFactory factory,
            ResourceLayout parentLayout)
        {
            // Nothing by default
        }     
        
        public void DirtyBound()
        {
            if (!_boundingSphereComputed) return;
            
            _boundingSphereComputed = false;
        }
        
        public BoundingBox GetBoundingBox()
        {
            if (_boundingSphereComputed) return _boundingBox;
            
            _boundingBox = _initialBoundingBox;

            _boundingBox.ExpandBy(null != ComputeBoundingBoxCallback
                ? ComputeBoundingBoxCallback(this)
                : ComputeBoundingBox());

            if (_boundingBox.Valid())
            {
                _boundingSphere.Set(_boundingBox.Center, _boundingBox.Radius);
            }
            else
            {
                _boundingSphere.Init();
            }

            _boundingSphereComputed = true;

            return _boundingBox;
        }

        protected abstract BoundingBox ComputeBoundingBox();

        public abstract DeviceBuffer GetVertexBufferForDevice(GraphicsDevice device);

        public abstract DeviceBuffer GetIndexBufferForDevice(GraphicsDevice device);


    }
}