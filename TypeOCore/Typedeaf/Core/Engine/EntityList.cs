using TypeOEngine.Typedeaf.Core.Collections;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Represents a collection of entities that can be managed, updated, and queried within a scene or context.
        /// </summary>
        /// <remarks>The <see cref="EntityList"/> class provides functionality to create, update, and
        /// manage entities within a scene or context. It supports operations such as adding entities, removing
        /// entities, querying entities by type or ID, and creating entities from stubs. This class is designed to work
        /// with the <see cref="Scene"/> and <see cref="Entity"/> systems, and it integrates with update and draw loops
        /// for entities that implement the appropriate interfaces.</remarks>
        public class EntityList : TypeOObject, IHasContext, IHasScene, IHasEntity
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }
            private ILogger Logger { get; set; }

            /// <summary>
            /// Gets or sets the scene associated with the current context.
            /// </summary>
            public Scene Scene { get; set; }

            /// <summary>
            /// Gets or sets the entity associated with this instance.
            /// </summary>
            public Entity Entity { get; set; } //TODO: This maybe should change to something else, OwnerEntity or Node?

            private DelayedList<Entity> Entities { get; set; }

            private DelayedList<IHasEntities> HasEntities { get; set; }

            private Dictionary<Type, IEnumerable<Entity>> EntityLists { get; set; }
            private Dictionary<string, Entity> EntityIDs { get; set; }

            private Dictionary<Type, Stub> Stubs { get; set; }

            internal Queue<Entity> RemoveQueue { get; set; }

            internal EntityList()
            {
                Entities = new DelayedList<Entity>();

                HasEntities = new DelayedList<IHasEntities>();

                EntityLists = new Dictionary<Type, IEnumerable<Entity>>();
                EntityIDs = new Dictionary<string, Entity>();

                Stubs = new Dictionary<Type, Stub>();

                RemoveQueue = new Queue<Entity>();
            }

            protected override void Initialize()
            {
            }

            protected override void Cleanup()
            {
            }

            /// <summary>
            /// Updates the state of all entities in the system and processes pending changes.
            /// </summary>
            /// <remarks>This method performs the following operations: <list type="bullet"> <item>
            /// Removes entities that are queued for deletion, ensuring they are cleaned up and removed from relevant
            /// collections. </item> <item> Updates all entities that are not marked for deletion or paused, propagating
            /// the update to their child entities. </item> <item> Processes any pending operations in the entity
            /// collections to ensure consistency. </item> </list> Entities marked for deletion or paused are skipped
            /// during the update process.</remarks>
            /// <param name="dt">The time elapsed, in seconds, since the last update. This value is used to update entity states.</param>
            public void Update(double dt)
            {
                //Remove entities
                while(RemoveQueue.Count > 0)
                {
                    var deleteEntity = RemoveQueue.Dequeue();
                    for(int j = 0; j < HasEntities.Count; j++)
                    {
                        if(HasEntities[j] == deleteEntity)
                        {
                            HasEntities.RemoveAt(j);
                            break;
                        }
                    }

                    var iType = deleteEntity.GetType();
                    if(EntityLists.ContainsKey(iType))
                    {
                        EntityLists.Remove(iType);
                    }

                    Logger.Log(LogLevel.Debug, $"Removing Entity of type '{iType.FullName}'");
                    deleteEntity.DoCleanup();
                    Entities.Remove(deleteEntity);
                }

                //TODO: Look over this, remove IHasEntities and make Drawstack and UpdateLoop to IUpdatable and IDrawable and create from Entity
                foreach(var entity in HasEntities)
                {
                    if((entity as Entity)?.WillBeDeleted == true) continue;
                    if((entity as IUpdatable)?.Pause == true) continue;
                    entity.Entities.Update(dt);
                }

                Entities.Process();
                HasEntities.Process();
            }

            /// <summary>
            /// Removes all entities from the collection and performs any necessary cleanup.
            /// </summary>
            /// <remarks>This method iterates through all entities in the collection and removes them
            /// individually. After calling this method, the collection will be processed and actually deleted later in the next update loop.</remarks>
            public void Clear()
            {
                foreach(var entity in Entities)
                {
                    entity.Remove();
                }
            }

            /// <summary>
            /// Creates a new instance of the specified <see cref="Entity"/> type and initializes it with the current
            /// context.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> of the entity to create. The type must derive from <see cref="Entity"/>.</param>
            /// <param name="pushToUpdateLoop">A value indicating whether the created entity should be automatically added to the update loop. 
            /// Defaults to <see langword="true"/>.</param>
            /// <param name="pushToDrawStack">A value indicating whether the created entity should be automatically added to the draw stack. Defaults
            /// to <see langword="true"/>.</param>
            /// <returns>The newly created <see cref="Entity"/> instance, initialized with the current context and optionally
            /// added to the update loop and draw stack.</returns>
            public Entity Create(Type type, bool pushToUpdateLoop = true, bool pushToDrawStack = true) //TODO: Split out, Should be able to push automatically to draw stack and update stack
            {
                var entity = Activator.CreateInstance(type) as Entity;
                {
                    entity.Parent = Entity;
                    entity.ParentEntityList = this;
                    entity.DrawStack = Scene?.DrawStack ?? Entity?.DrawStack; //TODO: Change this to be from same interface
                    entity.UpdateLoop = Scene?.UpdateLoop ?? Entity?.UpdateLoop; //TODO: Change this to be from same interface
                    entity.ContentLoader = Scene?.ContentLoader ?? Entity?.ContentLoader; //TODO: Change this to be from same interface
                };

                return Create(entity, pushToUpdateLoop, pushToDrawStack);
            }

            /// <summary>
            /// Creates a new instance of the specified entity type and initializes it with the current context.
            /// </summary>
            /// <remarks>The created entity is automatically associated with the current parent
            /// entity, scene, and relevant stacks (draw and update) based on the current context.</remarks>
            /// <typeparam name="E">The type of entity to create. Must inherit from <see cref="Entity"/> and have a parameterless
            /// constructor.</typeparam>
            /// <param name="pushToUpdateLoop">Indicates whether the created entity should be automatically added to the update loop. The default value
            /// is <see langword="true"/>.</param>
            /// <param name="pushToDrawStack">Indicates whether the created entity should be automatically added to the draw stack. The default value
            /// is <see langword="true"/>.</param>
            /// <returns>A new instance of the specified entity type, initialized with the current context.</returns>
            public E Create<E>(bool pushToUpdateLoop = true, bool pushToDrawStack = true) where E : Entity, new() //TODO: Split out, Should be able to push automatically to draw stack and update stack
            {
                var entity = new E
                {
                    Parent = Entity,
                    ParentEntityList = this,
                    DrawStack = Scene?.DrawStack ?? Entity?.DrawStack, //TODO: Change this to be from same interface
                    UpdateLoop = Scene?.UpdateLoop ?? Entity?.UpdateLoop, //TODO: Change this to be from same interface
                    ContentLoader = Scene?.ContentLoader ?? Entity?.ContentLoader //TODO: Change this to be from same interface
                };

                return Create(entity, pushToUpdateLoop, pushToDrawStack);
            }

            private E Create<E>(E entity, bool pushToUpdateLoop = true, bool pushToDrawStack = true) where E : Entity //TODO: Split out, Should be able to push automatically to draw stack and update stack
            {
                Logger.Log(LogLevel.Debug, $"Creating Entity of type '{typeof(E).FullName}'");
                Context.InitializeObject(entity, this);

                Entities.Add(entity);
                var eType = typeof(E);
                if (EntityLists.ContainsKey(eType))
                {
                    EntityLists[eType] = Entities.Where(e => e is E).Cast<E>().ToList();
                }

                if (string.IsNullOrEmpty(entity.ID))
                {
                    entity.ID = Guid.NewGuid().ToString();
                }
                EntityIDs.Add(entity.ID, entity);

                if (pushToUpdateLoop && entity.UpdateLoop != null && entity is IUpdatable updatable)
                {
                    entity.UpdateLoop.Push(updatable);
                }

                if (pushToDrawStack && entity.DrawStack != null && entity is IDrawable drawable)
                {
                    entity.DrawStack.Push(drawable);
                }

                if (entity is IHasEntities hasEntities)
                {
                    HasEntities.Add(hasEntities);
                }

                return entity;
            }

            /// <summary>
            /// Creates an instance of an <see cref="Entity"/> from a specified stub type.
            /// </summary>
            /// <remarks>If a stub of the specified type does not already exist, a new instance of the
            /// stub is created, initialized, and cached for future use. The stub is then used to create and return the
            /// corresponding entity.</remarks>
            /// <typeparam name="S">The type of the stub used to create the entity. Must inherit from <see cref="Stub"/> and have a
            /// parameterless constructor.</typeparam>
            /// <returns>An <see cref="Entity"/> instance created from the specified stub type.</returns>
            public Entity CreateFromStub<S>() where S : Stub, new() //TODO: Split out
            {
                var sType = typeof(S);
                if(!Stubs.ContainsKey(sType))
                {
                    var nStub = new S();
                    Logger.Log(LogLevel.Debug, $"Creating Stub of type '{typeof(S).FullName}'");
                    Context.InitializeObject(nStub, this);
                    Stubs.Add(sType, nStub);
                }

                var stub = Stubs[sType];
                Logger.Log(LogLevel.Debug, $"Creating Entity from Stub '{typeof(S).FullName}'");
                var entity = stub.CreateEntity(this);
                return entity;
            }

            /// <summary>
            /// Creates an instance of the specified entity type <typeparamref name="E"/> from a stub of type
            /// <typeparamref name="S"/>.
            /// </summary>
            /// <remarks>Logs a warning if the entity creation fails, indicating the types of the stub
            /// and entity involved.</remarks>
            /// <typeparam name="S">The type of the stub used to create the entity. Must inherit from <see cref="Stub{T}"/>.</typeparam>
            /// <typeparam name="E">The type of the entity to create. Must inherit from <see cref="Entity"/>.</typeparam>
            /// <returns>An instance of type <typeparamref name="E"/> created from the stub of type <typeparamref name="S"/>,  or
            /// <see langword="null"/> if the creation fails.</returns>
            public E CreateFromStub<S, E>() where S : Stub<E>, new() where E : Entity, new()
            {
                var entity = CreateFromStub<S>() as E;
                if(entity == null)
                {
                    Logger.Log(LogLevel.Warning, $"Could not create entity '{typeof(E).FullName}' from Stub '{typeof(S).FullName}'");
                }
                return entity;
            }

            /// <summary>
            /// Retrieves a list of entities of the specified type.
            /// </summary>
            /// <remarks>The method caches the results for each entity type to improve performance on
            /// subsequent calls.</remarks>
            /// <typeparam name="E">The type of entity to retrieve. Must derive from <see cref="Entity"/>.</typeparam>
            /// <returns>A list of entities of type <typeparamref name="E"/>. If no entities of the specified type exist,  an
            /// empty list is returned.</returns>
            public List<E> List<E>() where E : Entity
            {
                var eType = typeof(E);
                if(!EntityLists.ContainsKey(eType))
                {
                    EntityLists.Add(eType, Entities.Where(e => e is E).Cast<E>().ToList());
                }

                return EntityLists[eType] as List<E>;
            }

            /// <summary>
            /// Retrieves a list of all entities.
            /// </summary>
            /// <returns>A list containing all entities. The list will be empty if no entities are available.</returns>
            public List<Entity> ListAll()
            {
                return new List<Entity>(Entities);
            }

            /// <summary>
            /// Retrieves an entity of the specified type by its unique identifier.
            /// </summary>
            /// <remarks>If the entity exists but is not of the specified type <typeparamref name="E"/>, a warning is logged.</remarks>
            /// <typeparam name="E">The type of the entity to retrieve. Must derive from <see cref="Entity"/>.</typeparam>
            /// <param name="id">The unique identifier of the entity to retrieve. Cannot be <see langword="null"/> or empty.</param>
            /// <returns>The entity of type <typeparamref name="E"/> associated with the specified identifier, or
            /// <see langword="null"/> if no entity with the given identifier exists or if the entity is not of the
            /// specified type.</returns>
            public E GetEntityByID<E>(string id) where E : Entity
            {
                if(!EntityIDs.ContainsKey(id))
                    return null;
                var entity = EntityIDs[id] as E;
                if(entity == null)
                    Logger.Log(LogLevel.Warning, $"Entity with id '{id}' is not of type '{typeof(E).FullName}'");
                return entity;
            }
        }
    }
}