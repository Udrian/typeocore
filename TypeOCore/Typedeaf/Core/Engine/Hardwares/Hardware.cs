using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Hardwares
    {
        public abstract class Hardware : IHasContext
        {
            Context IHasContext.Context { get; set; }
            protected Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            protected Hardware() { }

            public abstract void Initialize();
            public abstract void Cleanup();
        }
    }
}
