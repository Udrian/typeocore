using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Collections;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>Maintains and draws visible components in draw-order sequence.</summary>
        public class DrawStack : TypeOObject, Interfaces.IHasParent
        {
            private sealed class DrawCommandBuffer
            {
                public List<Action>[] Commands { get; set; } = Array.Empty<List<Action>>();
            }

            private SortedDelayedList<IDrawable> Drawables { get; set; }
            private readonly object _sync = new();
            private readonly Dictionary<IDrawable, long> _drawSequences = new(ReferenceEqualityComparer.Instance);
            private long _nextDrawSequence;
            private readonly object _commandBufferSync = new();
            private readonly Stack<DrawCommandBuffer> _commandBufferPool = new();

            internal DrawThreadPool DrawWorkers { get; set; }

            /// <summary>Gets the scene that owns this draw stack. Null for the scene's default draw stack.</summary>
            public TypeOObject Parent { get; internal set; }

            /// <summary>Gets or sets the order the draw stack is drawn in among the scene's draw stacks; lower values are drawn first.</summary>
            [TypeOProperty("The order this draw stack is drawn in compared to the other draw stacks of the scene, lower values are drawn first.", Group = "DrawStack")]
            public int DrawOrder { get; set; }

            /// <summary>Gets or sets a value indicating whether the stack is drawn in screen space, ignoring the camera translation and scale.</summary>
            [TypeOProperty("Draws everything in this draw stack in screen space, ignoring the camera.", Group = "DrawStack")]
            public bool IgnoreCamera { get; set; }

            /// <summary>Gets the manager that creates drawables drawn by this stack.</summary>
            public DrawableManager<TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable> DrawableManager { get; internal set; }

            internal DrawStack()
            {
                Name = "DrawStack";
                Drawables = new SortedDelayedList<IDrawable>();
            }

            /// <inheritdoc/>
            protected override void Initialize()
            {
            }

            /// <inheritdoc/>
            protected override void Cleanup()
            {
            }

            /// <summary>Draws visible components on workers, replays canvas commands on the calling thread in draw order, and applies queued changes.</summary>
            /// <remarks>Drawable callbacks run concurrently and should not mutate shared game state. Canvas operations are recorded separately per drawable; all workers finish before commands are replayed.
            /// When <see cref="IgnoreCamera"/> is set the canvas camera is reset while drawing, so the drawables are placed in screen space.</remarks>
            public void Draw(ICanvas canvas)
            {
                if (!IgnoreCamera || canvas == null)
                {
                    DrawItems(canvas);
                    return;
                }

                var translation = canvas.WorldTranslation;
                var scale = canvas.WorldScale;
                canvas.WorldTranslation = TypeOEngine.Typedeaf.Core.Common.Vec3.Zero;
                canvas.WorldScale = 1;
                canvas.PreDraw();
                try
                {
                    DrawItems(canvas);
                }
                finally
                {
                    canvas.WorldTranslation = translation;
                    canvas.WorldScale = scale;
                    canvas.PreDraw();
                }
            }

            private void DrawItems(ICanvas canvas)
            {
                IDrawable[] visibleDrawables;
                lock (_sync)
                {
                    SortDrawables();

                    var visibleCount = 0;
                    foreach(var drawable in Drawables)
                        if(!drawable.Hidden)
                            visibleCount++;

                    visibleDrawables = new IDrawable[visibleCount];
                    var visibleIndex = 0;
                    foreach(var drawable in Drawables)
                        if(!drawable.Hidden)
                            visibleDrawables[visibleIndex++] = drawable;
                }

                if (visibleDrawables.Length > 0)
                {
                    if (DrawWorkers == null)
                        throw new InvalidOperationException("The draw stack is not attached to a running TypeO context.");

                    var commandBuffer = RentCommandBuffer(visibleDrawables.Length);
                    try
                    {
                        var commandsByDrawable = commandBuffer.Commands;
                        DrawWorkers.Run(visibleDrawables.Length, index =>
                        {
                            using (CanvasDrawCommandQueue.BeginRecording(commandsByDrawable[index]))
                                visibleDrawables[index].Draw(canvas);
                        });

                        using (CanvasDrawCommandQueue.BeginCommandReplay())
                        {
                            for(var i = 0; i < visibleDrawables.Length; i++)
                            {
                                foreach (var command in commandsByDrawable[i])
                                    CanvasDrawCommandQueue.ExecuteOrEnqueue(command);
                            }
                        }
                    }
                    finally
                    {
                        ReturnCommandBuffer(commandBuffer, visibleDrawables.Length);
                    }
                }

                /*foreach(var entity in HasEntities) //TODO: Don't know yet if we want nested Drawstacks, maybe? look into it
                {
                    if((entity as Entity)?.WillBeDeleted == true) continue;
                    if((entity as IDrawable)?.Hidden == true) continue;
                    entity.Entities.Draw(canvas);
                }*/

                lock (_sync)
                {
                    if (Drawables.ProcessAndReportChanges())
                    {
                        var active = new HashSet<IDrawable>(Drawables, ReferenceEqualityComparer.Instance);
                        foreach (var removed in _drawSequences.Keys.Where(drawable => !active.Contains(drawable)).ToList())
                            _drawSequences.Remove(removed);
                    }
                }
            }

            private void SortDrawables()
            {
                for (var i = 1; i < Drawables.Count; i++)
                {
                    var previous = Drawables[i - 1];
                    var current = Drawables[i];
                    if (previous.DrawOrder < current.DrawOrder ||
                        (previous.DrawOrder == current.DrawOrder && _drawSequences[previous] < _drawSequences[current]))
                        continue;

                    var sorted = Drawables.OrderBy(drawable => drawable.DrawOrder)
                        .ThenBy(drawable => _drawSequences[drawable]).ToArray();
                    for (var index = 0; index < sorted.Length; index++)
                        Drawables[index] = sorted[index];
                    return;
                }
            }

            /// <summary>Returns the currently processed drawables in draw order, with ties ordered by insertion.</summary>
            public IReadOnlyList<IDrawable> GetDrawablesInDrawOrder()
            {
                lock (_sync)
                {
                    SortDrawables();
                    return Drawables.ToArray();
                }
            }

            /// <summary>Returns the currently processed drawables in their insertion order.</summary>
            public IReadOnlyList<IDrawable> GetDrawablesInInsertionOrder()
            {
                lock (_sync)
                    return Drawables.OrderBy(drawable => _drawSequences[drawable]).ToArray();
            }

            private DrawCommandBuffer RentCommandBuffer(int itemCount)
            {
                DrawCommandBuffer buffer;
                lock (_commandBufferSync)
                    buffer = _commandBufferPool.Count == 0 ? new DrawCommandBuffer() : _commandBufferPool.Pop();

                if(buffer.Commands.Length < itemCount)
                {
                    var commands = buffer.Commands;
                    Array.Resize(ref commands, itemCount);
                    buffer.Commands = commands;
                }
                for(var i = 0; i < itemCount; i++)
                {
                    buffer.Commands[i] ??= new List<Action>();
                }

                return buffer;
            }

            private void ReturnCommandBuffer(DrawCommandBuffer buffer, int itemCount)
            {
                for(var i = 0; i < itemCount; i++)
                    buffer.Commands[i].Clear();

                lock (_commandBufferSync)
                    _commandBufferPool.Push(buffer);
            }

            /// <summary>Queues a drawable component to be added to the stack.</summary>
            public void Push(IDrawable drawable)
            {
                if(drawable == null) return;
                lock (_sync)
                {
                    if (!_drawSequences.ContainsKey(drawable))
                        _drawSequences.Add(drawable, _nextDrawSequence++);
                    Drawables.Add(drawable);
                }
            }

            /// <summary>Queues a drawable component to be removed from the stack.</summary>
            public void Pop(IDrawable drawable)
            {
                if(drawable == null) return;
                lock (_sync)
                    Drawables.RemoveDeferred(drawable);
            }
        }
    }
}
