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
using System.Runtime.InteropServices;
using SkiaSharp;
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
        private static readonly RgbaByte White = new RgbaByte(255, 255, 255, 255);
        private static readonly RgbaByte Transparent = new RgbaByte(0, 0, 0, 0);

        private RgbaByte _backgroundColor;
        private RgbaByte _outlineColor;
        private float _outlineStrokeWidth;

        private CharacterSizeModes _characterSizeMode;
        private float _charAspectRatio;

        private float _charHeight;
        private float _charWidth;

        private float _fontResolution;

        private HorizontalAlignment _horizontalAlignment;

        private Matrix4x4 _matrix;

        private int _padding;

        private string _text;

        private double _textAspectRatio;

        private RgbaByte _textColor;

        private int _textHeight;
        private int _textWidth;

        private VerticalAlignment _verticalAlignment;

        protected TextNode(string text,
            TextFont font,
            RgbaByte textColor,
            RgbaByte backgroundColor,
            RgbaByte outlineColor,
            float outlineStrokeWidth,
            VerticalAlignment verticalAlignment,
            HorizontalAlignment horizontalAlignment,
            int padding,
            float fontResolution)
        {
            _text = text;
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

            Font = font;
            
            ComputeTextRepresentation();
        }
        //private bool _dirty = true;

        public TextFont Font { get; protected set; }

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

        public RgbaByte TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Dirty();
            }
        }

        public RgbaByte BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Dirty();
            }
        }
        
        public RgbaByte OutlineColor
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
            TextFont font,
            RgbaByte textColor,
            RgbaByte backgroundColor,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            int padding = 4,
            float fontResolution = 1)
        {
            return new TextNode(
                text,
                font,
                textColor,
                backgroundColor,
                Transparent,
                0.0f,
                verticalAlignment,
                horizontalAlignment,
                padding,
                fontResolution
            );
        }
        
        public static ITextNode Create(string text,
            TextFont font,
            RgbaByte textColor,
            RgbaByte outlineColor,
            float outlineStrokeWidth,
            RgbaByte backgroundColor,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            int padding = 4,
            float fontResolution = 1)
        {
            return new TextNode(
                text,
                font,
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
            TextFont font,
            VerticalAlignment verticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center,
            int padding = 4,
            float fontResolution = 1)
        {
            return new TextNode(
                text,
                font,
                White,
                Transparent,
                Transparent,
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

            CalculateTextMetrics();

            var w = (float) _textAspectRatio * Font.Size;
            var h = 1.0f * Font.Size;

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
            var size = MeasureText(Font.Size * FontResolution);

            var padding = Padding * FontResolution;

            _charHeight = size.Height;
            _charWidth = size.Width;
            _charAspectRatio = _charWidth / _charHeight;

            //var texSize = (int)NextPowerOfTwo((uint)  System.Math.Round(rawSize));
            // Round up so the last (antialiased) column/row of the glyphs is not cut off.
            _textWidth = (int) System.Math.Ceiling(size.Width + padding * 2);
            _textHeight = (int) System.Math.Ceiling(size.Height + padding * 2);

            _textAspectRatio = _textWidth / (double) _textHeight;
        }

        private SKTypeface CreateTypeface()
        {
            return SKTypeface.FromFamilyName(Font.Family,
                       Font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                       SKFontStyleWidth.Normal,
                       Font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright)
                   ?? SKTypeface.Default;
        }

        private SKPaint CreatePaint(SKTypeface typeface, float pixelSize)
        {
            return new SKPaint
            {
                Typeface = typeface,
                TextSize = pixelSize,
                IsAntialias = true,
                SubpixelText = true
            };
        }

        // Antialiasing can put partial coverage up to a pixel outside the glyph outlines.
        private const float AntialiasMargin = 1f;

        // Width: the larger of the advance and the ink width, plus an antialiasing margin on each side.
        // Height: line height (ascent + descent + leading).
        private (float Width, float Height) MeasureText(float pixelSize)
        {
            using var typeface = CreateTypeface();
            using var paint = CreatePaint(typeface, pixelSize);
            var metrics = paint.FontMetrics;
            var inkBounds = new SKRect();
            var advance = paint.MeasureText(Text ?? string.Empty, ref inkBounds);
            var width = System.Math.Max(advance, inkBounds.Width) + 2 * AntialiasMargin;
            return (width, metrics.Descent - metrics.Ascent + metrics.Leading);
        }

        private ProcessedTexture BuildTexture()
        {
            var width = System.Math.Max(1, _textWidth);
            var height = System.Math.Max(1, _textHeight);

            using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
            using (var canvas = new SKCanvas(bitmap))
            {
                var padding = Padding * FontResolution;
                var targetWidth = width - padding * 2;
                var targetHeight = height - padding * 2;

                // Shrink the text so it fits inside the padding.
                var scalingFactor = System.Math.Min(targetWidth / width, targetHeight / height);

                using var typeface = CreateTypeface();
                using var paint = CreatePaint(typeface, scalingFactor * Font.Size * FontResolution);
                var metrics = paint.FontMetrics;
                var lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;

                float x;
                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        x = padding + AntialiasMargin;
                        paint.TextAlign = SKTextAlign.Left;
                        break;
                    case HorizontalAlignment.Right:
                        x = width - padding - AntialiasMargin;
                        paint.TextAlign = SKTextAlign.Right;
                        break;
                    case HorizontalAlignment.Center:
                        x = width / 2f;
                        paint.TextAlign = SKTextAlign.Center;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                float lineTop;
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Top:
                        lineTop = padding;
                        break;
                    case VerticalAlignment.Bottom:
                        lineTop = height - padding - lineHeight;
                        break;
                    case VerticalAlignment.Center:
                        lineTop = (height - lineHeight) / 2f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var baseline = lineTop - metrics.Ascent;

                canvas.Clear(ToSkColor(BackgroundColor));

                // Outline first so it forms a halo behind the fill instead of eating into thin glyphs.
                if (_outlineStrokeWidth > 0)
                {
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = _outlineStrokeWidth;
                    paint.StrokeJoin = SKStrokeJoin.Round;
                    paint.Color = ToSkColor(_outlineColor);
                    canvas.DrawText(Text ?? string.Empty, x, baseline, paint);
                }

                paint.Style = SKPaintStyle.Fill;
                paint.Color = ToSkColor(TextColor);
                canvas.DrawText(Text ?? string.Empty, x, baseline, paint);
            }

            return SkiaImageProcessor.CreateTexture(bitmap);
        }

        private static SKColor ToSkColor(RgbaByte color) => new SKColor(color.R, color.G, color.B, color.A);

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