using System;
using System.Collections.Generic;
using Nsg.Core;
using Nsg.Core.Interfaces;
using Veldrid;
using BufferDescription = Veldrid.BufferDescription;
using BufferUsage = Veldrid.BufferUsage;

namespace Nsg.VeldridBackend
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
    
    internal class DrawVisitor : IDrawVisitor
    {
        private Dictionary<Guid, DrawInfo> DrawInfoDictionary { get; }
        
        private GraphicsDevice GraphicsDevice { get; set; }
        internal DrawVisitor(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            DrawInfoDictionary = new Dictionary<Guid, DrawInfo>();
        }
        
        // Draw a Geometry
        public void Draw<T>(Geometry<T> geometry) where T : struct
        {
            DrawInfo drawInfo;
            if (!DrawInfoDictionary.TryGetValue(geometry.Id, out drawInfo))
            {
                drawInfo = SetupDrawInfo(geometry);
                DrawInfoDictionary.Add(geometry.Id, drawInfo);
            }
            
            // Begin() must be called before commands can be issued.
            drawInfo.CommandList.Begin();

            // We want to render directly to the output window.
            drawInfo.CommandList.SetFramebuffer(GraphicsDevice.SwapchainFramebuffer);
            drawInfo.CommandList.ClearColorTarget(0, RgbaFloat.Black);

            // Set all relevant state to draw our quad.
            drawInfo.CommandList.SetVertexBuffer(0, drawInfo.VertexBuffer);
            drawInfo.CommandList.SetIndexBuffer(drawInfo.IndexBuffer, IndexFormat.UInt16);
            drawInfo.CommandList.SetPipeline(drawInfo.PipeLine);
            // Issue a Draw command for a single instance with 4 indices.
            drawInfo.CommandList.DrawIndexed(
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);

            // End() must be called before commands can be submitted for execution.
            drawInfo.CommandList.End();
            
            GraphicsDevice.SubmitCommands(drawInfo.CommandList);

            // Once commands have been submitted, the rendered image can be presented to the application window.
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
            
            // 
            // KLUDGE - KLUDGE - KLUDGE 
            // 
            // This really needs to come from the geometry...
            // 
            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4));
            // 
            // KLUDGE - KLUDGE - KLUDGE 
            //
            
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
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
                drawInfo.CommandList.Dispose();
                drawInfo.VertexBuffer.Dispose();
                drawInfo.IndexBuffer.Dispose();
            }
            
        }
    }
}