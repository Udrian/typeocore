using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOCoreTest;

public class DrawableManagerTests
{
    public sealed class DrawableTestGame : Game
    {
        public DrawableTestScene TestScene { get; private set; }
        private int _updates;

        protected override void Initialize()
        {
            base.Initialize();
            TestScene = Scenes.SetScene<DrawableTestScene>();
        }

        protected override void Cleanup() => base.Cleanup();

        public override void Update(double dt)
        {
            Scenes.Update(dt);
            if (++_updates == 2)
                Exit();
        }

        public override void Draw() => Scenes.Draw();
    }

    public sealed class DrawableTestScene : Scene
    {
        public DrawableOwner Owner { get; private set; }
        public RecordingDrawable First { get; private set; }
        public RecordingDrawable Second { get; private set; }
        public RecordingDrawable Hidden { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            Owner = Entities.Create<DrawableOwner>("owner", pushToUpdateLoop: false, pushToDrawStack: false);
            First = Owner.Drawables.Create<RecordingDrawable>("first");
            First.DrawOrder = 5;
            Second = Owner.Drawables.Create<RecordingDrawable>("second");
            Second.DrawOrder = 1;
            Hidden = Owner.Drawables.Create<RecordingDrawable>("hidden");
            Hidden.Hidden = true;
        }

        protected override void Cleanup() { }
        public override void Update(double dt) { }
        public override void Draw() => DrawStack.Draw(null);
        public override void OnExit(Scene to) { }
        public override void OnEnter(Scene from) { }
    }

    public sealed class DrawableOwner : Entity
    {
        protected override void Initialize() => base.Initialize();
        protected override void Cleanup() => base.Cleanup();
    }

    public sealed class RecordingDrawable : Drawable
    {
        public int DrawCount { get; private set; }

        protected override void Initialize() { }
        protected override void Cleanup() { }
        public override void Draw(ICanvas canvas) => DrawCount++;
    }

    [Fact]
    public void DrawableManagerCreatesRegistersDrawsLooksUpAndDestroysDrawables()
    {
        var typeO = Assert.IsType<TypeO>(TypeO.Create<DrawableTestGame>("drawable-manager-test"));
        typeO.Start();

        var game = Assert.IsType<DrawableTestGame>(typeO.Context.Game);
        var scene = game.TestScene;
        var owner = scene.Owner;

        Assert.Equal(3, owner.Drawables.Get<RecordingDrawable>().Count());
        Assert.Same(scene.First, owner.Drawables.GetDrawableByID<RecordingDrawable>("owner/first"));
        Assert.Equal(1, scene.First.DrawCount);
        Assert.Equal(1, scene.Second.DrawCount);
        Assert.Equal(0, scene.Hidden.DrawCount);

        owner.Drawables.Destroy(scene.First);
        Assert.False(scene.First.Initialized);
        Assert.Equal(2, owner.Drawables.Get<RecordingDrawable>().Count());
        Assert.Equal(2, owner.Drawables.Destroy<RecordingDrawable>());
        Assert.Empty(owner.Drawables.Get<RecordingDrawable>());
    }
}
