using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Drawables
    {
        public abstract class Drawable2d : Drawable
        {
            public Vec2 Position { get; set; }
            public abstract Vec2 Size { get; protected set; }

            public new Entity2d Entity { get { return base.Entity as Entity2d; } internal set { base.Entity = value as Entity2d; } }

            protected Drawable2d() : base()
            {
                Position = Vec2.Zero;
            }
        }
    }
}
