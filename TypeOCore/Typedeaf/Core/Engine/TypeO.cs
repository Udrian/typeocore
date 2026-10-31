using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>Configures and starts a TypeO game runtime.</summary>
        public class TypeO : ITypeO
        {
            /// <summary>Creates a runtime and game context for the specified game type.</summary>
            public static ITypeO Create<G>(string name) where G : Game, new()
            {
                var typeO = new TypeO();
                typeO.Context = new Context(new G(), typeO, name);

                return typeO;
            }

            /// <summary>Gets the context owned by this runtime.</summary>
            public Context Context { get; private set; }

            internal TypeO() : base()
            {
            }

            /// <summary>Registers a service type with the runtime.</summary>
            public ITypeO AddService<S>(string id = "")
                where S : Service, new()
            {
                Context.AddService<S>(id);
                return this;
            }

            /// <summary>Creates and registers a hardware implementation for an interface.</summary>
            public ITypeO AddHardware<I, H>()
                where I : IHardware
                where H : Hardware, new()
            {
                //Instantiate the Hardware
                var hardware = new H();

                Context.Hardwares.Add(typeof(I), hardware);
                return this;
            }

            /// <summary>Maps a content base type to the concrete type used to load it.</summary>
            public ITypeO BindContent<CFrom, CTo>()
                where CFrom : Content
                where CTo : Content, new()
            {

                //TODO: Add checks so that CFrom are inhereting CTo
                Context.ContentBinding.Add(typeof(CFrom), typeof(CTo));

                return this;
            }

            /// <summary>Configures the default logger with the specified minimum log level.</summary>
            public ITypeO SetLogger(LogLevel logLevel = LogLevel.None)
            {
                return SetLogger<DefaultLogger>(new DefaultLoggerOption() { LogLevel = logLevel});
            }

            /// <summary>Creates and configures a logger of the specified type.</summary>
            public ITypeO SetLogger<L>(ILoggerOption option) where L : Logger, new()
            {
                Context.Logger = new L();
                Context.Logger.SetOption(option);
                return this;
            }

            /// <summary>Registers a module instance with optional settings.</summary>
            public ITypeO LoadModule(Module module, ModuleOption option = null, bool loadExtensions = true)
            {
                if (option == null)
                {
                    module.CreateOption();
                }
                else
                {
                    module.GetType().GetProperty("Option").SetValue(module, option);
                }

                //module.GetType().GetProperty("TypeO").SetValue(module, this);
                module.GetType().GetProperty("WillLoadExtensions").SetValue(module, loadExtensions);

                Context.Modules.Add(module);
                return this;
            }

            /// <summary>Creates and registers a module of the specified type.</summary>
            public ITypeO LoadModule<M>(ModuleOption option = null, bool loadExtensions = true) where M : Module, new()
            {
                return LoadModule(new M(), option, loadExtensions);
            }

            /// <summary>Initializes the configured runtime and starts the game.</summary>
            public void Start()
            {
                Context.Start();
            }
        }
    }
}
