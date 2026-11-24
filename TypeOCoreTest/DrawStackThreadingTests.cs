using System.Collections.Concurrent;
using System.Threading;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOCoreTest
{
    public class DrawStackThreadingTests
    {
        private sealed class ThreadedDrawGame : Game
        {
            public ThreadedDrawScene Scene { get; private set; }
            public int UpdateCount { get; private set; }
            public ConcurrentBag<int> UpdateThreadIds { get; } = new();
            public ConcurrentBag<int> RenderThreadIds { get; } = new();
            public int CompletedDrawFrames { get; private set; }

            public ThreadedDrawGame()
            {
            }

            protected override void Initialize()
            {
                base.Initialize();
                Scene = Scenes.SetScene<ThreadedDrawScene>();
            }

            protected override void Cleanup()
            {
                base.Cleanup();
            }

            public override void Update(double dt)
            {
                UpdateCount++;
                UpdateThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                Assert.Equal(UpdateCount - 1, CompletedDrawFrames);
                if (UpdateCount == 2)
                    Exit();
            }

            public override void Draw()
            {
                Scene.DrawStack.Draw(null);
                CompletedDrawFrames++;
                RenderThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
            }
        }

        private sealed class ThreadedDrawScene : Scene
        {
            public Barrier DrawBarrier { get; private set; }
            public ConcurrentBag<int> DrawWorkerThreadIds { get; } = new();
            public ConcurrentQueue<int> RenderedOrder { get; } = new();
            public ConcurrentQueue<int> BatchedOrder { get; } = new();
            public object BatchKey { get; } = new();
            public Action<object, IReadOnlyList<int>> BatchFlush { get; }
            public int BatchFlushCount;

            public ThreadedDrawScene()
            {
                BatchFlush = (_, values) =>
                {
                    Interlocked.Increment(ref BatchFlushCount);
                    foreach (var value in values)
                        BatchedOrder.Enqueue(value);
                };
            }

            protected override void Initialize()
            {
                base.Initialize();
                DrawBarrier = new Barrier(2);
                DrawStack.Push(new ThreadedDrawable(0, this));
                DrawStack.Push(new ThreadedDrawable(1, this));
                DrawStack.Draw(null);
            }

            protected override void Cleanup()
            {
                DrawBarrier.Dispose();
            }

            public override void Update(double dt)
            {
            }

            public override void Draw()
            {
            }

            public override void OnExit(Scene to)
            {
            }

            public override void OnEnter(Scene from)
            {
            }
        }

        private sealed class ThreadedDrawable : IDrawable
        {
            private readonly int _drawOrder;
            private readonly ThreadedDrawScene _scene;

            public ThreadedDrawable(int drawOrder, ThreadedDrawScene scene)
            {
                _drawOrder = drawOrder;
                _scene = scene;
            }

            public bool Hidden { get; set; }
            public int DrawOrder { get => _drawOrder; set { } }

            public void Draw(ICanvas canvas)
            {
                _scene.DrawWorkerThreadIds.Add(Thread.CurrentThread.ManagedThreadId);
                if (!_scene.DrawBarrier.SignalAndWait(TimeSpan.FromSeconds(5)))
                    throw new TimeoutException("Drawable calls did not run concurrently.");

                if (!CanvasDrawCommandQueue.TryEnqueue(() =>
                    {
                        _scene.RenderedOrder.Enqueue(_drawOrder);
                        if (!CanvasDrawCommandQueue.TryEnqueueBatch(_scene.BatchKey, _drawOrder, _scene.BatchFlush))
                            throw new InvalidOperationException("The command batch was not enabled during graphics replay.");
                    }))
                    throw new InvalidOperationException("The draw call was not recording canvas commands.");
            }

            public int CompareTo(IDrawable other)
            {
                return DrawOrder.CompareTo(other?.DrawOrder ?? int.MinValue);
            }
        }

        [Fact]
        public void DrawStack_RunsDrawablesOnWorkers_AndReplaysCanvasCommandsInOrderBeforeNextUpdate()
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            var typeO = TypeO.Create<ThreadedDrawGame>("threaded draw") as TypeO;
            typeO.DrawThreadCount = 2;

            typeO.Start();

            var game = Assert.IsType<ThreadedDrawGame>(typeO.Context.Game);
            Assert.Equal(2, game.UpdateCount);
            Assert.Single(game.UpdateThreadIds.Distinct());
            Assert.DoesNotContain(mainThreadId, game.UpdateThreadIds);
            Assert.Equal(2, game.CompletedDrawFrames);
            Assert.Equal(new[] { 0, 1, 0, 1 }, game.Scene.RenderedOrder.ToArray());
            Assert.Equal(2, game.Scene.BatchFlushCount);
            Assert.Equal(new[] { 0, 1, 0, 1 }, game.Scene.BatchedOrder.ToArray());
            Assert.All(game.RenderThreadIds, threadId => Assert.Equal(mainThreadId, threadId));
            Assert.DoesNotContain(mainThreadId, game.Scene.DrawWorkerThreadIds);
        }
    }
}
