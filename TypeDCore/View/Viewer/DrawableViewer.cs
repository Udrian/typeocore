using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.View.Viewer
{
    public class DrawableViewer
    {
        readonly TypeO FakeTypeO;
        private readonly Task GameTask;
        private int IsClosed;

        private class FakeGame : Game
        {
            private readonly object DrawablesLock = new();

            protected override void Initialize()
            {
                base.Initialize();
            }

            protected override void Cleanup()
            {
                lock (DrawablesLock)
                {
                    Drawables.Clear();
                }
                base.Cleanup();
            }

            public override void Draw()
            {
                lock (DrawablesLock)
                {
                    foreach (var drawable in Drawables.Get<Drawable>())
                    {
                        drawable.Draw(null);
                    }
                }
            }

            public override void Update(double dt)
            {
            }

            public void LoadDrawable(Type drawableType)
            {
                lock (DrawablesLock)
                {
                    Drawables.Create(drawableType);
                }
            }
        }

        private FakeGame Game { get; set; }

        public DrawableViewer(Project project, Component drawable, List<TypeOEngine.Typedeaf.Core.Engine.Module> modules = null)
        {
            if (drawable.TypeOBaseType != typeof(Drawable)) return;
            var typeInfo = project.Assembly.GetType(drawable.FullName);
            if (typeInfo == null) return;

            FakeTypeO = TypeO.Create<FakeGame>("Drawable Viewer") as TypeO;
            Game = FakeTypeO.Context.Game as FakeGame;
            if (modules != null)
            {
                foreach (var module in modules)
                {
                    FakeTypeO.LoadModule(module);
                }
            }
            GameTask = Task.Run(FakeTypeO.Start);
            if (!SpinWait.SpinUntil(() => Game.Initialized || GameTask.IsCompleted, TimeSpan.FromSeconds(30)))
            {
                Game.Exit();
                GameTask.GetAwaiter().GetResult();
                throw new TimeoutException("The drawable viewer game did not initialize within 30 seconds.");
            }

            if (GameTask.IsFaulted)
                GameTask.GetAwaiter().GetResult();
            if (!Game.Initialized)
                throw new InvalidOperationException("The drawable viewer game exited before initialization completed.");

            try
            {
                Game.LoadDrawable(typeInfo);
            }
            catch
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            if (Interlocked.Exchange(ref IsClosed, 1) != 0)
                return;

            Game?.Exit();
            GameTask?.GetAwaiter().GetResult();
        }
    }
}
