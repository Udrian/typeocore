using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOCoreTest;

public class SceneManagerTests
{
    public sealed class SceneTestGame : Game
    {
        public FirstScene First { get; private set; }
        public SecondScene Second { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            First = Scenes.SetScene<FirstScene>();
        }

        protected override void Cleanup() => base.Cleanup();

        public override void Update(double dt)
        {
            Scenes.Update(dt);
            Second = Scenes.SetScene<SecondScene>();
            Exit();
        }

        public override void Draw() => Scenes.Draw();
    }

    public sealed class FirstScene : Scene
    {
        public int UpdateCount { get; private set; }
        public int DrawCount { get; private set; }
        public Scene ExitedTo { get; private set; }
        public Scene EnteredFrom { get; private set; }

        protected override void Initialize() => base.Initialize();
        protected override void Cleanup() { }
        public override void Update(double dt) => UpdateCount++;
        public override void Draw() => DrawCount++;
        public override void OnExit(Scene to) => ExitedTo = to;
        public override void OnEnter(Scene from) => EnteredFrom = from;
    }

    public sealed class SecondScene : Scene
    {
        public int DrawCount { get; private set; }
        public Scene EnteredFrom { get; private set; }

        protected override void Initialize() => base.Initialize();
        protected override void Cleanup() { }
        public override void Update(double dt) { }
        public override void Draw() => DrawCount++;
        public override void OnExit(Scene to) { }
        public override void OnEnter(Scene from) => EnteredFrom = from;
    }

    [Fact]
    public void SetSceneInitializesAndNotifiesScenesAcrossTransitions()
    {
        var typeO = Assert.IsType<TypeO>(TypeO.Create<SceneTestGame>("scene-manager-test"));
        typeO.Start();

        var game = Assert.IsType<SceneTestGame>(typeO.Context.Game);
        Assert.Same(game.Second, game.Scenes.CurrentScene);
        Assert.Equal(1, game.First.UpdateCount);
        Assert.Equal(0, game.First.DrawCount);
        Assert.Same(game.Second, game.First.ExitedTo);
        Assert.Null(game.First.EnteredFrom);
        Assert.Same(game.First, game.Second.EnteredFrom);
        Assert.Equal(1, game.Second.DrawCount);
        Assert.False(game.First.Initialized);
        Assert.False(game.Second.Initialized);
    }
}
