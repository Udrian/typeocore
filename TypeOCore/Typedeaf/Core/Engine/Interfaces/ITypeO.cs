using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        public interface ITypeO
        {
            public void Start();

            public ITypeO AddService<S>(string id = "") where S : Service, new();
            public ITypeO AddHardware<I, H>() where I : IHardware where H : Hardware, new();
            public ITypeO BindContent<CFrom, CTo>() where CFrom : Content where CTo : Content, new();
            public ITypeO SetLogger(LogLevel logLevel = LogLevel.Warning);
            public ITypeO SetLogger<L>(ILoggerOption option) where L : ILogger, new();
            public ITypeO LoadModule<M>(ModuleOption option = null, bool loadExtensions = true) where M : Module, new();
        }
    }
}
