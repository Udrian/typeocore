using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        public struct Color : IEquatable<Color>
        {
            public byte A { get; set; }
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }

            //TODO: This is not super optimal, should maybe create a ColorF class
            public float Af { get { return A / 255f; } set { A = (byte)(value * 255); } }
            public float Rf { get { return R / 255f; } set { R = (byte)(value * 255); } }
            public float Gf { get { return G / 255f; } set { G = (byte)(value * 255); } }
            public float Bf { get { return B / 255f; } set { B = (byte)(value * 255); } }

            public Color(byte a, byte r, byte g, byte b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            public Color(byte r, byte g, byte b)
            {
                A = 255;
                R = r;
                G = g;
                B = b;
            }

            public Color(int a, int r, int g, int b)
            {
                A = (byte)a;
                R = (byte)r;
                G = (byte)g;
                B = (byte)b;
            }

            public Color(int r, int g, int b)
            {
                A = 255;
                R = (byte)r;
                G = (byte)g;
                B = (byte)b;
            }

            public Color(double a, double r, double g, double b)
            {
                A = (byte)(a * 255);
                R = (byte)(r * 255);
                G = (byte)(g * 255);
                B = (byte)(b * 255);
            }

            public Color(double r, double g, double b)
            {
                A = 255;
                R = (byte)(r * 255);
                G = (byte)(g * 255);
                B = (byte)(b * 255);
            }

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

            public static bool operator !=(Color? a, Color? b)
            {
                return !(a == b);
            }

            public static Color operator +(Color a, Color b)
            {
                return new Color(a.A + b.A, a.R + b.R, a.G + b.G, a.B + b.B);
            }

            public static Color operator -(Color a, Color b)
            {
                return new Color(a.A - b.A, a.R - b.R, a.G - b.G, a.B - b.B);
            }

            public static Color operator *(Color a, Color b)
            {
                return new Color((a.A / 255f) * (b.A / 255f), (a.R / 255f) * (b.R / 255f), (a.G / 255f) * (b.G / 255f), (a.B / 255f) * (b.B / 255f));
            }

            public static Color operator /(Color a, Color b)
            {
                return new Color((a.A / 255f) / (b.A / 255f), (a.R / 255f) / (b.R / 255f), (a.G / 255f) / (b.G / 255f), (a.B / 255f) / (b.B / 255f));
            }
            public static Color Lerp(Color A, Color B, double lerp)
            {
                if(lerp < 0) lerp = 0;
                if(lerp > 1) lerp = 1;
                var invLerp = 1 - lerp;
                return new Color((int)(A.A * invLerp + B.A * lerp), (int)(A.R * invLerp + B.R * lerp), (int)(A.G * invLerp + B.G * lerp), (int)(A.B * invLerp + B.B * lerp));
            }

            public bool Equals([AllowNull] Color other)
            {
                return A == other.A && R == other.R && G == other.G && B == other.B;
            }

            public override bool Equals(object obj)
            {
                if(obj == null) return false;
                if((Color)obj == this) return true;
                if(obj.GetType() != this.GetType()) return false;
                return Equals((Color)obj);
            }

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

            public static Color SoftBlack { get { return new Color(255, 20, 20, 20); } }
            public static Color Black { get { return new Color(255, 0, 0, 0); } }
            public static Color White { get { return new Color(255, 255, 255, 255); } }
            public static Color Cyan { get { return new Color(255, 0, 255, 255); } }
            public static Color DarkCyan { get { return new Color(255, 35, 65, 90); } }
            public static Color LightYellow { get { return new Color(255, 255, 255, 224); } }
            public static Color Yellow { get { return new Color(255, 255, 255, 0); } }
            public static Color SunYellow { get { return new Color(255, 229, 209, 152); } }
            public static Color CapeHoney { get { return new Color(255, 253, 227, 167); } }
            public static Color Red { get { return new Color(255, 255, 0, 0); } }
            public static Color Green { get { return new Color(255, 0, 255, 0); } }
            public static Color DarkGreen { get { return new Color(255, 0, 100, 0); } }
            public static Color TerrainGreen { get { return new Color(255, 70, 120, 0); } }
            public static Color Blue { get { return new Color(255, 0, 0, 255); } }
            public static Color WaterBlue { get { return new Color(150, 25, 50, 200); } }
            public static Color DarkSkyBlue { get { return new Color(255, 10, 15, 50); } }
            public static Color MarineBlue { get { return new Color(255, 8, 170, 150); } }
            public static Color SkyBlue { get { return new Color(255, 0, 191, 255); } }
            public static Color Gray { get { return new Color(255, 200, 200, 200); } }
            public static Color DarkGray { get { return new Color(255, 125, 125, 125); } }
            public static Color Brown { get { return new Color(255, 130, 60, 0); } }
            public static Color DarkBrown { get { return new Color(255, 91, 42, 0); } }
            public static Color RedBrown { get { return new Color(255, 140, 40, 0); } }
            public static Color WoodBrown { get { return new Color(255, 193, 154, 107); } }
            public static Color Orange { get { return new Color(255, 255, 140, 0); } }
            public static Color Transparent { get { return new Color(0, 0, 0, 0); } }
        }
    }
}