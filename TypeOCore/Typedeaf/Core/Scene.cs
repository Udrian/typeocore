using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    public abstract class Scene : IHasEntities, IHasContext //TODO: have a Scene Cleanup
    {
        Context IHasContext.Context { get; set; }
        private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

        public SceneList Scenes { get; set; }
        public IWindow Window { get; set; }
        public ICanvas Canvas { get; set; }
        public ContentLoader ContentLoader { get; set; }
        public EntityList Entities { get; set; } //TODO: Look over this
        public DrawStack DrawStack { get; private set; } //TODO: Should be able to create draw stack from Game maybe?
        public UpdateLoop UpdateLoop { get; private set; } //TODO: Should be able to create Update loop from Game maybe?

        public DrawableManager<Drawable> Drawables { get; private set; }
        public LogicManager Logics { get; private set; }

        public bool IsInitialized { get; set; } = false;
        public bool Pause         { get; set; } = false;
        public bool Hide          { get; set; } = false;

        protected Scene()
        {
            DrawStack = new DrawStack();
            UpdateLoop = new UpdateLoop();
        }

        internal void InternalInitialize()
        {
            Drawables = new DrawableManager<Drawable>(DrawStack, this);
            Context.InitializeObject(Drawables, this);
            Logics = new LogicManager(UpdateLoop, this);
            Context.InitializeObject(Logics, this);
        }

        public abstract void Initialize();
        public abstract void Update(double dt);
        public abstract void Draw();

        public abstract void OnExit(Scene to);
        public abstract void OnEnter(Scene from);
        }
}
