using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class TypeO : ITypeO
        {
            public static ITypeO Create<G>(string name) where G : Game, new()
            {
                var typeO = new TypeO();
                typeO.Context = new Context(new G(), typeO, name);

                return typeO;
            }

            public Context Context { get; private set; }

            internal TypeO() : base()
            {
            }

            public ITypeO AddService<S>(string id = "")
                where S : Service, new()
            {
                Context.AddService<S>(id);
                return this;
            }

            public ITypeO AddHardware<I, H>()
                where I : IHardware
                where H : Hardware, new()
            {
                //Instantiate the Hardware
                var hardware = new H();

                Context.Hardwares.Add(typeof(I), hardware);
                return this;
            }

            public ITypeO BindContent<CFrom, CTo>()
                where CFrom : Content
                where CTo : Content, new()
            {

                //TODO: Add checks so that CFrom are inhereting CTo
                Context.ContentBinding.Add(typeof(CFrom), typeof(CTo));

                return this;
            }

            public ITypeO SetLogger(LogLevel logLevel = LogLevel.None)
            {
                return SetLogger<DefaultLogger>(new DefaultLoggerOption() { LogLevel = logLevel});
            }

            public ITypeO SetLogger<L>(ILoggerOption option) where L : ILogger, new()
            {
                Context.Logger = new L();
                Context.Logger.SetOption(option);
                return this;
            }

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

                module.GetType().GetProperty("TypeO").SetValue(module, this);
                module.GetType().GetProperty("WillLoadExtensions").SetValue(module, loadExtensions);

                Context.Modules.Add(module);
                return this;
            }

            public ITypeO LoadModule<M>(ModuleOption option = null, bool loadExtensions = true) where M : Module, new()
            {
                return LoadModule(new M(), option, loadExtensions);
            }

            public void Start()
            {
                Context.Start();
            }
        }
    }
}
