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
using Veldrid;
using Veldrid.SceneGraph.Util;
using BufferDescription = Veldrid.BufferDescription;
using BufferUsage = Veldrid.BufferUsage;

namespace Veldrid.SceneGraph
{
    internal struct DrawInfo
    {
        internal DeviceBuffer VertexBuffer { get; set; }
        internal DeviceBuffer IndexBuffer { get; set; }
        internal uint NumIndices { get; set; }
        internal Shader VertexShader { get; set; }
        internal Shader FragmentShader { get; set; }
        internal ResourceSet ResourceSet { get; set; }
        internal CommandList CommandList { get; set; }
        internal Pipeline PipeLine { get; set; }
        
    }
    
    internal class DrawVisitor : NodeVisitor
    {
        private Dictionary<Guid, DrawInfo> DrawInfoDictionary { get; }

        private RenderInfo _renderInfo = new RenderInfo();

        public GraphicsDevice GraphicsDevice
        {
            get => _renderInfo.GraphicsDevice;
            set => _renderInfo.GraphicsDevice = value;
        }

        public ResourceFactory ResourceFactory
        {
            get => _renderInfo.ResourceFactory;
            set => _renderInfo.ResourceFactory = value;
        }
        
        public CommandList CommandList
        {
            get => _renderInfo.CommandList;
            set => _renderInfo.CommandList = value;
        }
        public ResourceLayout ResourceLayout
        {
            get => _renderInfo.ResourceLayout;
            set => _renderInfo.ResourceLayout = value;
        }
        public ResourceSet ResourceSet
        {
            get => _renderInfo.ResourceSet;
            set => _renderInfo.ResourceSet = value;
        }
        
        internal DrawVisitor() : base(VisitorType.NodeVisitor)
        {
            TraversalMode = TraversalModeType.TraverseActiveChildren;
            DrawInfoDictionary = new Dictionary<Guid, DrawInfo>();
        }

        public void BeginDraw()
        {
        }

        public override void Apply(Drawable drawable)
        {
            drawable.Draw(_renderInfo);
        }

        public void EndDraw()
        {
        }
        
    }
}