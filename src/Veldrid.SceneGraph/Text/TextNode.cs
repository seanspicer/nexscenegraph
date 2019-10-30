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
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
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

        //[PositionSemantic] 
        public Vector3 Position;
        //[ColorSemantic]
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
        public int Padding { get; }
        public float FontResolution { get; }
        public Rgba32 TextColor { get; }
        public Rgba32 BackgroundColor { get; }
        public VerticalAlignment VerticalAlignment { get; }

        public HorizontalAlignment HorizontalAlignment { get; }
        
        private int _textWidth;
        private int _textHeight;
        private double _textAspectRatio;
        
        public static ITextNode Create(string text, 
            Rgba32 textColor,
            Rgba32 backgroundColor,
            VerticalAlignment verticalAlignment=VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment=HorizontalAlignment.Center,
            int padding=4,
            float fontResolution=1)
        {
            return new TextNode(
                text, 
                textColor, 
                backgroundColor,
                verticalAlignment, 
                horizontalAlignment,
                padding,
                fontResolution
                );
        }
        
        public static ITextNode Create(string text,
            VerticalAlignment verticalAlignment=VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment=HorizontalAlignment.Center,
            int padding=4,
            float fontResolution=1)
        {
            return new TextNode(
                text, 
                Rgba32.White, 
                Rgba32.Transparent, 
                verticalAlignment, 
                horizontalAlignment,
                padding,
                fontResolution
            );
        }
        
        protected TextNode(string text,
            Rgba32 textColor,
            Rgba32 backgroundColor,
            VerticalAlignment verticalAlignment,
            HorizontalAlignment horizontalAlignment,
            int padding,
            float fontResolution)
        {
            Text = text;
            VerticalAlignment = verticalAlignment;
            HorizontalAlignment = horizontalAlignment;
            Padding = padding;
            TextColor = textColor;
            BackgroundColor = backgroundColor;
            FontResolution = fontResolution;

            // Create default Font
            Font = SystemFonts.CreateFont("Arial", 20);
            
            CalculateTextMetrics();

            var w = (float)_textAspectRatio;var h = 1.0f;
            
            VertexData = new VertexPositionTexture[]
            {
                // Quad
                new VertexPositionTexture(new Vector3(-w, +h, +0.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(+w, +h, +0.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(+w, -h, +0.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-w, -h, +0.0f), new Vector2(0, 1))
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
            
            var rsd = new RasterizerStateDescription();
            rsd.FillMode = PolygonFillMode.Wireframe;
            //PipelineState.RasterizerStateDescription = rsd;
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

        internal void CalculateTextMetrics()
        {
            SizeF size = TextMeasurer.Measure(Text, new RendererOptions(Font, 72*FontResolution));

            var padding = Padding * FontResolution;
            
            //var texSize = (int)NextPowerOfTwo((uint) Math.Round(rawSize));
            _textWidth = (int)((size.Width + (padding * 2)));//(int)NextPowerOfTwo((uint) Math.Round(size.Width));
            _textHeight = (int)((size.Height + (padding * 2)));//(int)NextPowerOfTwo((uint) Math.Round(size.Height));

            _textAspectRatio = (double)_textWidth / (double)_textHeight;
        }

        private ProcessedTexture BuildTexture()
        {
            using (var img = new Image<Rgba32>(_textWidth, _textHeight))
            {
                var padding = this.Padding*FontResolution;
                float targetWidth = img.Width - (padding * 2);
                float targetHeight = img.Height - (padding * 2);

                // measure the text size
                var size = TextMeasurer.Measure(Text, new RendererOptions(Font));

                //find out how much we need to scale the text to allow for alignment
                var scalingFactor = Math.Min(targetWidth/img.Width, targetHeight/img.Height);

                //create a new font 
                var scaledFont = new Font(Font, scalingFactor * Font.Size);
                
                var hCenter = img.Width / 2f;
                var vCenter = img.Height / 2f;
                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        hCenter = padding;
                        break;
                    case HorizontalAlignment.Right:
                        hCenter = img.Width-padding;
                        break;
                    case HorizontalAlignment.Center:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Top:
                        vCenter = padding;
                        break;
                    case VerticalAlignment.Bottom:
                        vCenter = img.Height-padding;
                        break;
                    case VerticalAlignment.Center:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                var center = new PointF(hCenter, vCenter);
                
                
                var textGraphicOptions = new TextGraphicsOptions(true) {
                    HorizontalAlignment = HorizontalAlignment,
                    VerticalAlignment = VerticalAlignment,
                    DpiX = 72*FontResolution,
                    DpiY = 72*FontResolution
                };
                
                img.Mutate(i => i.BackgroundColor(BackgroundColor));
                img.Mutate(i => i.DrawText(textGraphicOptions, Text, scaledFont, TextColor, center));
                
                var imageProcessor = new ImageSharpProcessor();
                return imageProcessor.ProcessT(img);

            }
        }
    }
}