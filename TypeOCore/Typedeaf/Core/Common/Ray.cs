using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>Represents a ray with an origin and direction in three-dimensional space.</summary>
        public struct Ray
        {
            /// <summary>Gets or sets the ray's origin.</summary>
            public Vec3 Position { get; set; }
            /// <summary>Gets or sets the ray's direction.</summary>
            public Vec3 Direction { get; set; }

            /// <summary>Initializes a ray with the specified origin and direction.</summary>
            public Ray(Vec3 position, Vec3 direction)
            {
                Position = position;
                Direction = direction;
            }

            /// <summary>Returns the distance to the intersection with a plane, or <see langword="null"/> if none exists.</summary>
            public double? Intersects(Plane plane)
            {
                var den = Vec3.Dot(Direction, plane.Normal);
                if(Math.Abs(den) < 0.00001f)
                {
                    return null;
                }

                var result = (-plane.D - Vec3.Dot(plane.Normal, Position)) / den;

                if(result < 0.0f)
                {
                    if(result < -0.00001f)
                    {
                        return null;
                    }

                    result = 0.0;
                }

                return result;
            }
        }
    }
}
