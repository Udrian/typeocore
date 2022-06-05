using System;
using System.Linq;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;
using Xunit;

namespace TypeOCoreTest
{
    public class TypeOTest
    {
        public string GameName { get; set; } = "test";

        public class TestGame : Game
        {
            public override void Initialize()
            {
            }

            public override void Update(double dt)
            {
                Exit();
            }

            public override void Draw()
            {
            }

            public override void Cleanup()
            {
            }
        }

        public class TestGameWithServiceHardware : Game
        {
            public TestServiceWithHardware TestServiceWithHardware { get; set; }

            public override void Initialize()
            {
            }

            public override void Update(double dt)
            {
                Exit();
            }

            public override void Draw()
            {
            }

            public override void Cleanup()
            {
            }
        }

        public class TestGameWithService : Game
        {
            public TestService TestService { get; set; }

            public override void Initialize()
            {
            }

            public override void Update(double dt)
            {
                Exit();
            }

            public override void Draw()
            {
            }

            public override void Cleanup()
            {
            }
        }

        public class TestService : Service
        {
            public override void Cleanup()
            {
            }

            public override void Initialize()
            {
            }
        }

        public class TestServiceWithHardware : Service
        {
            public ITestHardware TestHardware { get; set; }

            public override void Cleanup()
            {
            }

            public override void Initialize()
            {
            }
        }

        public interface ITestHardware : IHardware
        {

        }
        public class TestHardware : Hardware, ITestHardware
        {
            public override void Cleanup()
            {
            }

            public override void Initialize()
            {
            }
        }

        public abstract class BaseContent : Content
        {

        }

        public class SubContent : BaseContent
        {
            public override void Cleanup()
            {
            }

            public override void Load(string path, ContentLoader contentLoader)
            {
            }
        }

        public class TestModuleOption : ModuleOption { }

        public class TestModule : Module<TestModuleOption>
        {
            public TestModule() : base()
            {
            }

            public TestServiceWithHardware TestService { get; set; }
            public ITestHardware TestHardware { get; set; }

            public override void Initialize()
            {
            }

            public override void Cleanup()
            {
            }

            public override void LoadExtensions()
            {
            }
        }

        public class TestRefModule : Module<TestModuleOption>
        {
            public TestRefModule() : base()
            {
            }

            public override void Cleanup()
            {
            }

            public override void Initialize()
            {
            }

            public override void LoadExtensions()
            {
            }
        }

        public class TestLogger : ILogger
        {
            public LogLevel LogLevel { get; set; }

            public void Cleanup()
            {
            }

            public void Log(LogLevel level, string log)
            {
            }
        }

        [Fact]
        public void AddService()
        {
            var typeO = TypeO.Create<TestGameWithService>(GameName)
                .AddService<TestService>() as TypeO;
            typeO.Start();

            var game = typeO.Context.Game as TestGameWithService;
            Assert.NotNull(game.TestService);
            Assert.IsType<TestService>(game.TestService);
            Assert.NotEmpty(typeO.Context.Services);

            typeO = TypeO.Create<TestGameWithService>(GameName)
                .AddService<TestServiceWithHardware>() as TypeO;
            Assert.Throws<InvalidOperationException>(() => typeO.Start());
        }

        [Fact]
        public void AddHardware()
        {
            var typeO = TypeO.Create<TestGameWithServiceHardware>(GameName)
                .AddHardware<ITestHardware, TestHardware>()
                .AddService<TestServiceWithHardware>() as TypeO;
            typeO.Start();

            var game = typeO.Context.Game as TestGameWithServiceHardware;
            Assert.NotNull(game.TestServiceWithHardware.TestHardware);
            Assert.IsType<TestHardware>(game.TestServiceWithHardware.TestHardware);
            Assert.NotEmpty(typeO.Context.Hardwares);
        }

        [Fact]
        public void AddModule()
        {
            var typeO = TypeO.Create<TestGameWithServiceHardware>(GameName)
                             .LoadModule<TestModule>() as TypeO;
            var module = typeO.Context.Modules.FirstOrDefault(m => m.GetType() == typeof(TestModule)) as TestModule;
            Assert.NotNull(module);
            Assert.IsType<TestModule>(module);
            Assert.NotEmpty(typeO.Context.Modules);
            Assert.Null(module.TestService);
            Assert.Null(module.TestHardware);

            typeO
                .AddHardware<ITestHardware, TestHardware>()
                .AddService<TestServiceWithHardware>();

            typeO.Start();

            Assert.NotNull(module.TestService);
            Assert.NotNull(module.TestHardware);

            typeO = TypeO.Create<TestGameWithServiceHardware>(GameName) as TypeO;
            Assert.Throws<InvalidOperationException>(() => typeO.Start());

            typeO = TypeO.Create<TestGameWithServiceHardware>(GameName) as TypeO;
            typeO.LoadModule<TestRefModule>();
            Assert.Throws<InvalidOperationException>(() => typeO.Start());

            typeO = TypeO.Create<TestGameWithServiceHardware>(GameName) as TypeO;
            typeO.LoadModule<TestRefModule>();
            typeO.LoadModule<TestModule>();
            Assert.Throws<InvalidOperationException>(() => typeO.Start());
        }

        [Fact]
        public void BindContent()
        {
            var typeO = TypeO.Create<TestGame>(GameName)
                .BindContent<BaseContent, SubContent>() as TypeO;
            typeO.Start();

            Assert.NotEmpty(typeO.Context.ContentBinding);
            Assert.NotNull(typeO.Context.ContentBinding[typeof(BaseContent)]);
        }

        [Fact]
        public void LoggerTest()
        {
            var typeO = TypeO.Create<TestGame>(GameName) as TypeO;
            typeO.Start();

            Assert.NotNull(typeO.Context.Logger);
            Assert.IsType<DefaultLogger>(typeO.Context.Logger);

            typeO = TypeO.Create<TestGame>(GameName)
                .SetLogger(LogLevel.None) as TypeO;
            typeO.Start();

            Assert.NotNull(typeO.Context.Logger);
            Assert.IsType<DefaultLogger>(typeO.Context.Logger);
            Assert.Equal(LogLevel.None, typeO.Context.Logger.LogLevel);

            typeO = TypeO.Create<TestGame>(GameName)
                .SetLogger<TestLogger>(LogLevel.None) as TypeO;
            typeO.Start();

            Assert.NotNull(typeO.Context.Logger);
            Assert.IsType<TestLogger>(typeO.Context.Logger);
            Assert.Equal(LogLevel.None, typeO.Context.Logger.LogLevel);
        }
    }
}
