using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        public interface IAnchor2d
        {
            public Vec2 Position { get; set; }

            public Rectangle ScreenBounds { get; set; }
        }
    }
}
