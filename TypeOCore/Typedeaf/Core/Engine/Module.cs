namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public abstract class Module
        {
            public TypeO TypeO { get; protected set; }
            public bool WillLoadExtensions { get; protected set; }

            internal Module()
            {
            }

            public abstract void Initialize();
            public abstract void Cleanup();
            public abstract void LoadExtensions();

            internal virtual void CreateOption()
            {
                
            }
        }

        public abstract class Module<O> : Module where O : ModuleOption, new()
        {
            public O Option { get; protected set; }

            protected Module() : base() { }

            internal override void CreateOption()
            {
                Option = new O();
            }
        }
    }
}
