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
        /// <summary>
        /// Represents an abstract base class for entities within the application, providing core functionality for
        /// managing context, parent-child relationships, and drawable and logic managers.
        /// </summary>
        /// <remarks>The <see cref="Entity"/> class serves as the foundation for all entities in the
        /// application. It provides properties and methods to manage the entity's lifecycle, including initialization,
        /// removal, and interaction with the application's drawing and update systems. Each entity is associated with
        /// a context and can have a parent entity, as well as drawable and logic managers for handling visual and
        /// logical components. This class is designed to be extended by specific entity types, which can override its
        /// behavior as needed. The <see cref="Initialize"/> method sets up the drawable and logic managers, while the 
        /// <see cref="Remove"/> method handles cleanup and removal of the entity from its parent list and  associated
        /// systems.</remarks>
        public abstract class Entity : TypeOObject, IHasContext
        {
            Context IHasContext.Context { get; set; }
            internal Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            /// <summary>
            /// Gets the unique identifier for the entity.
            /// </summary>
            public string ID { get; internal set; }

            /// <summary>
            /// Gets the parent entity of the current entity.
            /// </summary>
            public Entity Parent { get; internal set; }

            internal EntityList ParentEntityList { get; set; } //TODO: This should change to something else

            /// <summary>
            /// Gets the draw stack used to manage and render drawable elements.
            /// </summary>
            /// <remarks>The property is read-only for external callers and can only be set
            /// internally. Ensure that the <see cref="DrawStack"/> instance is properly initialized before accessing
            /// it.</remarks>
            public DrawStack DrawStack { get; internal set; }

            /// <summary>
            /// Gets the update loop instance responsible for managing periodic updates.
            /// </summary>
            public UpdateLoop UpdateLoop { get; internal set; }

            /// <summary>
            /// Gets the <see cref="ContentLoader"/> instance used to load content for the application.
            /// </summary>
            public ContentLoader ContentLoader { get; internal set; }

            /// <summary>
            /// Gets the manager responsible for handling a collection of drawable objects.
            /// </summary>
            public DrawableManager<Drawable> Drawables { get; private set; }

            /// <summary>
            /// Gets the logic manager responsible for handling application-specific logic operations.
            /// </summary>
            public LogicManager Logics { get; private set; }

            /// <summary>
            /// Initializes the drawable and logic managers for the current context.
            /// </summary>
            /// <remarks>This method sets up the <see cref="DrawableManager{T}"/> and
            /// <see cref="LogicManager"/> instances, associating them with the current context. It ensures that the
            /// necessary objects are initialized and ready for use within the application.</remarks>
            protected override void Initialize()
            {
                Drawables = new DrawableManager<Drawable>(DrawStack, this);
                Context.InitializeObject(Drawables, this);
                Logics = new LogicManager(UpdateLoop, this);
                Context.InitializeObject(Logics, this);
            }

            /// <summary>
            /// Removes the current entity from its parent entity list and associated systems.
            /// </summary>
            /// <remarks>This method removes the entity from the draw stack, update loop, and any
            /// associated drawable or logic components. It also marks the entity for deletion and enqueues it for
            /// removal from the parent entity list.</remarks>
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

            /// <summary>
            /// Gets a value indicating whether the item is marked for deletion.
            /// </summary>
            public bool WillBeDeleted { get; private set; }
        }
    }
}