using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Drawables
    {
        public abstract class Drawable : IDrawable
        {
            public Entity Entity { get; internal set; } //TODO: Change to a anchor
            public bool Hidden { get; set; }
            public int DrawOrder { get; set; }

            protected Drawable() { }

            public abstract void Initialize();
            public abstract void Cleanup();
            public abstract void Draw(ICanvas canvas);
        }
    }
}
