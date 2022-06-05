using TypeOEngine.Typedeaf.Core.Engine;
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

        public bool Initialized { get; internal set; }

        protected Game() { }

        public SceneList CreateSceneHandler()
        {
            var scenes = new SceneList();
            Context.InitializeObject(scenes);
            return scenes;
        }

        internal void InternalInitialize()
        {
            Drawables = new DrawableManager<Drawable>(null, this);
            Context.InitializeObject(Drawables, this);
            Logics = new LogicManager(null, this);
            Context.InitializeObject(Logics, this);
        }

        public abstract void Initialize();
        public abstract void Update(double dt);
        public abstract void Draw();
        public abstract void Cleanup();
        public void Exit() { Context.Exit(); }
    }
}