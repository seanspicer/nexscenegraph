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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using AssetProcessor;
using Veldrid.Utilities;

namespace Veldrid.SceneGraph.RenderGraph
{
    public class CullAndAssembleVisitor : NodeVisitor
    {
        public DrawSet DrawSet = new DrawSet();
        public GraphicsDevice GraphicsDevice { get; set; } = null;
        public ResourceFactory ResourceFactory { get; set; } = null;
        public ResourceLayout ResourceLayout { get; set; } = null;
        
        public Stack<Matrix4x4> ModelMatrixStack { get; set; } = new Stack<Matrix4x4>();

        public Stack<GraphicsPipelineDescription> PipelineDescriptionStack = new Stack<GraphicsPipelineDescription>();

        public bool Valid => null != GraphicsDevice;
        
        private Polytope CullingFrustum { get; set; } = new Polytope();
        
        public CullAndAssembleVisitor() : 
            base(VisitorType.CullAndAssembleVisitor, TraversalModeType.TraverseActiveChildren)
        {
            ModelMatrixStack.Push(Matrix4x4.Identity);
        }

        public void SetCullingViewProjectionMatrix(Matrix4x4 vp)
        {
            CullingFrustum.SetToViewProjectionFrustum(vp, false, false);
        }

        public override void Apply(Node node)
        {
            var needsPop = false;
            if (node.PipelineDescription.HasValue)
            {
                PipelineDescriptionStack.Push(node.PipelineDescription.Value);
                needsPop = true;
            }
            
            Traverse(node);

            if (needsPop)
            {
                PipelineDescriptionStack.Pop();
            }
        }

        public override void Apply(Transform transform)
        {
            var curModel = ModelMatrixStack.Peek();
            transform.ComputeLocalToWorldMatrix(ref curModel, this);
            ModelMatrixStack.Push(curModel);
            
            Traverse(transform);

            ModelMatrixStack.Pop();

        }

        public override void Apply<T>(Geometry<T> geometry)
        {
            // TODO - should be at Drawable level?
            var bb = geometry.GetBoundingBox();
            if (IsCulled(bb)) return;
            
            DrawSetNode dsn = new DrawSetNode();
            dsn.Drawable = geometry;

            // Construct Node-specific resource Layout
            dsn.ModelBuffer = ResourceFactory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            GraphicsDevice.UpdateBuffer(dsn.ModelBuffer, 0, Matrix4x4.Identity);
            
            var resourceLayoutElementDescriptionList = new List<ResourceLayoutElementDescription> { };
            resourceLayoutElementDescriptionList.Add(
                new ResourceLayoutElementDescription("Model", ResourceKind.UniformBuffer, ShaderStages.Vertex));
  
            var bindableResourceList = new List<BindableResource>();
            bindableResourceList.Add(dsn.ModelBuffer);
            
            // Process Attached Textures
            foreach (var tex2d in geometry.TextureList)
            {
                var deviceTexture =
                    tex2d.ProcessedTexture.CreateDeviceTexture(GraphicsDevice, ResourceFactory, TextureUsage.Sampled);
                var textureView =
                    ResourceFactory.CreateTextureView(deviceTexture);
  
                resourceLayoutElementDescriptionList.Add(
                    new ResourceLayoutElementDescription(tex2d.TextureName, ResourceKind.TextureReadOnly, ShaderStages.Fragment)
                    );
                resourceLayoutElementDescriptionList.Add(
                    new ResourceLayoutElementDescription(tex2d.SamplerName, ResourceKind.Sampler, ShaderStages.Fragment)
                );    

                bindableResourceList.Add(textureView);
                bindableResourceList.Add(GraphicsDevice.Aniso4xSampler);
            }
            
            dsn.ResourceLayout = ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(resourceLayoutElementDescriptionList.ToArray()));
            
            dsn.ResourceSet = ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    dsn.ResourceLayout,
                    bindableResourceList.ToArray()
                    )
            );

            GraphicsPipelineDescription pd;
            if (geometry.PipelineDescription.HasValue)
            {
                pd = geometry.PipelineDescription.Value;
            }
            else if (PipelineDescriptionStack.Count != 0)
            {
                pd = PipelineDescriptionStack.Peek();
            }
            else
            {
                pd = new GraphicsPipelineDescription
                {
                    BlendState = BlendStateDescription.SingleAlphaBlend,
                    DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerState = RasterizerStateDescription.Default,
                    PrimitiveTopology = geometry.PrimitiveTopology
                };
            }
            
            var vertexShaderProg =
                ResourceFactory.CreateShader(
                    new ShaderDescription(ShaderStages.Vertex, 
                        geometry.VertexShader, 
                        geometry.VertexShaderEntryPoint
                        )
                    );
            
            var fragmentShaderProg =
                ResourceFactory.CreateShader(
                    new ShaderDescription(ShaderStages.Fragment, 
                        geometry.FragmentShader, 
                        geometry.FragmentShaderEntryPoint
                        )
                    );
            
            pd.ResourceLayouts = new[] {ResourceLayout, dsn.ResourceLayout};
                  
            pd.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { geometry.VertexLayout },
                shaders: new Shader[] { vertexShaderProg, fragmentShaderProg });
                
            pd.Outputs = GraphicsDevice.SwapchainFramebuffer.OutputDescription;
                
            dsn.Pipeline = ResourceFactory.CreateGraphicsPipeline(pd);
            dsn.ModelMatrix = ModelMatrixStack.Peek();

            DrawSet.Add(dsn);
            
        }

        
        private bool IsCulled(BoundingBox bb)
        {
            // Is this bounding box culled?
            if (!CullingFrustum.Contains(bb)) return true;

            return false;
        }
    }
}