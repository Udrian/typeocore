using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>A three-dimensional vector with double-precision components.</summary>
        public struct Vec3 : IEquatable<Vec3>
        {
            /// <summary>Gets or sets the horizontal component.</summary>
            public double X { get; set; }
            /// <summary>Gets or sets the vertical component.</summary>
            public double Y { get; set; }
            /// <summary>Gets or sets the depth component.</summary>
            public double Z { get; set; }

            /// <summary>Initializes all three components to the specified value.</summary>
            public Vec3(double xyz)
            {
                X = xyz;
                Y = xyz;
                Z = xyz;
            }

            /// <summary>Creates a vector from a two-dimensional vector and a depth value.</summary>
            public Vec3(Vec2 v2, double z)
            {
                X = v2.X;
                Y = v2.Y;
                Z = z;
            }

            /// <summary>Creates a vector from a two-dimensional vector with zero depth.</summary>
            public Vec3(Vec2 v2)
            {
                X = v2.X;
                Y = v2.Y;
                Z = 0;
            }

            /// <summary>Initializes the vector with the specified components.</summary>
            public Vec3(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            /// <summary>Creates a copy of another vector.</summary>
            public Vec3(Vec3 vec)
            {
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
            }

            /// <summary>Sets the horizontal component and returns this vector.</summary>
            public Vec3 SetX(double x)
            {
                X = x;
                return this;
            }
            /// <summary>Sets the horizontal component from another vector and returns this vector.</summary>
            public Vec3 SetX(Vec3 vec)
            {
                X = vec.X;
                return this;
            }

            /// <summary>Sets the vertical component and returns this vector.</summary>
            public Vec3 SetY(double y)
            {
                Y = y;
                return this;
            }
            /// <summary>Sets the vertical component from another vector and returns this vector.</summary>
            public Vec3 SetY(Vec3 vec)
            {
                Y = vec.Y;
                return this;
            }

            /// <summary>Sets the depth component and returns this vector.</summary>
            public Vec3 SetZ(double z)
            {
                Z = z;
                return this;
            }
            /// <summary>Sets the depth component from another vector and returns this vector.</summary>
            public Vec3 SetZ(Vec3 vec)
            {
                Z = vec.Z;
                return this;
            }

            /// <summary>Sets all three components and returns this vector.</summary>
            public Vec3 Set(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
                return this;
            }

            /// <summary>Copies all three components from another vector and returns this vector.</summary>
            public Vec3 Set(Vec3 vec)
            {
                X = vec.X;
                Y = vec.Y;
                Z = vec.Z;
                return this;
            }

            /// <summary>Adds a value to the horizontal component and returns this vector.</summary>
            public Vec3 TransformX(double x)
            {
                X += x;
                return this;
            }
            /// <summary>Adds another vector's horizontal component and returns this vector.</summary>
            public Vec3 TransformX(Vec3 vec)
            {
                X += vec.X;
                return this;
            }

            /// <summary>Adds a value to the vertical component and returns this vector.</summary>
            public Vec3 TransformY(double y)
            {
                Y += y;
                return this;
            }
            /// <summary>Adds another vector's vertical component and returns this vector.</summary>
            public Vec3 TransformY(Vec3 vec)
            {
                Y += vec.Y;
                return this;
            }

            /// <summary>Adds a value to the depth component and returns this vector.</summary>
            public Vec3 TransformZ(double z)
            {
                Z += z;
                return this;
            }
            /// <summary>Adds another vector's depth component and returns this vector.</summary>
            public Vec3 TransformZ(Vec3 vec)
            {
                Z += vec.Z;
                return this;
            }

            /// <summary>Adds the specified offsets to all components and returns this vector.</summary>
            public Vec3 Transform(double x, double y, double z)
            {
                X += x;
                Y += y;
                Z += z;
                return this;
            }

            /// <summary>Adds another vector's components and returns this vector.</summary>
            public Vec3 Transform(Vec3 vec)
            {
                X += vec.X;
                Y += vec.Y;
                Z += vec.Z;
                return this;
            }

            /// <summary>Adds the corresponding components of two vectors.</summary>
            public static Vec3 operator +(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            /// <summary>Subtracts the components of the right vector from the left vector.</summary>
            public static Vec3 operator -(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            /// <summary>Negates all components of a vector.</summary>
            public static Vec3 operator -(Vec3 a)
            {
                return new Vec3(-a.X, -a.Y, -a.Z);
            }

            /// <summary>Returns the squared magnitude of this vector.</summary>
            public double LengthSquared()
            {
                return (X * X) + (Y * Y) + (Z * Z);
            }

            /// <summary>Returns the magnitude of this vector.</summary>
            public double Length()
            {
                return Math.Sqrt(LengthSquared());
            }

            /// <summary>Returns the distance from this vector to another vector.</summary>
            public double Distance(Vec3 to)
            {
                return Distance(this, to);
            }

            /// <summary>Returns the squared distance from this vector to another vector.</summary>
            public double DistanceSquared(Vec3 to)
            {
                return DistanceSquared(this, to);
            }

            /// <summary>Returns the distance between two vectors.</summary>
            public static double Distance(Vec3 from, Vec3 to)
            {
                return (to - from).Length();
            }

            /// <summary>Returns the squared distance between two vectors.</summary>
            public static double DistanceSquared(Vec3 from, Vec3 to)
            {
                return (to - from).LengthSquared();
            }

            /// <summary>Normalizes this vector in place, leaving a zero vector unchanged.</summary>
            public void Normalize()
            {
                var l = Length();
                if(l <= 0) return;
                var factor = 1.0 / l;
                X *= factor;
                Y *= factor;
                Z *= factor;
            }

            /// <summary>Multiplies corresponding components of two vectors.</summary>
            public static Vec3 operator *(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
            }

            /// <summary>Returns the component-wise product of the two vectors.</summary>
            public static Vec3 operator /(Vec3 a, Vec3 b)
            {
                return new Vec3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
            }

            /// <summary>Multiplies each component by a scalar.</summary>
            public static Vec3 operator *(Vec3 a, double scalar)
            {
                return new Vec3(a.X * scalar, a.Y * scalar, a.Z * scalar);
            }

            /// <summary>Multiplies each component of a vector by a scalar.</summary>
            public static Vec3 operator *(double scalar, Vec3 a)
            {
                return new Vec3(a.X * scalar, a.Y * scalar, a.Z * scalar);
            }

            /// <summary>Divides each component by a scalar.</summary>
            public static Vec3 operator /(Vec3 a, double scalar)
            {
                return new Vec3(a.X / scalar, a.Y / scalar, a.Z / scalar);
            }

            /// <summary>Divides a scalar by each component of a vector.</summary>
            public static Vec3 operator /(double scalar, Vec3 a)
            {
                return new Vec3(a.X / scalar, a.Y / scalar, a.Z / scalar);
            }

            /// <summary>Determines whether two vectors have equal components.</summary>
            public static bool operator ==(Vec3 a, Vec3 b)
            {
                return a.Equals(b);
            }

            /// <summary>Determines whether two vectors differ in any component.</summary>
            public static bool operator !=(Vec3 a, Vec3 b)
            {
                return !(a == b);
            }

            /// <summary>Returns the dot product of this vector and another vector.</summary>
            public double Dot(Vec3 vec)
            {
                return (X * vec.X) + (Y * vec.Y) + (Z * vec.Z);
            }

            /// <summary>Returns the dot product of two vectors.</summary>
            public static double Dot(Vec3 a, Vec3 b)
            {
                return a.Dot(b);
            }

            /// <summary>Returns a vector containing the larger value in each component.</summary>
            public static Vec3 Max(Vec3 a, Vec3 b)
            {
                return new Vec3(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
            }

            /// <summary>Returns a vector containing the smaller value in each component.</summary>
            public static Vec3 Min(Vec3 a, Vec3 b)
            {
                return new Vec3(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
            }

            /// <summary>Returns the cross product of this vector and another vector.</summary>
            public Vec3 Cross(Vec3 b)
            {
                return new Vec3((Y * b.Z) - (Z * b.Y),
                                (Z * b.X) - (X * b.Z),
                                (X * b.Y) - (Y * b.X));
            }

            /// <summary>Returns the cross product of two vectors.</summary>
            public static Vec3 Cross(Vec3 a, Vec3 b)
            {
                return a.Cross(b);
            }

            /// <summary>Gets a vector whose components are all zero.</summary>
            public static Vec3 Zero { get { return new Vec3(); } }
            /// <summary>Gets the unit vector along the vertical axis.</summary>
            public static Vec3 UnitY { get { return new Vec3(0, 1, 0); } }
            /// <summary>Determines whether this vector has the same components as another vector.</summary>
            public bool Equals(Vec3 other)
            {
                return (X == other.X && Y == other.Y && Z == other.Z);
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
                    hash = hash * 31 + Z.GetHashCode();
                    return hash;
                }
            }
        }
    }
}