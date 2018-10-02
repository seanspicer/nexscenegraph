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

using System.Numerics;
using System.Runtime.InteropServices;
using AssetPrimitives;
using AssetProcessor;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using ShaderGen;
using Veldrid;
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
    
    public class TextNode : Geometry<VertexPositionTexture>
    {
        private Font Font { get; set; }
        public string Text { get; private set; }

        public TextNode(string text)
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

            IndexData = new ushort[]
            {
                0, 1, 2, 0, 2, 3,
            };
            
            var pSet = new DrawElements<VertexPositionTexture>(this, PrimitiveTopology.TriangleList, (uint)IndexData.Length, 1, 0, 0, 0);
            PrimitiveSets.Add(pSet);
            
            VertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

            PipelineState.VertexShaderDescription = Texture2DShader.Instance.VertexShaderDescription;
            PipelineState.FragmentShaderDescription = Texture2DShader.Instance.FragmentShaderDescription;;

            PipelineState.TextureList.Add(
                new Texture2D(BuildTexture(),
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