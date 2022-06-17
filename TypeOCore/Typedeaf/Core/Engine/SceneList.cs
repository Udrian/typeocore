using System;
using System.Collections.Generic;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core.Engine
{
    public class SceneList : IHasContext //TODO: Maybe change to a "GameContext class"
    {
        Context IHasContext.Context { get; set; }
        protected Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }
        private ILogger Logger { get; set; }

        private Dictionary<Type, Scene> Scenes { get; set; }
        public Scene CurrentScene { get; private set; }
        public IWindow Window { get; set; }
        public Canvas Canvas { get; set; }
        public ContentLoader ContentLoader { get; set; }

        internal SceneList()
        {
            Scenes = new Dictionary<Type, Scene>();
        }

        public void Cleanup()
        {
            if(Window is TypeObject typeObject)
                typeObject?.DoCleanup();
            Canvas?.Cleanup();
            //TODO: Cleanup all scenes?
        }

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
            Context.InitializeObject(scene);
            Scenes.Add(scene.GetType(), scene);

            if (Window == null)
                Logger.Log(LogLevel.Warning, $"Window have not been instantiated to SceneList on '{Context.Game.GetType().FullName}'");
            if (Canvas == null)
                Logger.Log(LogLevel.Warning, $"Canvas have not been instantiated to SceneList on '{Context.Game.GetType().FullName}'");
            if (ContentLoader == null)
                Logger.Log(LogLevel.Warning, $"ContentLoader have not been instantiated to SceneList on '{Context.Game.GetType().FullName}'");

            scene.Scenes = this;
            scene.Window = Window;
            scene.Canvas = Canvas;
            scene.ContentLoader = ContentLoader;
        }

        //TODO: Destroy Scene?
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
                CurrentScene.InternalInitialize();
                CurrentScene.Initialize();
            }
            fromScene?.OnExit(toScene);
            toScene?.OnEnter(fromScene);

            return Scenes[type];
        }

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
                CurrentScene.InternalInitialize();
                CurrentScene.Initialize();
            }
            fromScene?.OnExit(toScene);
            toScene?.OnEnter(fromScene);

            return Scenes[typeof(S)] as S;
        }

        public virtual void Update(double dt)
        {
            CurrentScene?.Update(dt);
        }
        public virtual void Draw()
        {
            CurrentScene?.Draw();
        }
    }
}
