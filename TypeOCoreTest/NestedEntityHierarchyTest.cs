using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeOCoreTest;

public class NestedEntityHierarchyTest
{
    public class NestedGame : Game
    {
        public ParentEntity Root { get; private set; }
        public NestedScene Scene { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            Scene = Scenes.SetScene<NestedScene>();
            Root = Scene.Entities.Create<ParentEntity>();
        }

        protected override void Cleanup() { }
        public override void Update(double dt) => Exit();
        public override void Draw() { }
    }

    public class NestedScene : Scene
    {
        protected override void Initialize() => base.Initialize();
        protected override void Cleanup() { }
        public override void Update(double dt) { }
        public override void Draw() { }
        public override void OnExit(Scene to) { }
        public override void OnEnter(Scene from) { }
    }

    public class ParentEntity : Entity
    {
        public ChildEntity Child { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            Child = Entities.Create<ChildEntity>();
        }

        protected override void Cleanup() => base.Cleanup();
    }

    public class ChildEntity : Entity
    {
        public LeafEntity Grandchild { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();
            Grandchild = Entities.Create<LeafEntity>();
        }

        protected override void Cleanup() => base.Cleanup();
    }

    public class LeafEntity : Entity
    {
        protected override void Initialize() => base.Initialize();
        protected override void Cleanup() => base.Cleanup();
    }

    [Fact]
    public void EntitiesCanContainChildEntitiesRecursively()
    {
        var typeO = Assert.IsType<TypeO>(TypeO.Create<NestedGame>("nested-entity-hierarchy"));
        typeO.Start();

        var game = Assert.IsType<NestedGame>(typeO.Context.Game);
        Assert.NotNull(game.Root);
        Assert.NotNull(game.Root.Entities);
        Assert.NotNull(game.Root.Child);
        Assert.Same(game.Root, game.Root.Child.Parent);
        Assert.NotNull(game.Root.Child.Entities);
        Assert.NotNull(game.Root.Child.Grandchild);
        Assert.Same(game.Root.Child, game.Root.Child.Grandchild.Parent);

        game.Scene.Entities.Update(0);
        Assert.Same(game.Root, Assert.Single(game.Scene.Entities.List<ParentEntity>()));
        Assert.Same(game.Root, game.Scene.Entities.GetEntityByID<ParentEntity>(game.Root.ID));

        game.Root.Remove();
        game.Scene.Entities.Update(0);

        Assert.False(game.Root.Initialized);
        Assert.False(game.Root.Child.Initialized);
        Assert.False(game.Root.Child.Grandchild.Initialized);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void QueuedRemovalsCanBeFlushedBeforeReusingAnEntityId(bool processAdditions)
    {
        var typeO = Assert.IsType<TypeO>(TypeO.Create<NestedGame>("node-undo-redo"));
        typeO.Start();
        var game = (NestedGame)typeO.Context.Game;
        var entity = game.Scene.Entities.Create<ParentEntity>("copied-node");
        if (processAdditions)
            game.Scene.Entities.Update(0);

        entity.Remove();
        game.Scene.Entities.ProcessRemovals();

        Assert.False(entity.Initialized);
        Assert.False(entity.Child.Initialized);
        Assert.False(entity.Child.Grandchild.Initialized);
        var replacement = game.Scene.Entities.Create<ParentEntity>("copied-node");
        game.Scene.Entities.Update(0);
        Assert.Same(replacement, typeO.Context.GetTypeOObjectByID<Entity>(((IHasParent)replacement).FullParentPath + replacement.ID));
        Assert.DoesNotContain(entity, game.Scene.Entities.List<ParentEntity>());
        Assert.Contains(replacement, game.Scene.Entities.List<ParentEntity>());
    }
}
