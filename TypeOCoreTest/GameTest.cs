using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOCoreTest
{
    public class GameTest
    {
        public string GameName { get; set; } = "test";

        public class TestGame : Game
        {
            public bool InitializedForTest { get; private set; }
            public bool UpdatedForTest { get; private set; }
            public bool DrawnForTest { get; private set; }
            public bool CleanedUpForTest { get; private set; }

            protected override void Initialize()
            {
                base.Initialize();
                InitializedForTest = true;
            }

            protected override void Cleanup()
            {
                base.Cleanup();
                CleanedUpForTest = true;
            }

            public override void Update(double dt)
            {
                UpdatedForTest = true;
                Exit();
            }

            public override void Draw()
            {
                DrawnForTest = true;
            }
        }

        [Fact]
        public void CreateGame()
        {
            var typeO = TypeO.Create<TestGame>(GameName) as TypeO;
            Assert.NotNull(typeO);
            Assert.NotNull(typeO.Context);
            Assert.NotNull(typeO.Context.Game);
            var game = Assert.IsType<TestGame>(typeO.Context.Game);

            typeO.Start();

            Assert.True(game.InitializedForTest);
            Assert.True(game.UpdatedForTest);
            Assert.True(game.DrawnForTest);
            Assert.True(game.CleanedUpForTest);
            Assert.False(game.Initialized);
        }
    }
}
