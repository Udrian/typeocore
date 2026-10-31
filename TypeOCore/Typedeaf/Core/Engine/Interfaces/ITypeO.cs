using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Interfaces
    {
        /// <summary>Defines methods for configuring and starting a TypeO runtime.</summary>
        public interface ITypeO
        {
            /// <summary>Starts the configured game runtime.</summary>
            public void Start();

            /// <summary>Registers a service type with the runtime.</summary>
            public ITypeO AddService<S>(string id = "") where S : Service, new();
            /// <summary>Registers a hardware implementation for an interface.</summary>
            public ITypeO AddHardware<I, H>() where I : IHardware where H : Hardware, new();
            /// <summary>Maps a content base type to a concrete content type.</summary>
            public ITypeO BindContent<CFrom, CTo>() where CFrom : Content where CTo : Content, new();
            /// <summary>Configures the default logger's minimum log level.</summary>
            public ITypeO SetLogger(LogLevel logLevel = LogLevel.Warning);
            /// <summary>Configures a logger of the specified type.</summary>
            public ITypeO SetLogger<L>(ILoggerOption option) where L : Logger, new();
            /// <summary>Creates and registers a module of the specified type.</summary>
            public ITypeO LoadModule<M>(ModuleOption option = null, bool loadExtensions = true) where M : Module, new();
        }
    }
}
