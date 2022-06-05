using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Graphics
    {
        public abstract class Window : IHasContext
        {
            Context IHasContext.Context { get; set; }
            protected Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            public virtual string Title { get; set; }
            public virtual Vec2 Size { get; set; }

            protected Window()
            {
            }

            public abstract void Initialize();
            public abstract void Cleanup();
        }
    }
}