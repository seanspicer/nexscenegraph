//
// Copyright 2018 Sean Spicer 
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

using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using ShaderGen;
using Veldrid;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.AssetProcessor;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;
using Math = System.Math;


namespace Veldrid.SceneGraph.Text
{
    public struct VertexPositionTexture : IPrimitiveElement
    {
        public const uint SizeInBytes = 20;

        [PositionSemantic] 
        public Vector3 Position;
        [ColorSemantic]
        public Vector2 TexCoord;
        
        public VertexPositionTexture(Vector3 position, Vector2 texCoord)
        {
            Position = position;
            TexCoord = texCoord;
        }

        public Vector3 VertexPosition
        {
            get => Position;
            set => Position = value;
        }
    }
    
    public class TextNode : Geometry<VertexPositionTexture>, ITextNode
    {
        private Font Font { get; set; }
        public string Text { get; private set; }

        public static ITextNode Create(string text)
        {
            return new TextNode(text);
        }
        
        protected TextNode(string text)
        {
            Text = text;
            
            VertexData = new VertexPositionTexture[]
            {
                // Quad
                new VertexPositionTexture(new Vector3(-1.0f, +1.0f, +0.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, +1.0f, +0.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(+1.0f, -1.0f, +0.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1.0f, -1.0f, +0.0f), new Vector2(0, 1))
            };

            IndexData = new uint[]
            {
                0, 1, 2, 0, 2, 3,
            };
            
            var pSet = DrawElements<VertexPositionTexture>.Create(this, PrimitiveTopology.TriangleList, (uint)IndexData.Length, 1, 0, 0, 0);
            PrimitiveSets.Add(pSet);
            
            VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            PipelineState.VertexShaderDescription = Texture2DShader.Instance.VertexShaderDescription;
            PipelineState.FragmentShaderDescription = Texture2DShader.Instance.FragmentShaderDescription;;

            PipelineState.AddTexture(
                Texture2D.Create(BuildTexture(),
                    1,
                    "SurfaceTexture", 
                    "SurfaceSampler"));
            
            PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;
        }

        private uint NextPowerOfTwo(uint v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;

            return v;
        }

        internal ProcessedTexture BuildTexture()
        {
            // Create default Font
            Font = SystemFonts.CreateFont("Arial", 20);
            SizeF size = TextMeasurer.Measure(Text, new RendererOptions(Font));

            var rawSize = Math.Max(size.Width, size.Height);
            var texSize = (int)NextPowerOfTwo((uint) Math.Round(rawSize));
            
            using (var img = new Image<Rgba32>(texSize, texSize))
            {
                var padding = 4;
                float targetWidth = img.Width - (padding * 2);
                float targetHeight = img.Height - (padding * 2);

                // measure the text size
                //SizeF size = TextMeasurer.Measure(Text, new RendererOptions(Font));

                //find out how much we need to scale the text to fill the space (up or down)
//                float scalingFactor = Math.Min(img.Width / size.Width, img.Height / size.Height);
//
//                //create a new font 
//                Font scaledFont = new Font(Font, scalingFactor * Font.Size);
//
                var center = new PointF(img.Width / 2, img.Height / 2);
                
                var textGraphicOptions = new TextGraphicsOptions(true) {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    
                };
                
                img.Mutate(i => i.BackgroundColor(Rgba32.Transparent));
                img.Mutate(i => i.DrawText(textGraphicOptions, Text, Font, Rgba32.White, center));
                
                var imageProcessor = new ImageSharpProcessor();
                return imageProcessor.ProcessT(img);

            }
        }
    }
}