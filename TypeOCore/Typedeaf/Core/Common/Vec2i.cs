using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        public struct Vec2i : IEquatable<Vec2i>
        {
            public int X { get; set; }
            public int Y { get; set; }

            public static implicit operator Vec2(Vec2i v) => new Vec2(v.X, v.Y);

            public Vec2i(int xy)
            {
                X = xy;
                Y = xy;
            }

            public Vec2i(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Vec2i(Vec2i vec)
            {
                X = vec.X;
                Y = vec.Y;
            }

            public Vec2i(Vec2 vec)
            {
                X = (int)vec.X;
                Y = (int)vec.Y;
            }

            public Vec2i SetX(int x)
            {
                X = x;
                return this;
            }

            public Vec2i SetX(Vec2i vec)
            {
                X = vec.X;
                return this;
            }

            public Vec2i SetY(int y)
            {
                Y = y;
                return this;
            }

            public Vec2i SetY(Vec2i vec)
            {
                Y = vec.Y;
                return this;
            }

            public Vec2i Set(int x, int y)
            {
                X = x;
                Y = y;
                return this;
            }

            public Vec2i Set(Vec2i vec)
            {
                X = vec.X;
                Y = vec.Y;
                return this;
            }

            public Vec2i TransformX(int x)
            {
                X += x;
                return this;
            }

            public Vec2i TransformX(Vec2i vec)
            {
                X += vec.X;
                return this;
            }

            public Vec2i TransformY(int y)
            {
                Y += y;
                return this;
            }

            public Vec2i TransformY(Vec2i vec)
            {
                Y += vec.Y;
                return this;
            }

            public Vec2i Transform(int x, int y)
            {
                X += x;
                Y += y;
                return this;
            }

            public Vec2i Transform(Vec2i vec)
            {
                X += vec.X;
                Y += vec.Y;
                return this;
            }

            public static Vec2i operator +(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X + b.X, a.Y + b.Y);
            }

            public static Vec2i operator +(Vec2i a, int b)
            {
                return new Vec2i(a.X + b, a.Y + b);
            }

            public static Vec2i operator +(int a, Vec2i b)
            {
                return new Vec2i(a + b.X, a + b.Y);
            }

            public static Vec2i operator -(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X - b.X, a.Y - b.Y);
            }

            public static Vec2i operator -(Vec2i a, int b)
            {
                return new Vec2i(a.X - b, a.Y - b);
            }

            public static Vec2i operator -(int a, Vec2i b)
            {
                return new Vec2i(a - b.X, a - b.Y);
            }

            public static Vec2i operator -(Vec2i a)
            {
                return new Vec2i(-a.X, -a.Y);
            }

            public double LengthSquared()
            {
                return (X * X) + (Y * Y);
            }

            public double Length()
            {
                return Math.Sqrt(LengthSquared());
            }

            public double Distance(Vec2i to)
            {
                return Distance(this, to);
            }

            public double DistanceSquared(Vec2i to)
            {
                return DistanceSquared(this, to);
            }

            public static double Distance(Vec2i from, Vec2i to)
            {
                return Direction(from, to).Length();
            }

            public static double DistanceSquared(Vec2i from, Vec2i to)
            {
                return Direction(from, to).LengthSquared();
            }

            public Vec2i Direction(Vec2i to)
            {
                return new Vec2i(to - this);
            }

            public static Vec2i Direction(Vec2i from, Vec2i to)
            {
                return from.Direction(to);
            }

            public Vec2i Normalize()
            {
                var l = Length();
                if(l <= 0) return this;
                var factor = 1.0 / l;
                X = (int)(X * factor);
                Y = (int)(Y * factor);
                return this;
            }

            public Vec2i Rotate(double radians)
            {
                var cosRadians = Math.Cos(radians);
                var sinRadians = Math.Sin(radians);

                Set((int)(X * cosRadians - Y * sinRadians),
                    (int)(X * sinRadians + Y * cosRadians));

                return this;
            }

            public static Vec2i operator *(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X * b.X, a.Y * b.Y);
            }

            public static Vec2i operator /(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X / b.X, a.Y / b.Y);
            }

            public static Vec2i operator *(Vec2i a, double scalar)
            {
                return new Vec2i((int)(a.X * scalar), (int)(a.Y * scalar));
            }

            public static Vec2i operator *(double scalar, Vec2i a)
            {
                return new Vec2i((int)(a.X * scalar), (int)(a.Y * scalar));
            }

            public static Vec2i operator /(Vec2i a, double scalar)
            {
                return new Vec2i((int)(a.X / scalar), (int)(a.Y / scalar));
            }

            public static Vec2i operator /(double scalar, Vec2i a)
            {
                return new Vec2i((int)(scalar / a.X), (int)(scalar / a.Y));
            }

            public static bool operator ==(Vec2i a, Vec2i b)
            {
                return a.X == b.X && a.Y == b.Y;
            }

            public static bool operator !=(Vec2i a, Vec2i b)
            {
                return !(a == b);
            }

            public double Dot(Vec2i vec)
            {
                return (X * vec.X) + (Y * vec.Y);
            }

            public static double Dot(Vec2i a, Vec2i b)
            {
                return a.Dot(b);
            }

            public static Vec2i Max(Vec2i a, Vec2i b)
            {
                return new Vec2i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            }

            public static Vec2i Min(Vec2i a, Vec2i b)
            {
                return new Vec2i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            }

            public static Vec2i One { get { return new Vec2i(1); } }
            public static Vec2i Zero { get { return new Vec2i(0); } }
            public static Vec2i UnitY { get { return new Vec2i(0, 1); } }
            public static Vec2i UnitX { get { return new Vec2i(1, 0); } }

            public bool Equals(Vec2i other)
            {
                return (X == other.X && Y == other.Y);
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
                    return hash;
                }
            }
        }
    }
}