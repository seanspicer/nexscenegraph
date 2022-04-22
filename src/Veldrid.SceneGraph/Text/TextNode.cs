//
// Copyright 2018-2021 Sean Spicer 
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
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid.SceneGraph.AssetPrimitives;
using Veldrid.SceneGraph.AssetProcessor;
using Veldrid.SceneGraph.Shaders.Standard;
using Veldrid.SceneGraph.Util;

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
        private Rgba32 _backgroundColor;
        private Rgba32 _outlineColor;
        private float _outlineStrokeWidth;

        private CharacterSizeModes _characterSizeMode;
        private float _charAspectRatio;

        private float _charHeight;
        private float _charWidth;

        private float _fontResolution;

        private float _fontSize;

        private HorizontalAlignment _horizontalAlignment;

        private Matrix4x4 _matrix;

        private int _padding;

        private string _text;

        private double _textAspectRatio;

        private Rgba32 _textColor;

        private int _textHeight;
        private int _textWidth;

        private VerticalAlignment _verticalAlignment;

        protected TextNode(string text,
            float fontSize,
            Rgba32 textColor,
            Rgba32 backgroundColor,
            Rgba32 outlineColor,
            float outlineStrokeWidth,
            VerticalAlignment verticalAlignment,
            HorizontalAlignment horizontalAlignment,
            int padding,
            float fontResolution)
        {
            _text = text;
            _fontSize = fontSize;
            _verticalAlignment = verticalAlignment;
            _horizontalAlignment = horizontalAlignment;
            _padding = padding;
            _textColor = textColor;
            _backgroundColor = backgroundColor;
            _outlineColor = outlineColor;
            _outlineStrokeWidth = outlineStrokeWidth;
            _fontResolution = fontResolution;

            AutoRotateToScreen = true;
            _characterSizeMode = CharacterSizeModes.ScreenCoords;

            ComputeTextRepresentation();
        }
        //private bool _dirty = true;

        private Font Font { get; set; }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Dirty();
            }
        }

        public int Padding
        {
            get => _padding;
            set
            {
                _padding = value;
                Dirty();
            }
        }

        public float FontResolution
        {
            get => _fontResolution;
            set
            {
                _fontResolution = value;
                Dirty();
            }
        }

        public Rgba32 TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Dirty();
            }
        }

        public Rgba32 BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Dirty();
            }
        }
        
        public Rgba32 OutlineColor
        {
            get => _outlineColor;
            set
            {
                _outlineColor = value;
                Dirty();
            }
        }

        public float OutlineStrokeWidth
        {
            get => _outlineStrokeWidth;
            set
            {
                _outlineStrokeWidth = value;
                Dirty();
            }
        }
        
        public VerticalAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _verticalAlignment = value;
                Dirty();
            }
        }

        public float FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                Dirty();
            }
        }

        public bool AutoRotateToScreen { get; set; }

        public CharacterSizeModes CharacterSizeMode
        {
            get => _characterSizeMode;
            set
            {
                _characterSizeMode = value;
                Dirty();
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                _horizontalAlignment = value;
                Dirty();
            }
        }


        public override bool ComputeMatrix(ref Matrix4x4 computedMatrix, IState state)
        {
            if (CharacterSizeMode != CharacterSizeModes.ObjectCoords || AutoRotateToScreen)
            {
                var modelview = state.ModelViewMatrix;
                var projection = state.ProjectionMatrix;

                var temp_matrix = Matrix4x4.Identity.PostMultiply(modelview);
                temp_matrix = temp_matrix.SetTranslation(Vector3.Zero);

                var rotationMatrix = Matrix4x4.Identity;
                var canInvert = Matrix4x4.Invert(temp_matrix, out rotationMatrix);
                if (false == canInvert) rotationMatrix = Matrix4x4.Identity;

                if (CharacterSizeMode != CharacterSizeModes.ObjectCoords)
                {
                    float width = state.Viewport.Width;
                    float height = state.Viewport.Height;

                    var mvpw = rotationMatrix * modelview * projection *
                               Matrix4x4.CreateScale(width / 2.0f, height / 2.0f, 1.0f);

                    var origin = mvpw.PreMultiply(Vector3.Zero);
                    var left = mvpw.PreMultiply(Vector3.UnitX) - origin;
                    var up = mvpw.PreMultiply(Vector3.UnitY) - origin;

                    var scaleF = 1f;

                    // compute the pixel size vector.
                    var length_x = left.Length();
                    var scale_x = length_x > 0.0f ? scaleF / length_x : 1.0f;

                    var length_y = up.Length();
                    var scale_y = length_y > 0.0f ? scaleF / length_y : 1.0f;

                    if (CharacterSizeMode == CharacterSizeModes.ScreenCoords)
                    {
                        var scaleVec = new Vector3(scale_x, scale_y, 1.0f);
                        computedMatrix = computedMatrix.PostMultiply(Matrix4x4.CreateScale(scaleVec));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                if (AutoRotateToScreen) computedMatrix = computedMatrix.PostMultiply(rotationMatrix);

                if (_matrix != computedMatrix)
                {
                    _matrix = computedMatrix;
                    DirtyBound();
                }
            }

            return true;
        }

        public static ITextNode Create(string text,
            float fontSize,
            Rgba32 textColor,
            Rgba32 backgroundColor,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            int padding = 4,
            float fontResolution = 1)
        {
            return new TextNode(
                text,
                fontSize,
                textColor,
                backgroundColor,
                SixLabors.ImageSharp.Color.Transparent,
                0.0f,
                verticalAlignment,
                horizontalAlignment,
                padding,
                fontResolution
            );
        }
        
        public static ITextNode Create(string text,
            float fontSize,
            Rgba32 textColor,
            Rgba32 outlineColor,
            float outlineStrokeWidth,
            Rgba32 backgroundColor,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            int padding = 4,
            float fontResolution = 1)
        {
            return new TextNode(
                text,
                fontSize,
                textColor,
                backgroundColor,
                outlineColor,
                outlineStrokeWidth,
                verticalAlignment,
                horizontalAlignment,
                padding,
                fontResolution
            );
        }

        public static ITextNode Create(string text,
            float fontSize = 20f,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            int padding = 4,
            float fontResolution = 1)
        {
            return new TextNode(
                text,
                fontSize,
                SixLabors.ImageSharp.Color.White,
                SixLabors.ImageSharp.Color.Transparent,
                SixLabors.ImageSharp.Color.Transparent,
                0.0f,
                verticalAlignment,
                horizontalAlignment,
                padding,
                fontResolution
            );
        }

        private void Dirty()
        {
            ComputeTextRepresentation();
        }

        private void ComputeTextRepresentation()
        {
            _matrix = Matrix4x4.Identity;

            // Create default Font
            Font = SystemFonts.CreateFont("Arial", FontSize);

            CalculateTextMetrics();

            var w = (float) _textAspectRatio * FontSize;
            var h = 1.0f * FontSize;

            Vector4 widthVec;
            Vector4 heightVec;

            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    widthVec = new Vector4(0, w, w, 0);
                    break;
                case HorizontalAlignment.Right:
                    widthVec = new Vector4(-w, 0, 0, -w);
                    break;
                case HorizontalAlignment.Center:
                    widthVec = new Vector4(-w / 2f, w / 2f, w / 2f, -w / 2f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    heightVec = new Vector4(0, 0, -h, -h);
                    break;
                case VerticalAlignment.Bottom:
                    heightVec = new Vector4(h, h, 0, 0);
                    break;
                case VerticalAlignment.Center:
                    heightVec = new Vector4(h / 2f, h / 2f, -h / 2f, -h / 2f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            VertexData = new[]
            {
                // Quad
                new VertexPositionTexture(new Vector3(widthVec.X, heightVec.X, +0.0f), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(widthVec.Y, heightVec.Y, +0.0f), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(widthVec.Z, heightVec.Z, +0.0f), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(widthVec.W, heightVec.W, +0.0f), new Vector2(0, 1))
            };

            IndexData = new uint[]
            {
                0, 1, 2, 0, 2, 3
            };

            var pSet = DrawElements<VertexPositionTexture>.Create(this, PrimitiveTopology.TriangleList,
                (uint) IndexData.Length, 1, 0, 0, 0);
            PrimitiveSets.Add(pSet);

            VertexLayouts = new List<VertexLayoutDescription>
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float3),
                    new VertexElementDescription("Texture", VertexElementSemantic.TextureCoordinate,
                        VertexElementFormat.Float2))
            };

            PipelineState.ShaderSet = Texture2DShader.Instance.ShaderSet;

            PipelineState.AddTexture(
                Texture2D.Create(BuildTexture(),
                    SamplerDescription.Aniso4x, 
                    1,
                    "SurfaceTexture",
                    "SurfaceSampler"));

            PipelineState.BlendStateDescription = BlendStateDescription.SingleAlphaBlend;

            DirtyBound();
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
            var size = TextMeasurer.Measure(Text, new TextOptions(Font){Dpi = 72 * FontResolution, KerningMode = KerningMode.Auto});

            var padding = Padding * FontResolution;

            _charHeight = size.Height;
            _charWidth = size.Width;
            _charAspectRatio = _charWidth / _charHeight;

            //var texSize = (int)NextPowerOfTwo((uint)  System.Math.Round(rawSize));
            _textWidth = (int) (size.Width + padding * 2); //(int)NextPowerOfTwo((uint)  System.Math.Round(size.Width));
            _textHeight =
                (int) (size.Height + padding * 2); //(int)NextPowerOfTwo((uint)  System.Math.Round(size.Height));

            _textAspectRatio = _textWidth / (double) _textHeight;
        }

        private ProcessedTexture BuildTexture()
        {
            using (var img = new Image<Rgba32>(_textWidth, _textHeight))
            {
                var padding = Padding * FontResolution;
                var targetWidth = img.Width - padding * 2;
                var targetHeight = img.Height - padding * 2;

                // measure the text size
                var size = TextMeasurer.Measure(Text, new TextOptions(Font));

                //find out how much we need to scale the text to allow for alignment
                var scalingFactor = System.Math.Min(targetWidth / img.Width, targetHeight / img.Height);

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
                        hCenter = img.Width - padding;
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
                        vCenter = img.Height - padding;
                        break;
                    case VerticalAlignment.Center:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var center = new PointF(hCenter, vCenter);

                img.Mutate(i => i.BackgroundColor(BackgroundColor));

                var textOptions = new TextOptions(scaledFont)
                {
                    Origin = center,
                    HorizontalAlignment = HorizontalAlignment,
                    VerticalAlignment = VerticalAlignment,
                    Dpi = 72 * FontResolution
                };

                var drawingOptions = new DrawingOptions
                {
                    GraphicsOptions =
                    {
                        Antialias = true
                    }
                };

                if (_outlineStrokeWidth > 0)
                {
                    img.Mutate(i => i.DrawText(drawingOptions, textOptions, Text, new SolidBrush(TextColor), Pens.Solid(_outlineColor, _outlineStrokeWidth)));
                }
                else
                {
                    img.Mutate(i => i.DrawText(drawingOptions, textOptions, Text, new SolidBrush(TextColor), null));
                }
                

                

                var imageProcessor = new ImageSharpProcessor();
                return imageProcessor.ProcessT(img);
            }
        }

        protected override IBoundingBox ComputeBoundingBox()
        {
            var bb = base.ComputeBoundingBox();

            if (CharacterSizeMode != CharacterSizeModes.ObjectCoords)
            {
                var newBb = BoundingBox.Create(bb.Min / 1000, bb.Max / 1000);
                bb = newBb;
            }

            return bb;
        }
    }
}