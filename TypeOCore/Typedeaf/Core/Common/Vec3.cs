using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        public struct Vec3 : IEquatable<Vec3>
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public Vec3(double xyz)
            {
                X = xyz;
                Y = xyz;
                Z = xyz;
            }

            public Vec3(Vec2 v2, double z)
            {
                X = v2.X;
                Y = v2.Y;
                Z = z;
            }

            public Vec3(Vec2 v2)
            {
                X = v2.X;
                Y = v2.Y;
                Z = 0;
            }

            public Vec3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Vec3(Vec3 vec)
            {
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
            }

            public Vec3 SetX(double x)
            {
                X = x;
                return this;
            }
            public Vec3 SetX(Vec3 vec)
            {
                X = vec.X;
                return this;
            }

            public Vec3 SetY(double y)
            {
                Y = y;
                return this;
            }
            public Vec3 SetY(Vec3 vec)
            {
                Y = vec.Y;
                return this;
            }

            public Vec3 SetZ(double z)
            {
                Z = z;
                return this;
            }
            public Vec3 SetZ(Vec3 vec)
            {
                Z = vec.Z;
                return this;
            }

            public Vec3 Set(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
                return this;
            }

            public Vec3 Set(Vec3 vec)
            {
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
                return this;
            }

            public Vec3 TransformX(double x)
            {
                X += x;
                return this;
            }
            public Vec3 TransformX(Vec3 vec)
            {
                X += vec.X;
                return this;
            }

            public Vec3 TransformY(double y)
            {
                Y += y;
                return this;
            }
            public Vec3 TransformY(Vec3 vec)
            {
                Y += vec.Y;
                return this;
            }

            public Vec3 TransformZ(double z)
            {
                Z += z;
                return this;
            }
            public Vec3 TransformZ(Vec3 vec)
            {
                Z += vec.Z;
                return this;
            }

            public Vec3 Transform(double x, double y, double z)
            {
                X += x;
                Y += y;
                Z += z;
                return this;
            }

            public Vec3 Transform(Vec3 vec)
            {
                X += vec.X;
                Y += vec.Y;
                Z += vec.Z;
                return this;
            }

            public static Vec3 operator +(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            public static Vec3 operator -(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            public static Vec3 operator -(Vec3 a)
            {
                return new Vec3(-a.X, -a.Y, -a.Z);
            }

            public double LengthSquared()
            {
                return (X * X) + (Y * Y) + (Z * Z);
            }

            public double Length()
            {
                return Math.Sqrt(LengthSquared());
            }

            public double Distance(Vec3 to)
            {
                return Distance(this, to);
            }

            public double DistanceSquared(Vec3 to)
            {
                return DistanceSquared(this, to);
            }

            public static double Distance(Vec3 from, Vec3 to)
            {
                return (to - from).Length();
            }

            public static double DistanceSquared(Vec3 from, Vec3 to)
            {
                return (to - from).LengthSquared();
            }

            public void Normalize()
            {
                var l = Length();
                if(l <= 0) return;
                var factor = 1.0 / l;
                X *= factor;
                Y *= factor;
                Z *= factor;
            }

            public static Vec3 operator *(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
            }

            public static Vec3 operator /(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
            }

            public static Vec3 operator *(Vec3 a, double scalar)
            {
                return new Vec3(a.X * scalar, a.Y * scalar, a.Z * scalar);
            }

            public static Vec3 operator *(double scalar, Vec3 a)
            {
                return new Vec3(a.X * scalar, a.Y * scalar, a.Z * scalar);
            }

            public static Vec3 operator /(Vec3 a, double scalar)
            {
                return new Vec3(a.X / scalar, a.Y / scalar, a.Z / scalar);
            }

            public static Vec3 operator /(double scalar, Vec3 a)
            {
                return new Vec3(a.X / scalar, a.Y / scalar, a.Z / scalar);
            }

            public static bool operator ==(Vec3 a, Vec3 b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Vec3 a, Vec3 b)
            {
                return !(a == b);
            }

            public double Dot(Vec3 vec)
            {
                return (X * vec.X) + (Y * vec.Y) + (Z * vec.Z);
            }

            public static double Dot(Vec3 a, Vec3 b)
            {
                return a.Dot(b);
            }

            public static Vec3 Max(Vec3 a, Vec3 b)
            {
                return new Vec3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
            }

            public static Vec3 Min(Vec3 a, Vec3 b)
            {
                return new Vec3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
            }

            public Vec3 Cross(Vec3 b)
            {
                return new Vec3((Y * b.Z) - (Z * b.Y),
                                (Z * b.X) - (X * b.Z),
                                (X * b.Y) - (Y * b.X));
            }

            public static Vec3 Cross(Vec3 a, Vec3 b)
            {
                return a.Cross(b);
            }

            public static Vec3 Zero { get { return new Vec3(); } }
            public static Vec3 UnitY { get { return new Vec3(0, 1, 0); } }
            public bool Equals(Vec3 other)
            {
                return (X == other.X && Y == other.Y && Z == other.Z);
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 23;
                    hash = hash * 31 + X.GetHashCode();
                    hash = hash * 31 + Y.GetHashCode();
                    hash = hash * 31 + Z.GetHashCode();
                    return hash;
                }
            }
        }
    }
}