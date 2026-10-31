namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        /// <summary>Represents a plane defined by a normal vector and distance from the origin.</summary>
        public struct Plane
        {
            /// <summary>Gets or sets the normalized normal vector of the plane.</summary>
            public Vec3 Normal { get; set; }
            /// <summary>Gets or sets the plane's signed distance parameter.</summary>
            public double D { get; set; }

            /// <summary>Initializes a plane from a normal vector and distance parameter.</summary>
            public Plane(Vec3 normal, double d)
            {
                Normal = normal;
                Normal.Normalize();
                D = d;
            }
        }
    }
}