using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Services
    {
        public abstract class Service : IHasContext
        {
            Context IHasContext.Context { get; set; }
            protected Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            protected Service() { }

            public abstract void Initialize();
            public abstract void Cleanup();
        }
    }
}
