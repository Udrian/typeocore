using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        public abstract class Entity : IHasContext
        {
            Context IHasContext.Context { get; set; }
            internal Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            public string ID { get; internal set; }
            public Entity Parent { get; internal set; }
            internal EntityList ParentEntityList { get; set; } //TODO: This should change to something else
            public DrawStack DrawStack { get; internal set; }
            public UpdateLoop UpdateLoop { get; internal set; }
            public ContentLoader ContentLoader { get; internal set; }

            public DrawableManager<Drawable> Drawables { get; private set; }
            public LogicManager Logics { get; private set; }

            internal virtual void InternalInitialize()
            {
                Drawables = new DrawableManager<Drawable>(DrawStack, this);
                Context.InitializeObject(Drawables, this);
                Logics = new LogicManager(UpdateLoop, this);
                Context.InitializeObject(Logics, this);
            }

            public abstract void Initialize();
            public abstract void Cleanup();

            public virtual void Remove()
            {
                foreach (var drawable in Drawables.Drawables)
                {
                    DrawStack.Pop(drawable);
                }
                foreach (var logic in Logics.Logics)
                {
                    UpdateLoop.Pop(logic);
                }
                DrawStack.Pop(this as IDrawable);
                UpdateLoop.Pop(this as IUpdatable);
                WillBeDeleted = true;
                ParentEntityList.RemoveQueue.Enqueue(this);
            }
            public bool WillBeDeleted { get; private set; }
        }
    }
}