namespace TypeOEngine.Typedeaf.Core
{
    namespace Common
    {
        public struct Plane
        {
            public Vec3 Normal { get; set; }
            public double D { get; set; }

            public Plane(Vec3 normal, double d)
            {
                Normal = normal;
                Normal.Normalize();
                D = d;
            }
        }
    }
}