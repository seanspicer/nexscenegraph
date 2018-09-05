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
        
        public GraphicsDevice GraphicsDevice { get; set; }
        public ResourceFactory ResourceFactory { get; set; }
        public CommandList CommandList { get; set; }
        public ResourceLayout ResourceLayout { get; set; }
        public ResourceSet ResourceSet { get; set; }
        
        internal DrawVisitor()
        {
            TraversalMode = TraversalModeType.TraverseActiveChildren;
            DrawInfoDictionary = new Dictionary<Guid, DrawInfo>();
        }

        public void BeginDraw()
        {
        }
        
        // Draw a Geometry
        public override void Apply<T>(Geometry<T> geometry)
        {
            DrawInfo drawInfo;
            if (!DrawInfoDictionary.TryGetValue(geometry.Id, out drawInfo))
            {
                drawInfo = SetupDrawInfo(geometry);
                DrawInfoDictionary.Add(geometry.Id, drawInfo);
            }
            
            // Set pipeline
            CommandList.SetPipeline(drawInfo.PipeLine);

            // Set the resources
            CommandList.SetGraphicsResourceSet(0, ResourceSet);
            
            // Set all relevant state to draw our quad.
            CommandList.SetVertexBuffer(0, drawInfo.VertexBuffer);
            CommandList.SetIndexBuffer(drawInfo.IndexBuffer, IndexFormat.UInt16); // Problem child
            
            // Issue a Draw command for a single instance with 4 indices.
            CommandList.DrawIndexed(
                indexCount: drawInfo.NumIndices,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }

        public void EndDraw()
        {
        }
        
        private DrawInfo SetupDrawInfo<T>(Geometry<T> geometry) where T : struct
        {
            var drawInfo = new DrawInfo();

            var vbDescription = new BufferDescription(
                (uint) (geometry.VertexData.Length * geometry.SizeOfVertexData),
                BufferUsage.VertexBuffer);

            drawInfo.VertexBuffer = ResourceFactory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(drawInfo.VertexBuffer, 0, geometry.VertexData);
            
            var ibDescription = new BufferDescription(
                (uint) geometry.IndexData.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);

            drawInfo.NumIndices = (uint) geometry.IndexData.Length;
            drawInfo.IndexBuffer = ResourceFactory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(drawInfo.IndexBuffer, 0, geometry.IndexData);

            // TODO - maybe make a class for this stuff <ShaderProgram> ?
            drawInfo.VertexShader =
                ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Vertex, geometry.VertexShader, geometry.VertexShaderEntryPoint));
            drawInfo.FragmentShader =
                ResourceFactory.CreateShader(new ShaderDescription(ShaderStages.Fragment, geometry.FragmentShader, geometry.FragmentShaderEntryPoint));
            
            var pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = geometry.PrimitiveTopology;

            pipelineDescription.ResourceLayouts = new[] {ResourceLayout};
              
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { geometry.VertexLayout },
                shaders: new Shader[] { drawInfo.VertexShader, drawInfo.FragmentShader });
            
            pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
            
            drawInfo.PipeLine = ResourceFactory.CreateGraphicsPipeline(pipelineDescription);

            return drawInfo;
        }
        
    }
}