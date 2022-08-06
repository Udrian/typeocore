using System;
using System.Collections.Generic;
using System.Linq;
using TypeOEngine.Typedeaf.Core.Collections;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class EntityList : IHasContext, IHasScene, IHasEntity
        {
            Context IHasContext.Context { get; set; }
            private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }
            private ILogger Logger { get; set; }

            public Scene Scene { get; set; }
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
                    deleteEntity.Cleanup();
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
                entity.InternalInitialize();
                entity.Initialize();

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

            public Entity CreateFromStub<S>() where S : Stub, new() //TODO: Split out
            {
                var sType = typeof(S);
                if(!Stubs.ContainsKey(sType))
                {
                    var nStub = new S();
                    Logger.Log(LogLevel.Debug, $"Creating Stub of type '{typeof(S).FullName}'");
                    Context.InitializeObject(nStub, this);
                    nStub.Initialize();
                    Stubs.Add(sType, nStub);
                }

                var stub = Stubs[sType];
                Logger.Log(LogLevel.Debug, $"Creating Entity from Stub '{typeof(S).FullName}'");
                var entity = stub.CreateEntity(this);
                return entity;
            }

            public E CreateFromStub<S, E>() where S : Stub<E>, new() where E : Entity, new()
            {
                var entity = CreateFromStub<S>() as E;
                if(entity == null)
                {
                    Logger.Log(LogLevel.Warning, $"Could not create entity '{typeof(E).FullName}' from Stub '{typeof(S).FullName}'");
                }
                return entity;
            }

            public List<E> List<E>() where E : Entity
            {
                var eType = typeof(E);
                if(!EntityLists.ContainsKey(eType))
                {
                    EntityLists.Add(eType, Entities.Where(e => e is E).Cast<E>().ToList());
                }

                return EntityLists[eType] as List<E>;
            }

            public List<Entity> ListAll()
            {
                return new List<Entity>(Entities);
            }

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