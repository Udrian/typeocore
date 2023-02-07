using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOCoreTest
{
    public class GameTest
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

        [Fact]
        public void CreateGame()
        {
            var typeO = TypeO.Create<TestGame>(GameName) as TypeO;
            Assert.NotNull(typeO);
            Assert.NotNull(typeO.Context);
            Assert.NotNull(typeO.Context.Game);
            Assert.IsType<TestGame>(typeO.Context.Game);
        }
    }
}
