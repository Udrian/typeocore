using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>Represents a color with 8-bit alpha, red, green, and blue channels.</summary>
        public struct Color : IEquatable<Color>
        {
            /// <summary>Gets or sets the alpha channel.</summary>
            public byte A { get; set; }
            /// <summary>Gets or sets the red channel.</summary>
            public byte R { get; set; }
            /// <summary>Gets or sets the green channel.</summary>
            public byte G { get; set; }
            /// <summary>Gets or sets the blue channel.</summary>
            public byte B { get; set; }

            //TODO: This is not super optimal, should maybe create a ColorF class
            /// <summary>Gets or sets the alpha channel as a value scaled to the 0–1 range.</summary>
            public float Af { get { return A / 255f; } set { A = (byte)(value * 255); } }
            /// <summary>Gets or sets the red channel as a value scaled to the 0–1 range.</summary>
            public float Rf { get { return R / 255f; } set { R = (byte)(value * 255); } }
            /// <summary>Gets or sets the green channel as a value scaled to the 0–1 range.</summary>
            public float Gf { get { return G / 255f; } set { G = (byte)(value * 255); } }
            /// <summary>Gets or sets the blue channel as a value scaled to the 0–1 range.</summary>
            public float Bf { get { return B / 255f; } set { B = (byte)(value * 255); } }

            /// <summary>Initializes a color with 8-bit alpha, red, green, and blue channels.</summary>
            public Color(byte a, byte r, byte g, byte b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            /// <summary>Initializes an opaque color with 8-bit red, green, and blue channels.</summary>
            public Color(byte r, byte g, byte b)
            {
                A = 255;
                R = r;
                G = g;
                B = b;
            }

            /// <summary>Initializes a color by converting integer alpha, red, green, and blue values to bytes.</summary>
            public Color(int a, int r, int g, int b)
            {
                A = (byte)a;
                R = (byte)r;
                G = (byte)g;
                B = (byte)b;
            }

            /// <summary>Initializes an opaque color by converting integer red, green, and blue values to bytes.</summary>
            public Color(int r, int g, int b)
            {
                A = 255;
                R = (byte)r;
                G = (byte)g;
                B = (byte)b;
            }

            /// <summary>Initializes a color from normalized alpha, red, green, and blue values scaled to bytes.</summary>
            public Color(double a, double r, double g, double b)
            {
                A = (byte)(a * 255);
                R = (byte)(r * 255);
                G = (byte)(g * 255);
                B = (byte)(b * 255);
            }

            /// <summary>Initializes an opaque color from normalized red, green, and blue values scaled to bytes.</summary>
            public Color(double r, double g, double b)
            {
                A = 255;
                R = (byte)(r * 255);
                G = (byte)(g * 255);
                B = (byte)(b * 255);
            }

            /// <summary>Determines whether two nullable colors have equal channels or are both null.</summary>
            public static bool operator ==(Color? a, Color? b)
            {
                if(a is null)
                {
                    return b is null;
                }
                if(b is null)
                {
                    return a is null;
                }
                return a?.A == b?.A &&
                       a?.R == b?.R &&
                       a?.G == b?.G &&
                       a?.B == b?.B;
            }

            /// <summary>Determines whether two nullable colors differ or exactly one is null.</summary>
            public static bool operator !=(Color? a, Color? b)
            {
                return !(a == b);
            }

            /// <summary>Adds corresponding channels of two colors.</summary>
            public static Color operator +(Color a, Color b)
            {
                return new Color(a.A + b.A, a.R + b.R, a.G + b.G, a.B + b.B);
            }

            /// <summary>Subtracts corresponding channels of the second color from the first.</summary>
            public static Color operator -(Color a, Color b)
            {
                return new Color(a.A - b.A, a.R - b.R, a.G - b.G, a.B - b.B);
            }

            /// <summary>Multiplies corresponding normalized channels of two colors.</summary>
            public static Color operator *(Color a, Color b)
            {
                return new Color((a.A / 255f) * (b.A / 255f), (a.R / 255f) * (b.R / 255f), (a.G / 255f) * (b.G / 255f), (a.B / 255f) * (b.B / 255f));
            }

            /// <summary>Divides corresponding normalized channels of the first color by the second.</summary>
            public static Color operator /(Color a, Color b)
            {
                return new Color((a.A / 255f) / (b.A / 255f), (a.R / 255f) / (b.R / 255f), (a.G / 255f) / (b.G / 255f), (a.B / 255f) / (b.B / 255f));
            }
            /// <summary>Interpolates between two colors, clamping the interpolation amount to the 0–1 range.</summary>
            public static Color Lerp(Color A, Color B, double lerp)
            {
                if(lerp < 0) lerp = 0;
                if(lerp > 1) lerp = 1;
                var invLerp = 1 - lerp;
                return new Color((int)(A.A * invLerp + B.A * lerp), (int)(A.R * invLerp + B.R * lerp), (int)(A.G * invLerp + B.G * lerp), (int)(A.B * invLerp + B.B * lerp));
            }

            /// <summary>Determines whether this color has the same channel values as another color.</summary>
            public bool Equals([AllowNull] Color other)
            {
                return A == other.A && R == other.R && G == other.G && B == other.B;
            }

            /// <summary>Determines whether this color equals the specified object.</summary>
            public override bool Equals(object obj)
            {
                if(obj == null || !(obj is Color)) return false;
                if((Color)obj == this) return true;
                if(obj.GetType() != this.GetType()) return false;
                return Equals((Color)obj);
            }

            /// <summary>Returns the hash code for this color.</summary>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = A;
                    hashCode = (hashCode * 397) ^ R;
                    hashCode = (hashCode * 397) ^ G;
                    hashCode = (hashCode * 397) ^ B;
                    return hashCode;
                }
            }

            /// <summary>Returns the channel values and, when recognized, the matching named color.</summary>
            public override string ToString()
            {
                string colorName = "";

                var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Static);

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(Color))
                    {
                        var val = (property.GetValue(null) as Color?);
                        if (val.HasValue && val.Value == this)
                        {
                            colorName = property.Name;
                            break;
                        }
                    }
                }

                return $"R: {R}, G: {G}, B: {B}, A:{A}" + (string.IsNullOrEmpty(colorName) ? "" : $" - {colorName}");
            }

            /// <summary>Gets a near-black opaque color.</summary>
            public static Color SoftBlack { get { return new Color(255, 20, 20, 20); } }
            /// <summary>Gets opaque black.</summary>
            public static Color Black { get { return new Color(255, 0, 0, 0); } }
            /// <summary>Gets opaque white.</summary>
            public static Color White { get { return new Color(255, 255, 255, 255); } }
            /// <summary>Gets opaque cyan.</summary>
            public static Color Cyan { get { return new Color(255, 0, 255, 255); } }
            /// <summary>Gets a dark cyan color.</summary>
            public static Color DarkCyan { get { return new Color(255, 35, 65, 90); } }
            /// <summary>Gets a light yellow color.</summary>
            public static Color LightYellow { get { return new Color(255, 255, 255, 224); } }
            /// <summary>Gets opaque yellow.</summary>
            public static Color Yellow { get { return new Color(255, 255, 255, 0); } }
            /// <summary>Gets a sun-yellow color.</summary>
            public static Color SunYellow { get { return new Color(255, 229, 209, 152); } }
            /// <summary>Gets a cape-honey color.</summary>
            public static Color CapeHoney { get { return new Color(255, 253, 227, 167); } }
            /// <summary>Gets opaque red.</summary>
            public static Color Red { get { return new Color(255, 255, 0, 0); } }
            /// <summary>Gets opaque green.</summary>
            public static Color Green { get { return new Color(255, 0, 255, 0); } }
            /// <summary>Gets a dark green color.</summary>
            public static Color DarkGreen { get { return new Color(255, 0, 100, 0); } }
            /// <summary>Gets a terrain-green color.</summary>
            public static Color TerrainGreen { get { return new Color(255, 70, 120, 0); } }
            /// <summary>Gets opaque blue.</summary>
            public static Color Blue { get { return new Color(255, 0, 0, 255); } }
            /// <summary>Gets a semi-transparent water-blue color.</summary>
            public static Color WaterBlue { get { return new Color(150, 25, 50, 200); } }
            /// <summary>Gets a dark sky-blue color.</summary>
            public static Color DarkSkyBlue { get { return new Color(255, 10, 15, 50); } }
            /// <summary>Gets a marine-blue color.</summary>
            public static Color MarineBlue { get { return new Color(255, 8, 170, 150); } }
            /// <summary>Gets opaque sky blue.</summary>
            public static Color SkyBlue { get { return new Color(255, 0, 191, 255); } }
            /// <summary>Gets a light gray color.</summary>
            public static Color Gray { get { return new Color(255, 200, 200, 200); } }
            /// <summary>Gets a dark gray color.</summary>
            public static Color DarkGray { get { return new Color(255, 125, 125, 125); } }
            /// <summary>Gets a brown color.</summary>
            public static Color Brown { get { return new Color(255, 130, 60, 0); } }
            /// <summary>Gets a dark brown color.</summary>
            public static Color DarkBrown { get { return new Color(255, 91, 42, 0); } }
            /// <summary>Gets a red-brown color.</summary>
            public static Color RedBrown { get { return new Color(255, 140, 40, 0); } }
            /// <summary>Gets a wood-brown color.</summary>
            public static Color WoodBrown { get { return new Color(255, 193, 154, 107); } }
            /// <summary>Gets opaque orange.</summary>
            public static Color Orange { get { return new Color(255, 255, 140, 0); } }
            /// <summary>Gets fully transparent black.</summary>
            public static Color Transparent { get { return new Color(0, 0, 0, 0); } }
        }
    }
}