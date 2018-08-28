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
using BufferDescription = Veldrid.BufferDescription;
using BufferUsage = Veldrid.BufferUsage;

namespace Veldrid.SceneGraph
{
    internal struct DrawInfo
    {
        internal DeviceBuffer VertexBuffer { get; set; }
        internal DeviceBuffer IndexBuffer { get; set; }
        internal Shader VertexShader { get; set; }
        internal Shader FragmentShader { get; set; }
        internal CommandList CommandList { get; set; }
        internal Pipeline PipeLine { get; set; }
        
    }
    
    internal class DrawVisitor : NodeVisitor
    {
        private Dictionary<Guid, DrawInfo> DrawInfoDictionary { get; }
        
        public GraphicsDevice GraphicsDevice { get; set; }
        private CommandList CommandList { get; set; }
        internal DrawVisitor()
        {
            DrawInfoDictionary = new Dictionary<Guid, DrawInfo>();
        }

        public void BeginDraw()
        {
            // TODO: Question - is this inefficient? Get from pool?
            CommandList = GraphicsDevice.ResourceFactory.CreateCommandList();
            
            // Begin() must be called before commands can be issued.
            CommandList.Begin();

            // We want to render directly to the output window.
            CommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            CommandList.ClearColorTarget(0, RgbaFloat.Black);
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

            // Set all relevant state to draw our quad.
            CommandList.SetVertexBuffer(0, drawInfo.VertexBuffer);
            CommandList.SetIndexBuffer(drawInfo.IndexBuffer, IndexFormat.UInt16);
            CommandList.SetPipeline(drawInfo.PipeLine);
            // Issue a Draw command for a single instance with 4 indices.
            CommandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }

        public void EndDraw()
        {
            // End() must be called before commands can be submitted for execution.
            CommandList.End();
            
            GraphicsDevice.SubmitCommands(CommandList);

            // Once commands have been submitted, the rendered image can be presented to the application window.
            // TODO - perhaps swapbuffers should be elsewhere (like in the viewer) as this may be called on a thread
            GraphicsDevice.SwapBuffers();
        }
        
        private DrawInfo SetupDrawInfo<T>(Geometry<T> geometry) where T : struct
        {
            var drawInfo = new DrawInfo();

            var factory = GraphicsDevice.ResourceFactory;

            var vbDescription = new BufferDescription(
                (uint) (geometry.VertexData.Length * geometry.SizeOfVertexData),
                BufferUsage.VertexBuffer);

            drawInfo.VertexBuffer = factory.CreateBuffer(vbDescription);
            GraphicsDevice.UpdateBuffer(drawInfo.VertexBuffer, 0, geometry.VertexData);
            
            var ibDescription = new BufferDescription(
                (uint) geometry.IndexData.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);

            drawInfo.IndexBuffer = factory.CreateBuffer(ibDescription);
            GraphicsDevice.UpdateBuffer(drawInfo.IndexBuffer, 0, geometry.IndexData);

            drawInfo.VertexShader =
                factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, geometry.VertexShader, "VS"));
            drawInfo.FragmentShader =
                factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, geometry.FragmentShader, "FS"));
            
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
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
              
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { geometry.VertexLayout },
                shaders: new Shader[] { drawInfo.VertexShader, drawInfo.FragmentShader });
            pipelineDescription.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;

            drawInfo.PipeLine = factory.CreateGraphicsPipeline(pipelineDescription);

            drawInfo.CommandList = factory.CreateCommandList();
            
            return drawInfo;
        }
        
        internal void DisposeResources()
        {
            foreach (var drawInfo in DrawInfoDictionary.Values)
            {
                drawInfo.PipeLine.Dispose();
                drawInfo.VertexShader.Dispose();
                drawInfo.FragmentShader.Dispose();
                drawInfo.VertexBuffer.Dispose();
                drawInfo.IndexBuffer.Dispose();
            }
            CommandList.Dispose();
        }
    }
}