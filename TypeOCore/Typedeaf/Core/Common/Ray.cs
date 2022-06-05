using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        public struct Ray
        {
            public Vec3 Position { get; set; }
            public Vec3 Direction { get; set; }

            public Ray(Vec3 position, Vec3 direction)
            {
                Position = position;
                Direction = direction;
            }

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
