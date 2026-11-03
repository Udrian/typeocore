using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core.Engine
{
    /// <summary>Creates, stores, and switches between a game's scenes.</summary>
    public class SceneManager : TypeOObject, IHasContext //TODO: Maybe change to a "GameContext class"
    {
        Context IHasContext.Context { get; set; }
        /// <summary>Gets the context associated with this scene manager.</summary>
        protected Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }
        private ILogger Logger { get; set; }

        private Dictionary<Type, Scene> Scenes { get; set; }
        private Game Game { get; set; }
        /// <summary>Gets the currently active scene.</summary>
        public Scene CurrentScene { get; private set; }
        /// <summary>Gets the game's main window.</summary>
        public IWindow Window { get => Game.MainWindow; }
        /// <summary>Gets the game's main canvas.</summary>
        public ICanvas Canvas { get => Game.MainCanvas; }
        /// <summary>Gets the game's content loader.</summary>
        public ContentLoader ContentLoader { get => Game.ContentLoader; }

        internal SceneManager(Game game)
        {
            Game = game;
            Scenes = new Dictionary<Type, Scene>();
        }

        /// <summary>Initializes the scene manager.</summary>
        protected override void Initialize()
        {
        }

        /// <summary>Cleans up the window and scenes owned by this manager.</summary>
        protected override void Cleanup()
        {
            if(Window is TypeOObject typeObject)
                typeObject?.DoCleanup();
            foreach (var scene in Scenes)
            {
                scene.Value?.DoCleanup();
            }

            //TODO: Should we cleanup Canvas as well?
        }

        /// <summary>Creates and returns a scene of the specified type.</summary>
        public Scene CreateScene(Type type)
        {
            if (!Scenes.ContainsKey(type))
            {
                Logger.Log(LogLevel.Debug, $"Creating Scene '{type.FullName}'");
                var scene = Activator.CreateInstance(type) as Scene;
                CreateScene(scene);
            }
            return Scenes[type];
        }

        /// <summary>Creates and returns a scene of the specified type.</summary>
        public S CreateScene<S>() where S : Scene, new()
        {
            if(!Scenes.ContainsKey(typeof(S)))
            {
                Logger.Log(LogLevel.Debug, $"Creating Scene '{typeof(S).FullName}'");
                var scene = new S();
                CreateScene(scene);
            }
            return Scenes[typeof(S)] as S;
        }

        private void CreateScene(Scene scene)
        {
            Scenes.Add(scene.GetType(), scene);

            if (Window == null)
                Logger.Log(LogLevel.Warning, $"Window have not been instantiated to SceneManager on '{Context.Game.GetType().FullName}'");
            if (Canvas == null)
                Logger.Log(LogLevel.Warning, $"Canvas have not been instantiated to SceneManager on '{Context.Game.GetType().FullName}'");
            if (ContentLoader == null)
                Logger.Log(LogLevel.Warning, $"ContentLoader have not been instantiated to SceneManager on '{Context.Game.GetType().FullName}'");

            scene.Scenes = this;
            scene.Window = Window;
            scene.Canvas = Canvas;
            scene.ContentLoader = ContentLoader;
        }

        //TODO: Destroy Scene?
        /// <summary>Activates the scene of the specified type and returns it.</summary>
        public Scene SetScene(Type type)
        {
            var init = false;
            if (!Scenes.ContainsKey(type))
            {
                CreateScene(type);
                init = true;
            }
            Logger.Log(LogLevel.Debug, $"Switching to Scene '{type.FullName}'");
            var fromScene = CurrentScene;
            var toScene = Scenes[type];
            CurrentScene = toScene;
            if (init)
            {
                Context.InitializeObject(CurrentScene);
            }
            fromScene?.OnExit(toScene);
            toScene?.OnEnter(fromScene);

            return Scenes[type];
        }

        /// <summary>Activates a scene of the specified type and returns it.</summary>
        public S SetScene<S>() where S : Scene, new()
        {
            var init = false;
            if(!Scenes.ContainsKey(typeof(S)))
            {
                CreateScene<S>();
                init = true;
            }
            Logger.Log(LogLevel.Debug, $"Switching to Scene '{typeof(S).FullName}'");
            var fromScene = CurrentScene;
            var toScene = Scenes[typeof(S)];
            CurrentScene = toScene;
            if(init)
            {
                Context.InitializeObject(CurrentScene);
            }
            fromScene?.OnExit(toScene);
            toScene?.OnEnter(fromScene);

            return Scenes[typeof(S)] as S;
        }

        /// <summary>Updates the active scene.</summary>
        public virtual void Update(double dt)
        {
            CurrentScene?.Update(dt);
        }

        /// <summary>Draws the active scene to the game's canvas.</summary>
        public virtual void Draw()
        {
            Canvas?.PreDraw();
            CurrentScene?.Draw();
            Canvas?.PostDraw();
        }
    }
}
