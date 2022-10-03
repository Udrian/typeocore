using System;
using System.Collections.Generic;
using System.Reflection;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Services;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        public class Context
        {
            public string Name { get; internal set; }
            public Game Game { get; internal set; }
            public TypeO TypeO { get; internal set; }
            public TimeSpan TimeSinceStart { get; internal set; }
            public DateTime StartTime { get; internal set; }
            public DateTime LastTick { get; internal set; }
            public List<Module> Modules { get; internal set; }
            public Dictionary<Type, Hardware> Hardwares { get; internal set; }
            public Dictionary<Type, Dictionary<string, Service>> Services { get; internal set; }
            public Dictionary<Type, Type> ContentBinding { get; internal set; }
            public ILogger Logger { get; internal set; }

            internal Context(Game game, TypeO typeO, string name) : base()
            {
                Name = name;
                Game = game;
                TypeO = typeO;
                LastTick = DateTime.UtcNow;
                Modules = new List<Module>();
                Hardwares = new Dictionary<Type, Hardware>();
                Services = new Dictionary<Type, Dictionary<string, Service>>();
                ContentBinding = new Dictionary<Type, Type>();
            }

            private bool ExitApplication = false;
            public void Exit()
            {
                ExitApplication = true;
            }

            internal void Start()
            {
                StartTime = DateTime.UtcNow;

                foreach(var module in Modules)
                {
                    if(module.WillLoadExtensions)
                    {
                        module.DoLoadExtensions(TypeO);
                    }
                }

                if(Logger == null)
                {
                    TypeO.SetLogger();
                }

                //We need to set Context here before so that the logger works in InitializeObject
                (Logger as IHasContext)?.SetContext(this);
                InitializeObject(Logger);
                Logger.Log($"Game started at: {StartTime}");
                Logger.Log($"Logger of type '{Logger.GetType().FullName}' loaded");

                //Initialize Hardware
                foreach(var hardware in Hardwares.Values)
                {
                    InitializeObject(hardware);
                    hardware.Initialize();

                    Logger.Log($"Hardware of type '{hardware.GetType().FullName}' loaded");
                }

                //Create Services
                foreach(var serviceIdPair in Services)
                {
                    foreach(var servicePair in serviceIdPair.Value)
                    {
                        var service = servicePair.Value;
                        InitializeObject(service);
                        service.DoInitialize();

                        Logger.Log($"Service of type '{service.GetType().FullName}' loaded");
                    }
                }

                //Set modules Hardware and initialize
                foreach(var module in Modules)
                {
                    InitializeObject(module);
                    module.DoInitialize();

                    Logger.Log($"Module of type '{module.GetType().FullName}' loaded");
                }

                //Setup content binding
                foreach(var binding in ContentBinding)
                {
                    var bindingTo = binding.Value;
                    var bindingFrom = binding.Key;

                    if(!bindingTo.IsSubclassOf(bindingFrom))
                    {
                        var message = $"Content Binding from '{bindingFrom.Name}' must be of a base type to '{bindingTo.Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new ArgumentException(message);
                    }
                }

                //Initialize the game
                InitializeObject(Game);
                Game.InternalInitialize();
                Game.Initialize();
                Game.Initialized = true;

                Logger.Log($"Game of type '{Game.GetType().FullName}' loaded");

                Logger.Log($"Everything loaded successfully, spinning up game loop");
                while(!ExitApplication)
                {
                    var now = DateTime.UtcNow;
                    TimeSinceStart = (now - StartTime);
                    var dt = (now - LastTick).TotalSeconds;
                    LastTick = now;

                    foreach(var module in Modules)
                    {
                        if((module as IUpdatable)?.Pause == false)
                            (module as IUpdatable)?.Update(dt);
                    }

                    foreach(var hardware in Hardwares.Values)
                    {
                        if((hardware as IUpdatable)?.Pause == false)
                            (hardware as IUpdatable)?.Update(dt);
                    }

                    foreach(var serviceids in Services.Values)
                    {
                        foreach(var service in serviceids.Values)
                        {
                            if ((service as IUpdatable)?.Pause == false)
                                (service as IUpdatable)?.Update(dt);
                        }
                    }

                    Game.Update(dt);
                    Game.Draw();
                }
                Logger.Log("Exiting game, initiating cleanup");

                //Cleanup
                Game.Cleanup();

                foreach(var serviceIdPair in Services)
                {
                    foreach(var servicePair in serviceIdPair.Value)
                    {
                        servicePair.Value.DoCleanup();
                    }
                }

                foreach(var hardware in Hardwares)
                {
                    hardware.Value.Cleanup();
                }

                foreach(var module in Modules)
                {
                    module.DoCleanup();
                }

                Logger.Log("Bye bye\n\r\n\r");
                Logger?.Cleanup();
            }

            public void InitializeObject(object obj, object from = null)
            {
                Logger.Log(LogLevel.Debug, $"Initializing obj '{obj.GetType().FullName}'" + (from != null ? $" from '{from.GetType().FullName}'" : ""));

                (obj as IHasContext)?.SetContext(this);
                SetHardwares(obj);
                SetServices(obj);
                SetLogger(obj);

                if(obj is IHasGame)
                {
                    Logger.Log(LogLevel.Ludacris, $"Injecting Game of type '{Game.GetType().FullName}' into {obj.GetType().FullName}");
                    (obj as IHasGame).Game = Game;
                }

                if((obj is IHasData))
                {
                    var hasData = (obj as IHasData);

                    if(obj is Logic && from is IHasData)
                    {
                        (obj as IHasData).EntityData = (from as IHasData).EntityData;
                        Logger.Log(LogLevel.Ludacris, $"Injecting EntityData of type '{(obj as IHasData).EntityData.GetType().FullName}' from '{from.GetType().FullName}' into {obj.GetType().FullName}");
                    }
                    else
                    {
                        hasData.CreateData();
                        if(hasData.EntityData != null)
                        {
                            Logger.Log(LogLevel.Ludacris, $"Creating EntityData of type '{(obj as IHasData).EntityData.GetType().FullName}' into {obj.GetType().FullName}");
                            hasData.EntityData.Initialize();
                        }
                        else
                        {
                            Logger.Log(LogLevel.Ludacris, $"EntityData creation skipped on {obj.GetType().FullName}");
                        }
                    }

                    if((obj as IHasData).EntityData == null)
                    {
                        Logger.Log(LogLevel.Warning, $"EntityData is null in {obj.GetType().FullName}");
                    }
                }

                if(obj is IHasScene)
                {
                    if(from is Scene)
                    {
                        (obj as IHasScene).Scene = from as Scene;
                    }
                    else
                    {
                        (obj as IHasScene).Scene = (from as IHasScene)?.Scene;
                    }
                    Logger.Log(LogLevel.Ludacris, $"Injecting Scene of type '{(obj as IHasScene).Scene?.GetType().FullName}' from '{from.GetType().FullName}' into {obj.GetType().FullName}");
                    if((obj as IHasScene).Scene == null)
                    {
                        Logger.Log(LogLevel.Warning, $"Scene is null in {obj.GetType().FullName}");
                    }
                }

                if(obj is IHasEntity)
                {
                    (obj as IHasEntity).Entity = from as Entity;
                    Logger.Log(LogLevel.Ludacris, $"Injecting Entity of type '{(obj as IHasEntity).Entity?.GetType().FullName}' from '{from.GetType().FullName}' into {obj.GetType().FullName}");

                    if((obj as IHasEntity).Entity == null)
                    {
                        Logger.Log(LogLevel.Warning, $"Entity is null in {obj.GetType().FullName}");
                    }
                }

                if(obj is IHasEntities)
                {
                    var hasEntities = obj as IHasEntities;

                    hasEntities.Entities = new EntityList();
                    Logger.Log(LogLevel.Ludacris, $"Creating EntityList in {obj.GetType().FullName}");
                    InitializeObject(hasEntities.Entities, obj);
                }

                if (obj is TypeObject typeObject)
                {
                    Logger.Log(LogLevel.Debug, $"´Calling Initialize on '{obj.GetType().FullName}'" + (from != null ? $" from '{from.GetType().FullName}'" : ""));
                    typeObject.DoInitialize();
                }
            }

            private void SetHardwares(object obj)
            {
                var type = obj.GetType();
                //TODO: Should set hardware from a attribute
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach(var property in properties)
                {
                    if(property.PropertyType.GetInterface(nameof(IHardware)) == null)
                    {
                        continue;
                    }
                    if(!Hardwares.ContainsKey(property.PropertyType))
                    {
                        var message = $"Hardware type '{property.PropertyType.Name}' is not loaded for '{obj.GetType().Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new InvalidOperationException(message);
                    }

                    Logger.Log(LogLevel.Ludacris, $"Hardware '{Hardwares[property.PropertyType].GetType().FullName}' injected to property '{property.Name}' on object '{obj.GetType().FullName}'");
                    property.SetValue(obj, Hardwares[property.PropertyType]);
                }
            }

            private void SetServices(object obj)
            {
                var type = obj.GetType();
                //TODO: Should set Service from a attribute
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach(var property in properties)
                {
                    if(!property.PropertyType.IsSubclassOf(typeof(Service)))
                    {
                        continue;
                    }
                    if(!Services.ContainsKey(property.PropertyType))
                    {
                        var message = $"Service type '{property.PropertyType.Name}' is not loaded for '{obj.GetType().Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new InvalidOperationException(message);
                    }

                    var serviceId = property.GetCustomAttribute<ServiceId>() ?? new ServiceId();

                    if(!Services[property.PropertyType].ContainsKey(serviceId.Id))
                    {
                        var message = $"Service type '{property.PropertyType.Name}' with ID '{serviceId.Id}' is not loaded for '{obj.GetType().Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new InvalidOperationException(message);
                    }

                    Logger.Log(LogLevel.Ludacris, $"Service '{Services[property.PropertyType][serviceId.Id].GetType().FullName}' injected to property '{property.Name}' on object '{obj.GetType().FullName}'");
                    property.SetValue(obj, Services[property.PropertyType][serviceId.Id]);
                }
            }

            private void SetLogger(object obj)
            {
                var type = obj.GetType();
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach(var property in properties)
                {
                    if(property.PropertyType != typeof(ILogger))
                    {
                        continue;
                    }

                    Logger.Log(LogLevel.Ludacris, $"Logger injected to property '{property.Name}' on object '{obj.GetType().FullName}'");
                    property.SetValue(obj, Logger);
                    break;
                }
            }

            internal Drawable CreateDrawable(Type type, object obj, DrawStack drawStack, DrawableOption<Drawable> option)
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Drawable of type '{type.FullName}' into {obj.GetType().FullName}");

                var drawable = Activator.CreateInstance(type) as Drawable;
                drawable.Entity = obj as Entity;

                InitializeObject(drawable, obj);
                option?.Create(drawable);
                drawable.Initialize();

                if (drawStack != null)
                {
                    drawStack.Push(drawable);
                }

                return drawable;
            }

            internal D CreateDrawable<D>(object obj, DrawStack drawStack, DrawableOption<D> option) where D : Drawable, new()
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Drawable of type '{typeof(D).FullName}' into {obj.GetType().FullName}");

                var drawable = new D()
                {
                    Entity = obj as Entity
                };

                InitializeObject(drawable, obj);
                option?.Create(drawable);
                drawable.Initialize();

                if(drawStack != null)
                {
                    drawStack.Push(drawable);
                }

                return drawable;
            }

            internal static void DestroyDrawable(Drawable drawable, DrawStack drawStack)
            {
                if(drawStack != null)
                {
                    drawStack.Pop(drawable);
                }
                drawable.Cleanup();
            }

            internal L CreateLogic<L>(object obj, UpdateLoop updateLoop, LogicOption<L> option) where L : Logic, new()
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Logic of type '{typeof(L).FullName}' into {obj.GetType().FullName}");

                var logic = new L()
                {
                    Parent = obj as Entity
                };

                InitializeObject(logic, obj);
                option?.Create(logic);
                logic.Initialize();

                if(updateLoop != null)
                {
                    updateLoop.Push(logic);
                }

                return logic;
            }

            internal static void DestroyLogic(Logic logic, UpdateLoop updateLoop)
            {
                if(updateLoop != null)
                {
                    updateLoop.Pop(logic);
                }
            }

            public void AddService<S>(string id = "") where S : Service, new()
            {
                //Instantiate the Service
                var service = new S();
                var ServiceType = typeof(S);

                if(!Services.ContainsKey(ServiceType))
                {
                    Services.Add(ServiceType, new Dictionary<string, Service>());
                }

                if(Services[ServiceType].ContainsKey(id))
                {
                    var message = $"Service of type '{ServiceType.Name}' already have key Id '{id}'";
                    Logger.Log(LogLevel.Fatal, message);
                    throw new ArgumentException(message);
                }

                Services[ServiceType].Add(id, service);
            }

            public S GetService<S>(string id = "") where S : Service, new()
            {
                var ServiceType = typeof(S);
                if(!Services.ContainsKey(ServiceType))
                {
                    Logger.Log(LogLevel.Warning, $"Service of type '{ServiceType.Name}' does not exist");
                    return null;
                }
                if(!Services[ServiceType].ContainsKey(id))
                {
                    Logger.Log(LogLevel.Warning, $"Service of type '{ServiceType.Name}' does not exist with id '{id}'");
                    return null;
                }

                return Services[ServiceType][id] as S;
            }
        }
    }
}
