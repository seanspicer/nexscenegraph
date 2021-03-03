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
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Veldrid.SceneGraph
{
    public struct Color : IEquatable<Color>, IFormattable
    {
        // Use a System.Numerics.Vector4 internally
        // This guarantees that operations on arrays of Color are
        // vectorized.
        private Vector4 _v;

        public float R
        {
            get => _v.X;
            set => _v.X = value;
        }

        public float G
        {
            get => _v.Y;
            set => _v.Y = value;
        }
        
        public float B
        {
            get => _v.Z;
            set => _v.Z = value;
        }
        public float A
        {
            get => _v.W;
            set => _v.W = value;
        }
        
        public static Color FromRgba(float r, float g, float b, float a)
        {
            return new Color(r, g, b, a);
        }
        
        public static Color FromRgb(float r, float g, float b)
        {
            return new Color(r, g, b, 1.0f);
        }

        private Color(float r, float g, float b, float a)
        {
            _v = new Vector4(r, g, b, a);
        }

        private Color(Vector4 v)
        {
            _v = v;
        }

        public static implicit operator Vector4(Color c)
        {
            return c._v;
        }
        
        public static implicit operator Vector3(Color c)
        {
            return new Vector3(c.R, c.G, c.B);
        }

        public static implicit operator Color(Vector4 v)
        {
            return new Color(v);
        }

        public static implicit operator System.Drawing.Color(Color c)
        {
            return System.Drawing.Color.FromArgb(
                (int)(c.A * 255.0f), 
                (int)(c.R * 255.0f),
                (int)(c.G * 255.0f),
                (int)(c.B * 255.0f));
        }
        
        public static implicit operator Color(System.Drawing.Color c)
        {
            return new Color(
                c.R / 255.0f,
                c.G / 255.0f,
                c.B / 255.0f,
                c.A / 255.0f);
        }

        // index operator
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return R;
                    case 1: return G;
                    case 2: return B;
                    case 3: return A;
                    default:
                        throw new IndexOutOfRangeException("Invalid Color Component index(" + index + ")!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: R = value; break;
                    case 1: G = value; break;
                    case 2: B = value; break;
                    case 3: A = value; break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Color Color Component index(" + index + ")!");
                }
            }
        }
        
        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.InvariantCulture.NumberFormat);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "F3";
            }
            return $"Rgba({R.ToString(format, formatProvider)}, {G.ToString(format, formatProvider)}, {B.ToString(format, formatProvider)}, {A.ToString(format, formatProvider)})";
        }

        public override int GetHashCode()
        {
            return ((Vector4)this).GetHashCode();
        }

        public override bool Equals(object other)
        {
            return other is Color color && Equals(color);
        }

        public bool Equals(Color other)
        {
            return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
        }

        // Equal
        public static bool operator==(Color lhs, Color rhs)
        {
            // Returns false in the presence of NaN values.
            return (Vector4)lhs == (Vector4)rhs;
        }

        // Not Equal
        public static bool operator!=(Color lhs, Color rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }
        
        // Component Add
        public static Color operator +(Color a, Color b)
        {
            return a._v + b._v;
        }

        // Component Subtract
        public static Color operator -(Color a, Color b)
        {
            return a._v - b._v;
        }

        // Component Multiply
        public static Color operator *(Color a, Color b)
        {
            return a._v * b._v;
        }

        // Scale color
        public static Color operator *(Color a, float b)
        {
            return a._v * b;
        }

        // Scale Color
        public static Color operator *(float b, Color a)
        {
            return a._v * b;
        }

        // Component Divide
        public static Color operator /(Color a, float b)
        {
            return a._v / b;
        }

        // Unit Clamp [0-1]
        private static float UnitClamp(float value)
        {
            if (value < 0)
                return 0;
            else if (value > 1)
                return 1;
            else
                return value;
        }
        
        // Lerp Func
        public static Color Lerp(Color a, Color b, float t)
        {
            t = UnitClamp(t);
            return new Color(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t
            );
        }

        // Unclamped Lerp
        public static Color LerpUnclamped(Color a, Color b, float t)
        {
            return new Color(
                a.R + (b.R - a.R) * t,
                a.G + (b.G - a.G) * t,
                a.B + (b.B - a.B) * t,
                a.A + (b.A - a.A) * t
            );
        }

        // RGB component scale
        internal Color RgbMultiplied(float multiplier)
        {
            return new Color(R * multiplier, G * multiplier, B * multiplier, A);
        }
        // Alpha scale
        internal Color AlphaMultiplied(float multiplier)
        {
            return new Color(R, G, B, A * multiplier);
        }
        // RGB component multiply
        internal Color RgbMultiplied(Color multiplier)
        {
            return new Color(R * multiplier.R, G * multiplier.G, B * multiplier.B, A);
        }

        // Solid red. 
        public static Color Red => new Color(1f, 0f, 0f, 1f);
        // Solid green. 
        public static Color Green => new Color(0f, 1f, 0f, 1f);
        // Solid blue. 
        public static Color Blue => new Color(0f, 0f, 1f, 1f);
        // Solid white. 
        public static Color White => new Color(1f, 1f, 1f, 1f);
        // Solid black. 
        public static Color Black => new Color(0f, 0f, 0f, 1f);
        // Yellow. RGBA is (1, 0.92, 0.016, 1)
        public static Color Yellow => new Color(1f, 235f / 255f, 4f / 255f, 1f);
        // Cyan. 
        public static Color Cyan => new Color(0f, 1f, 1f, 1f);
        // Magenta. 
        public static Color Magenta => new Color(1f, 0f, 1f, 1f);
        // Gray. 
        public static Color Gray => new Color(.5f, .5f, .5f, 1f);
        public static Color Grey => Gray;
        // Transparent. 
        public static Color Transparent => new Color(0f, 0f, 0f, 0f);
        // Grayscale conversion
        public float Grayscale => 0.299f * R + 0.587f * G + 0.114f * B;
        public Color ToGrayscale() => new Color(Grayscale, Grayscale, Grayscale, 1.0f);
        
        // Max component value
        public float MaxColorComponent => System.Math.Max(System.Math.Max(R, G), B);
        
        // RGB to HSV color space conversion
        public static (float h, float s, float v) RgbToHsv(Color rgbColor)
        {
            if ((rgbColor.B > rgbColor.G) && (rgbColor.B > rgbColor.R))
            {
                return RgbToHsvPrivate(4, 
                    rgbColor.B, 
                    rgbColor.R, 
                    rgbColor.G);
            }
            else if (rgbColor.G > rgbColor.R)
            {
                return RgbToHsvPrivate(2, 
                    rgbColor.G, 
                    rgbColor.B, 
                    rgbColor.R);
            }
            else
            {
                return RgbToHsvPrivate(0, 
                    rgbColor.R, 
                    rgbColor.G, 
                    rgbColor.B);
            }
        }

        private static (float h, float s, float v) RgbToHsvPrivate(
            float offset, 
            float dominantColor,
            float colorOne, 
            float colorTwo)
        {
            float h;
            float s;
            var v = dominantColor;
            
            if (v != 0)
            {
                var small = colorOne > colorTwo ? colorTwo : colorOne;

                var diff = v - small;

                if (diff != 0)
                {
                    s = diff / v;
                    h = offset + ((colorOne - colorOne) / diff);
                }
                else
                {
                    s = 0;
                    h = offset + (colorOne - colorOne);
                }

                h /= 6;

                if (h < 0)
                    h += 1.0f;
            }
            else
            {
                s = 0;
                h = 0;
            }

            return (h, s, v);
        }
        
        // Colorspace Conversion
        public static Color HsvToRgb(float h, float s, float v, bool hdr = true)
        {
            var retVal = Color.White;
            if (s == 0)
            {
                retVal.R = v;
                retVal.G = v;
                retVal.B = v;
            }
            else if (v == 0)
            {
                retVal.R = 0;
                retVal.G = 0;
                retVal.B = 0;
            }
            else
            {
                retVal.R = 0;
                retVal.G = 0;
                retVal.B = 0;

                var tS = s;
                var tV = v;
                var hToFloor = h * 6.0f;

                var temp = (int)System.Math.Floor(hToFloor);
                var t = hToFloor - (temp);
                var var1 = (tV) * (1 - tS);
                var var2 = tV * (1 - tS *  t);
                var var3 = tV * (1 - tS * (1 - t));

                switch (temp)
                {
                    case 0:
                        retVal.R = tV;
                        retVal.G = var3;
                        retVal.B = var1;
                        break;

                    case 1:
                        retVal.R = var2;
                        retVal.G = tV;
                        retVal.B = var1;
                        break;

                    case 2:
                        retVal.R = var1;
                        retVal.G = tV;
                        retVal.B = var3;
                        break;

                    case 3:
                        retVal.R = var1;
                        retVal.G = var2;
                        retVal.B = tV;
                        break;

                    case 4:
                        retVal.R = var3;
                        retVal.G = var1;
                        retVal.B = tV;
                        break;

                    case 5:
                        retVal.R = tV;
                        retVal.G = var1;
                        retVal.B = var2;
                        break;

                    case 6:
                        retVal.R = tV;
                        retVal.G = var3;
                        retVal.B = var1;
                        break;

                    case -1:
                        retVal.R = tV;
                        retVal.G = var1;
                        retVal.B = var2;
                        break;
                }

                if (hdr) return retVal;
                retVal.R = Clamp(retVal.R, 0.0f, 1.0f);
                retVal.G = Clamp(retVal.G, 0.0f, 1.0f);
                retVal.B = Clamp(retVal.B, 0.0f, 1.0f);
            }
            return retVal;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException($"Min Cannot be Greater than Max.  Min: {min}, Max:{max}");
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}