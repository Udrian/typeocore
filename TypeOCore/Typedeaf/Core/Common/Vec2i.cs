using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>A two-dimensional vector with integer components.</summary>
        public struct Vec2i : IEquatable<Vec2i>
        {
            /// <summary>Gets or sets the horizontal component.</summary>
            public int X { get; set; }
            /// <summary>Gets or sets the vertical component.</summary>
            public int Y { get; set; }

            /// <summary>Converts this integer vector to a double-precision vector.</summary>
            public static implicit operator Vec2(Vec2i v) => new Vec2(v.X, v.Y);

            /// <summary>Initializes both components to the specified value.</summary>
            public Vec2i(int xy)
            {
                X = xy;
                Y = xy;
            }

            /// <summary>Initializes the vector with the specified components.</summary>
            public Vec2i(int x, int y)
            {
                X = x;
                Y = y;
            }

            /// <summary>Creates a copy of another integer vector.</summary>
            public Vec2i(Vec2i vec)
            {
                X = vec.X;
                Y = vec.Y;
            }

            /// <summary>Creates an integer vector by truncating another vector's components.</summary>
            public Vec2i(Vec2 vec)
            {
                X = (int)vec.X;
                Y = (int)vec.Y;
            }

            /// <summary>Sets the horizontal component and returns this vector.</summary>
            public Vec2i SetX(int x)
            {
                X = x;
                return this;
            }

            /// <summary>Sets the horizontal component from another vector and returns this vector.</summary>
            public Vec2i SetX(Vec2i vec)
            {
                X = vec.X;
                return this;
            }

            /// <summary>Sets the vertical component and returns this vector.</summary>
            public Vec2i SetY(int y)
            {
                Y = y;
                return this;
            }

            /// <summary>Sets the vertical component from another vector and returns this vector.</summary>
            public Vec2i SetY(Vec2i vec)
            {
                Y = vec.Y;
                return this;
            }

            /// <summary>Sets both components and returns this vector.</summary>
            public Vec2i Set(int x, int y)
            {
                X = x;
                Y = y;
                return this;
            }

            /// <summary>Copies both components from another vector and returns this vector.</summary>
            public Vec2i Set(Vec2i vec)
            {
                X = vec.X;
                Y = vec.Y;
                return this;
            }

            /// <summary>Adds a value to the horizontal component and returns this vector.</summary>
            public Vec2i TransformX(int x)
            {
                X += x;
                return this;
            }

            /// <summary>Adds another vector's horizontal component and returns this vector.</summary>
            public Vec2i TransformX(Vec2i vec)
            {
                X += vec.X;
                return this;
            }

            /// <summary>Adds a value to the vertical component and returns this vector.</summary>
            public Vec2i TransformY(int y)
            {
                Y += y;
                return this;
            }

            /// <summary>Adds another vector's vertical component and returns this vector.</summary>
            public Vec2i TransformY(Vec2i vec)
            {
                Y += vec.Y;
                return this;
            }

            /// <summary>Adds the specified offsets to both components and returns this vector.</summary>
            public Vec2i Transform(int x, int y)
            {
                X += x;
                Y += y;
                return this;
            }

            /// <summary>Adds another vector's components and returns this vector.</summary>
            public Vec2i Transform(Vec2i vec)
            {
                X += vec.X;
                Y += vec.Y;
                return this;
            }

            /// <summary>Adds the corresponding components of two vectors.</summary>
            public static Vec2i operator +(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X + b.X, a.Y + b.Y);
            }

            /// <summary>Adds an integer to both components of a vector.</summary>
            public static Vec2i operator +(Vec2i a, int b)
            {
                return new Vec2i(a.X + b, a.Y + b);
            }

            /// <summary>Adds an integer to both components of a vector.</summary>
            public static Vec2i operator +(int a, Vec2i b)
            {
                return new Vec2i(a + b.X, a + b.Y);
            }

            /// <summary>Subtracts the components of the right vector from the left vector.</summary>
            public static Vec2i operator -(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X - b.X, a.Y - b.Y);
            }

            /// <summary>Subtracts an integer from both components of a vector.</summary>
            public static Vec2i operator -(Vec2i a, int b)
            {
                return new Vec2i(a.X - b, a.Y - b);
            }

            /// <summary>Subtracts each vector component from the integer.</summary>
            public static Vec2i operator -(int a, Vec2i b)
            {
                return new Vec2i(a - b.X, a - b.Y);
            }

            /// <summary>Negates both components of a vector.</summary>
            public static Vec2i operator -(Vec2i a)
            {
                return new Vec2i(-a.X, -a.Y);
            }

            /// <summary>Returns the squared magnitude of this vector.</summary>
            public double LengthSquared()
            {
                return (X * X) + (Y * Y);
            }

            /// <summary>Returns the magnitude of this vector.</summary>
            public double Length()
            {
                return Math.Sqrt(LengthSquared());
            }

            /// <summary>Returns the distance from this vector to another vector.</summary>
            public double Distance(Vec2i to)
            {
                return Distance(this, to);
            }

            /// <summary>Returns the squared distance from this vector to another vector.</summary>
            public double DistanceSquared(Vec2i to)
            {
                return DistanceSquared(this, to);
            }

            /// <summary>Returns the distance between two vectors.</summary>
            public static double Distance(Vec2i from, Vec2i to)
            {
                return Direction(from, to).Length();
            }

            /// <summary>Returns the squared distance between two vectors.</summary>
            public static double DistanceSquared(Vec2i from, Vec2i to)
            {
                return Direction(from, to).LengthSquared();
            }

            /// <summary>Returns the displacement from this vector to another vector.</summary>
            public Vec2i Direction(Vec2i to)
            {
                return new Vec2i(to - this);
            }

            /// <summary>Returns the displacement from one vector to another.</summary>
            public static Vec2i Direction(Vec2i from, Vec2i to)
            {
                return from.Direction(to);
            }

            /// <summary>Normalizes this vector in place, truncating components to integers.</summary>
            public Vec2i Normalize()
            {
                var l = Length();
                if(l <= 0) return this;
                var factor = 1.0 / l;
                X = (int)(X * factor);
                Y = (int)(Y * factor);
                return this;
            }

            /// <summary>Rotates this vector by the specified angle in radians, truncating components to integers.</summary>
            public Vec2i Rotate(double radians)
            {
                var cosRadians = Math.Cos(radians);
                var sinRadians = Math.Sin(radians);

                Set((int)(X * cosRadians - Y * sinRadians),
                    (int)(X * sinRadians + Y * cosRadians));

                return this;
            }

            /// <summary>Multiplies corresponding components of two vectors.</summary>
            public static Vec2i operator *(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X * b.X, a.Y * b.Y);
            }

            /// <summary>Divides corresponding components of two vectors using integer division.</summary>
            public static Vec2i operator /(Vec2i a, Vec2i b)
            {
                return new Vec2i(a.X / b.X, a.Y / b.Y);
            }

            /// <summary>Multiplies each component by a scalar and truncates the results.</summary>
            public static Vec2i operator *(Vec2i a, double scalar)
            {
                return new Vec2i((int)(a.X * scalar), (int)(a.Y * scalar));
            }

            /// <summary>Multiplies each component of a vector by a scalar and truncates the results.</summary>
            public static Vec2i operator *(double scalar, Vec2i a)
            {
                return new Vec2i((int)(a.X * scalar), (int)(a.Y * scalar));
            }

            /// <summary>Divides each component by a scalar and truncates the results.</summary>
            public static Vec2i operator /(Vec2i a, double scalar)
            {
                return new Vec2i((int)(a.X / scalar), (int)(a.Y / scalar));
            }

            /// <summary>Divides a scalar by each component and truncates the results.</summary>
            public static Vec2i operator /(double scalar, Vec2i a)
            {
                return new Vec2i((int)(scalar / a.X), (int)(scalar / a.Y));
            }

            /// <summary>Determines whether two vectors have equal components.</summary>
            public static bool operator ==(Vec2i a, Vec2i b)
            {
                return a.X == b.X && a.Y == b.Y;
            }

            /// <summary>Determines whether two vectors differ in any component.</summary>
            public static bool operator !=(Vec2i a, Vec2i b)
            {
                return !(a == b);
            }

            /// <summary>Returns the dot product of this vector and another vector.</summary>
            public double Dot(Vec2i vec)
            {
                return (X * vec.X) + (Y * vec.Y);
            }

            /// <summary>Returns the dot product of two vectors.</summary>
            public static double Dot(Vec2i a, Vec2i b)
            {
                return a.Dot(b);
            }

            /// <summary>Returns a vector containing the larger value in each component.</summary>
            public static Vec2i Max(Vec2i a, Vec2i b)
            {
                return new Vec2i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            }

            /// <summary>Returns a vector containing the smaller value in each component.</summary>
            public static Vec2i Min(Vec2i a, Vec2i b)
            {
                return new Vec2i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            }

            /// <summary>Gets an integer vector whose components are both one.</summary>
            public static Vec2i One { get { return new Vec2i(1); } }
            /// <summary>Gets an integer vector whose components are both zero.</summary>
            public static Vec2i Zero { get { return new Vec2i(0); } }
            /// <summary>Gets the unit integer vector along the vertical axis.</summary>
            public static Vec2i UnitY { get { return new Vec2i(0, 1); } }
            /// <summary>Gets the unit integer vector along the horizontal axis.</summary>
            public static Vec2i UnitX { get { return new Vec2i(1, 0); } }

            /// <summary>Determines whether this vector has the same components as another vector.</summary>
            public bool Equals(Vec2i other)
            {
                return (X == other.X && Y == other.Y);
            }

            /// <summary>Determines whether this vector equals the specified object.</summary>
            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            /// <summary>Returns the hash code for this vector.</summary>
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