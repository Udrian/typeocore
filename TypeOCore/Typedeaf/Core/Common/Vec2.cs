using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>A two-dimensional vector with double-precision components.</summary>
        public struct Vec2 : IEquatable<Vec2>
        {
            /// <summary>Gets or sets the horizontal component.</summary>
            public double X { get; set; }
            /// <summary>Gets or sets the vertical component.</summary>
            public double Y { get; set; }

            /// <summary>Converts this vector to an integer vector by truncating its components.</summary>
            public static explicit operator Vec2i(Vec2 v) => new Vec2i((int)v.X, (int)v.Y);

            /// <summary>Initializes both components to the specified value.</summary>
            public Vec2(double xy)
            {
                X = xy;
                Y = xy;
            }

            /// <summary>Initializes the vector with the specified components.</summary>
            public Vec2(double x, double y)
            {
                X = x;
                Y = y;
            }

            /// <summary>Creates a copy of another vector.</summary>
            public Vec2(Vec2 vec)
            {
                X = vec.X;
                Y = vec.Y;
            }

            /// <summary>Creates a vector from the components of an integer vector.</summary>
            public Vec2(Vec2i vec)
            {
                X = vec.X;
                Y = vec.Y;
            }

            /// <summary>Sets the horizontal component and returns this vector.</summary>
            public Vec2 SetX(double x)
            {
                X = x;
                return this;
            }

            /// <summary>Sets the horizontal component from another vector and returns this vector.</summary>
            public Vec2 SetX(Vec2 vec)
            {
                X = vec.X;
                return this;
            }

            /// <summary>Sets the vertical component and returns this vector.</summary>
            public Vec2 SetY(double y)
            {
                Y = y;
                return this;
            }

            /// <summary>Sets the vertical component from another vector and returns this vector.</summary>
            public Vec2 SetY(Vec2 vec)
            {
                Y = vec.Y;
                return this;
            }

            /// <summary>Sets both components and returns this vector.</summary>
            public Vec2 Set(double x, double y)
            {
                X = x;
                Y = y;
                return this;
            }

            /// <summary>Copies both components from another vector and returns this vector.</summary>
            public Vec2 Set(Vec2 vec)
            {
                X = vec.X;
                Y = vec.Y;
                return this;
            }

            /// <summary>Adds a value to the horizontal component and returns this vector.</summary>
            public Vec2 TransformX(double x)
            {
                X += x;
                return this;
            }

            /// <summary>Adds another vector's horizontal component and returns this vector.</summary>
            public Vec2 TransformX(Vec2 vec)
            {
                X += vec.X;
                return this;
            }

            /// <summary>Adds a value to the vertical component and returns this vector.</summary>
            public Vec2 TransformY(double y)
            {
                Y += y;
                return this;
            }

            /// <summary>Adds another vector's vertical component and returns this vector.</summary>
            public Vec2 TransformY(Vec2 vec)
            {
                Y += vec.Y;
                return this;
            }

            /// <summary>Adds the specified offsets to both components and returns this vector.</summary>
            public Vec2 Transform(double x, double y)
            {
                X += x;
                Y += y;
                return this;
            }

            /// <summary>Adds another vector's components and returns this vector.</summary>
            public Vec2 Transform(Vec2 vec)
            {
                X += vec.X;
                Y += vec.Y;
                return this;
            }

            /// <summary>Adds the corresponding components of two vectors.</summary>
            public static Vec2 operator +(Vec2 a, Vec2 b)
            {
                return new Vec2(a.X + b.X, a.Y + b.Y);
            }

            /// <summary>Adds a scalar to both components of a vector.</summary>
            public static Vec2 operator +(Vec2 a, double b)
            {
                return new Vec2(a.X + b, a.Y + b);
            }

            /// <summary>Adds a scalar to both components of a vector.</summary>
            public static Vec2 operator +(double a, Vec2 b)
            {
                return new Vec2(a + b.X, a + b.Y);
            }

            /// <summary>Subtracts the components of the right vector from the left vector.</summary>
            public static Vec2 operator -(Vec2 a, Vec2 b)
            {
                return new Vec2(a.X - b.X, a.Y - b.Y);
            }

            /// <summary>Subtracts a scalar from both components of a vector.</summary>
            public static Vec2 operator -(Vec2 a, double b)
            {
                return new Vec2(a.X - b, a.Y - b);
            }

            /// <summary>Subtracts each vector component from the scalar.</summary>
            public static Vec2 operator -(double a, Vec2 b)
            {
                return new Vec2(a - b.X, a - b.Y);
            }

            /// <summary>Negates both components of a vector.</summary>
            public static Vec2 operator -(Vec2 a)
            {
                return new Vec2(-a.X, -a.Y);
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
            public double Distance(Vec2 to)
            {
                return Distance(this, to);
            }

            /// <summary>Returns the squared distance from this vector to another vector.</summary>
            public double DistanceSquared(Vec2 to)
            {
                return DistanceSquared(this, to);
            }

            /// <summary>Returns the distance between two vectors.</summary>
            public static double Distance(Vec2 from, Vec2 to)
            {
                return Direction(from, to).Length();
            }

            /// <summary>Returns the squared distance between two vectors.</summary>
            public static double DistanceSquared(Vec2 from, Vec2 to)
            {
                return Direction(from, to).LengthSquared();
            }

            /// <summary>Returns the displacement from this vector to another vector.</summary>
            public Vec2 Direction(Vec2 to)
            {
                return new Vec2(to - this);
            }

            /// <summary>Returns the displacement from one vector to another.</summary>
            public static Vec2 Direction(Vec2 from, Vec2 to)
            {
                return from.Direction(to);
            }

            /// <summary>Normalizes this vector in place, leaving a zero vector unchanged.</summary>
            public Vec2 Normalize()
            {
                var l = Length();
                if(l <= 0) return this;
                var factor = 1.0 / l;
                X *= factor;
                Y *= factor;
                return this;
            }

            /// <summary>Rotates this vector counterclockwise by the specified angle in radians.</summary>
            public Vec2 Rotate(double radians)
            {
                var cosRadians = Math.Cos(radians);
                var sinRadians = Math.Sin(radians);

                Set(X * cosRadians - Y * sinRadians,
                    X * sinRadians + Y * cosRadians);

                return this;
            }

            /// <summary>Multiplies corresponding components of two vectors.</summary>
            public static Vec2 operator *(Vec2 a, Vec2 b)
            {
                return new Vec2(a.X * b.X, a.Y * b.Y);
            }

            /// <summary>Divides corresponding components of two vectors.</summary>
            public static Vec2 operator /(Vec2 a, Vec2 b)
            {
                return new Vec2(a.X / b.X, a.Y / b.Y);
            }

            /// <summary>Multiplies each component by a scalar.</summary>
            public static Vec2 operator *(Vec2 a, double scalar)
            {
                return new Vec2(a.X * scalar, a.Y * scalar);
            }

            /// <summary>Multiplies each component of a vector by a scalar.</summary>
            public static Vec2 operator *(double scalar, Vec2 a)
            {
                return new Vec2(a.X * scalar, a.Y * scalar);
            }

            /// <summary>Divides each component by a scalar.</summary>
            public static Vec2 operator /(Vec2 a, double scalar)
            {
                return new Vec2(a.X / scalar, a.Y / scalar);
            }

            /// <summary>Divides a scalar by each component of a vector.</summary>
            public static Vec2 operator /(double scalar, Vec2 a)
            {
                return new Vec2(scalar / a.X, scalar / a.Y);
            }

            /// <summary>Determines whether two vectors have equal components.</summary>
            public static bool operator ==(Vec2 a, Vec2 b)
            {
                return a.X == b.X && a.Y == b.Y;
            }

            /// <summary>Determines whether two vectors differ in any component.</summary>
            public static bool operator !=(Vec2 a, Vec2 b)
            {
                return !(a == b);
            }

            /// <summary>Returns the dot product of this vector and another vector.</summary>
            public double Dot(Vec2 vec)
            {
                return (X * vec.X) + (Y * vec.Y);
            }

            /// <summary>Returns the dot product of two vectors.</summary>
            public static double Dot(Vec2 a, Vec2 b)
            {
                return a.Dot(b);
            }

            /// <summary>Returns a vector containing the larger value in each component.</summary>
            public static Vec2 Max(Vec2 a, Vec2 b)
            {
                return new Vec2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
            }

            /// <summary>Returns a vector containing the smaller value in each component.</summary>
            public static Vec2 Min(Vec2 a, Vec2 b)
            {
                return new Vec2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
            }

            /// <summary>Gets a vector whose components are both one.</summary>
            public static Vec2 One { get { return new Vec2(1); } }
            /// <summary>Gets a vector whose components are both zero.</summary>
            public static Vec2 Zero { get { return new Vec2(0); } }
            /// <summary>Gets the unit vector along the vertical axis.</summary>
            public static Vec2 UnitY { get { return new Vec2(0, 1); } }
            /// <summary>Gets the unit vector along the horizontal axis.</summary>
            public static Vec2 UnitX { get { return new Vec2(1, 0); } }

            /// <summary>Determines whether this vector has the same components as another vector.</summary>
            public bool Equals(Vec2 other)
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