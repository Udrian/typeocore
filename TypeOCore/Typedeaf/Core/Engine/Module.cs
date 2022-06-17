namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Base class for Modules, this will be a Module without any Module Options tied to it
        /// </summary>
        public abstract class Module : TypeObject
        {
            /// <summary>
            /// Set to true to call LoadExtensions function during init.
            /// </summary>
            public bool WillLoadExtensions { get; protected set; }

            internal Module() { }

            internal void DoLoadExtensions(TypeO typeO)
            {
                LoadExtensions(typeO);
            }

            /// <summary>
            /// Load all module extensions through the TypeO builder object, will be called before calling Initialize.
            /// </summary>
            /// <param name="typeO">TypeO builder object</param>
            protected abstract void LoadExtensions(TypeO typeO);

            internal virtual void CreateOption() { }
        }

        /// <summary>
        /// Base class for Modules with a ModuleOption.
        /// </summary>
        /// <typeparam name="O">A tied Module Option to this Module</typeparam>
        public abstract class Module<O> : Module where O : ModuleOption, new()
        {
            /// <summary>
            /// The Module options set through the TypeO builder
            /// </summary>
            public O Option { get; protected set; }

            /// <summary>
            /// Do not explicit call constructor.
            /// </summary>
            protected Module() : base() { }

            internal override void CreateOption()
            {
                Option = new O();
            }
        }
    }
}
