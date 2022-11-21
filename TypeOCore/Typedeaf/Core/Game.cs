using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    public abstract class Game : IHasContext
    {
        Context IHasContext.Context { get; set; }
        private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

        public string Name { get { return Context.Name; } }

        public DrawableManager<Drawable> Drawables { get; private set; }
        public LogicManager Logics { get; private set; }
        public ContentLoader ContentLoader { get; private set; }
        public SceneList Scenes { get; private set; }
        public IWindow MainWindow { get; set; }
        public ICanvas MainCanvas { get { return MainWindow?.Canvas; } }

        public bool Initialized { get; internal set; }

        protected Game() { }

        internal void InternalInitialize()
        {
            Drawables = new DrawableManager<Drawable>(null, this);
            Context.InitializeObject(Drawables, this);
            Logics = new LogicManager(null, this);
            Context.InitializeObject(Logics, this);
            ContentLoader = new ContentLoader(Context.ContentBinding);
            Context.InitializeObject(ContentLoader);
            Scenes = new SceneList(this);
            Context.InitializeObject(Scenes);
        }

        public abstract void Initialize();
        public abstract void Update(double dt);
        public abstract void Draw();
        public abstract void Cleanup();
        public void Exit() { Context.Exit(); }
    }
}