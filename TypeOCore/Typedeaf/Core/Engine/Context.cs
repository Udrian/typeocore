using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Collections.Concurrent;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares;
using TypeOEngine.Typedeaf.Core.Engine.Hardwares.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;
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
        /// <summary>Holds the runtime state and services for a running TypeO game.</summary>
        public class Context
        {
            /// <summary>Gets the name assigned to this game context.</summary>
            public string Name { get; internal set; }
            /// <summary>Gets the game instance associated with this context.</summary>
            public Game Game { get; internal set; }
            /// <summary>Gets the TypeO runtime that owns this context.</summary>
            public TypeO TypeO { get; internal set; }
            /// <summary>Gets the elapsed time since the game started.</summary>
            public TimeSpan TimeSinceStart { get; internal set; }
            /// <summary>Gets the time at which the game started.</summary>
            public DateTime StartTime { get; internal set; }
            /// <summary>Gets the time of the previous game update.</summary>
            public DateTime LastTick { get; internal set; }
            /// <summary>Gets the modules loaded into this context.</summary>
            public List<Module> Modules { get; internal set; }
            /// <summary>Gets the hardware instances registered by interface type.</summary>
            public Dictionary<Type, Hardware> Hardwares { get; internal set; }
            /// <summary>Gets the registered services, grouped by service type and identifier.</summary>
            public Dictionary<Type, Dictionary<string, Service>> Services { get; internal set; }
            /// <summary>Gets the mappings from content base types to concrete content types.</summary>
            public Dictionary<Type, Type> ContentBinding { get; internal set; }
            private Dictionary<string, TypeOObject> IDToTypeOObjectMap { get; set; }
            private readonly ConcurrentDictionary<Type, InjectionProperties> _injectionProperties = new();
            /// <summary>Gets the logger used by this context.</summary>
            public Logger Logger { get; internal set; }
            internal DrawThreadPool DrawWorkers { get; private set; }

            private readonly AutoResetEvent _updateRequested = new(false);
            private readonly AutoResetEvent _updateCompleted = new(false);
            private readonly AutoResetEvent _drawCompleted = new(false);
            private readonly AutoResetEvent _mainThreadUpdateRequested = new(false);
            private readonly AutoResetEvent _mainThreadUpdateCompleted = new(false);
            private readonly SemaphoreSlim _processGameLock = new(1, 1);
            private Thread _updateThread;
            private volatile bool _stopUpdateThread;
            private double _pendingDelta;
            private bool _shouldDrawFrame;
            private ExceptionDispatchInfo _updateException;
            private Action _mainThreadUpdateAction;
            private ExceptionDispatchInfo _mainThreadUpdateException;

            private sealed class InjectionProperties
            {
                public PropertyInfo[] Hardwares { get; init; }
                public (PropertyInfo Property, string Id)[] Services { get; init; }
                public PropertyInfo Logger { get; init; }
            }

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
                IDToTypeOObjectMap = new Dictionary<string, TypeOObject>();
            }

            internal Entity CreateEntity(Type type, EntityList parentList, bool pushToUpdateLoop = true, bool pushToDrawStack = true, string id = null)
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Entity of type '{type.FullName}' into {parentList.GetType().FullName}");

                var entity = Activator.CreateInstance(type) as Entity;

                return CreateEntity(entity, parentList, id, pushToUpdateLoop, pushToDrawStack);
            }

            internal E CreateEntity<E>(EntityList parentList, bool pushToUpdateLoop = true, bool pushToDrawStack = true, string id = null) where E : Entity, new()
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Entity of type '{typeof(E).FullName}' into {parentList.GetType().FullName}");

                var entity = new E();

                return CreateEntity(entity, parentList, id, pushToUpdateLoop, pushToDrawStack) as E;
            }

            private Entity CreateEntity(Entity entity, EntityList parentList, string id = null, bool pushToUpdateLoop = true, bool pushToDrawStack = true)
            {
                entity.Parent = parentList.Parent;
                entity.ParentEntityList = parentList;
                entity.DrawStack = parentList.Scene?.DrawStack ?? parentList.Entity?.DrawStack;
                entity.UpdateLoop = parentList.Scene?.UpdateLoop ?? parentList.Entity?.UpdateLoop;
                entity.ContentLoader = parentList.Scene?.ContentLoader ?? parentList.Entity?.ContentLoader;
                entity.ID = id;

                InitializeObject(entity, parentList);

                if (pushToUpdateLoop && entity.UpdateLoop != null && entity is IUpdatable updatable)
                {
                    entity.UpdateLoop.Push(updatable);
                }

                if (pushToDrawStack && entity.DrawStack != null && entity is IDrawable drawable)
                {
                    entity.DrawStack.Push(drawable);
                }

                return entity;
            }

            private volatile bool ExitApplication = false;
            /// <summary>Requests that the game loop stop.</summary>
            public void Exit()
            {
                ExitApplication = true;
                if (!_processGameLock.Wait(0))
                    return;

                try
                {
                    _stopUpdateThread = true;
                    _updateRequested.Set();
                    DrawWorkers?.Dispose();
                }
                finally
                {
                    _processGameLock.Release();
                }
            }

            internal void Start()
            {
                StartTime = DateTime.UtcNow;

                foreach (var module in Modules)
                {
                    if (module.WillLoadExtensions)
                    {
                        module.DoLoadExtensions(TypeO);
                    }
                }

                if (Logger == null)
                {
                    TypeO.SetLogger();
                }

                //We need to set Context here before so that the logger works in InitializeObject
                (Logger as IHasContext)?.SetContext(this);
                InitializeObject(Logger);
                Logger.Log($"Game started at: {StartTime}");
                Logger.Log($"Logger of type '{Logger.GetType().FullName}' loaded");

                //Initialize Hardware
                foreach (var hardware in Hardwares.Values)
                {
                    InitializeObject(hardware);

                    Logger.Log($"Hardware of type '{hardware.GetType().FullName}' loaded");
                }

                //Create Services
                foreach (var serviceIdPair in Services)
                {
                    foreach (var servicePair in serviceIdPair.Value)
                    {
                        var service = servicePair.Value;
                        InitializeObject(service);

                        Logger.Log($"Service of type '{service.GetType().FullName}' loaded");
                    }
                }

                //Set modules Hardware and initialize
                foreach (var module in Modules)
                {
                    InitializeObject(module);

                    Logger.Log($"Module of type '{module.GetType().FullName}' loaded");
                }

                //Setup content binding
                foreach (var binding in ContentBinding)
                {
                    var bindingTo = binding.Value;
                    var bindingFrom = binding.Key;

                    if (!bindingTo.IsSubclassOf(bindingFrom))
                    {
                        var message = $"Content Binding from '{bindingFrom.Name}' must be of a base type to '{bindingTo.Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new ArgumentException(message);
                    }
                }

                //Initialize the game
                DrawWorkers = new DrawThreadPool(Name, TypeO.DrawThreadCount);
                try
                {
                    InitializeObject(Game);
                }
                catch
                {
                    DrawWorkers.Dispose();
                    DrawWorkers = null;
                    throw;
                }

                Logger.Log($"Game of type '{Game.GetType().FullName}' loaded");

                Logger.Log($"Everything loaded successfully, spinning up game loop");
                if (ExitApplication)
                {
                    Cleanup();
                    return;
                }

                try
                {
                    StartUpdateThread();
                }
                catch
                {
                    DrawWorkers.Dispose();
                    DrawWorkers = null;
                    throw;
                }
                if (Game.RunSynchronously)
                {
                    try
                    {
                        while (!ExitApplication)
                        {
                            ProcessGame();
                        }
                    }
                    finally
                    {
                        try
                        {
                            StopUpdateThread();
                        }
                        finally
                        {
                            DrawWorkers?.Dispose();
                            DrawWorkers = null;
                        }
                    }
                    Cleanup();
                    return;
                }

            }

            /// <summary>
            /// Processes one frame, coordinating the update thread with drawing on the calling thread.
            /// </summary>
            /// <remarks>Each frame waits for the single update thread to finish, draws on the calling thread, then
            /// releases the update thread to begin the next frame.</remarks>
            public void ProcessGame()
            {
                if (_updateThread == null)
                    throw new InvalidOperationException("The game runtime must be started before processing frames.");

                _processGameLock.Wait();
                try
                {
                    if (ExitApplication)
                    {
                        _stopUpdateThread = true;
                        _updateRequested.Set();
                        DrawWorkers?.Dispose();
                        return;
                    }

                    if (!_updateThread.IsAlive)
                    {
                        if (ExitApplication)
                            return;
                        throw new InvalidOperationException("The update thread has stopped unexpectedly.");
                    }

                    var now = DateTime.UtcNow;
                    TimeSinceStart = now - StartTime;
                    _pendingDelta = (now - LastTick).TotalSeconds;
                    LastTick = now;
                    _updateException = null;
                    _mainThreadUpdateAction = null;
                    _mainThreadUpdateException = null;

                    _updateRequested.Set();
                    WaitForUpdate();

                    var updateException = _updateException;
                    try
                    {
                        if (updateException == null && _shouldDrawFrame)
                            Game.Draw();
                    }
                    finally
                    {
                        _drawCompleted.Set();
                    }

                    updateException?.Throw();
                }
                finally
                {
                    _processGameLock.Release();
                }
            }

            private void StartUpdateThread()
            {
                if (_updateThread != null)
                    throw new InvalidOperationException("The update thread has already been started.");

                _stopUpdateThread = false;
                _updateThread = new Thread(UpdateThreadLoop)
                {
                    IsBackground = true,
                    Name = $"{Name} Update"
                };
                _updateThread.Start();
            }

            private void StopUpdateThread()
            {
                var updateThread = _updateThread;
                if (updateThread == null)
                    return;
                if (Thread.CurrentThread == updateThread)
                    throw new InvalidOperationException("The update thread cannot stop itself.");

                _processGameLock.Wait();
                try
                {
                    _stopUpdateThread = true;
                    _drawCompleted.Set();
                    _updateRequested.Set();
                    updateThread.Join();
                    _updateThread = null;
                }
                finally
                {
                    _processGameLock.Release();
                }
            }

            private void UpdateThreadLoop()
            {
                using (CanvasDrawCommandQueue.EnterWorkerThread())
                {
                    while (true)
                    {
                        _updateRequested.WaitOne();
                        if (_stopUpdateThread)
                            return;

                        try
                        {
                            _shouldDrawFrame = !ExitApplication && UpdateFrame(_pendingDelta);
                        }
                        catch (Exception exception)
                        {
                            _updateException = ExceptionDispatchInfo.Capture(exception);
                            _shouldDrawFrame = false;
                        }
                        finally
                        {
                            _updateCompleted.Set();
                        }

                        _drawCompleted.WaitOne();
                        if (ExitApplication || _stopUpdateThread)
                        {
                            if (ExitApplication)
                                DrawWorkers?.Dispose();
                            return;
                        }
                    }
                }
            }

            private void WaitForUpdate()
            {
                var waitHandles = new WaitHandle[] { _updateCompleted, _mainThreadUpdateRequested };
                while (WaitHandle.WaitAny(waitHandles) != 0)
                {
                    try
                    {
                        var action = _mainThreadUpdateAction
                            ?? throw new InvalidOperationException("A main-thread update was requested without an action.");
                        action();
                    }
                    catch (Exception exception)
                    {
                        _mainThreadUpdateException = ExceptionDispatchInfo.Capture(exception);
                    }
                    finally
                    {
                        _mainThreadUpdateAction = null;
                        _mainThreadUpdateCompleted.Set();
                    }
                }
            }

            private bool UpdateFrame(double dt)
            {
                foreach (var module in Modules)
                    UpdateComponent(module as IUpdatable, dt);

                foreach (var hardware in Hardwares.Values)
                    UpdateComponent(hardware as IUpdatable, dt);

                foreach (var services in Services.Values)
                {
                    foreach (var service in services.Values)
                        UpdateComponent(service as IUpdatable, dt);
                }

                if (ExitApplication)
                    return false;

                Game.Update(dt);
                return true;
            }

            private void UpdateComponent(IUpdatable updatable, double dt)
            {
                if (updatable?.Pause != false)
                    return;

                if (updatable is IMainThreadUpdatable)
                {
                    _mainThreadUpdateAction = () => updatable.Update(dt);
                    _mainThreadUpdateRequested.Set();
                    _mainThreadUpdateCompleted.WaitOne();

                    var exception = _mainThreadUpdateException;
                    _mainThreadUpdateException = null;
                    exception?.Throw();
                    return;
                }

                updatable.Update(dt);
            }

            /// <summary>
            /// Performs cleanup operations for the game and its associated resources.
            /// </summary>
            /// <remarks>This method ensures that all game-related resources, including services,
            /// hardware components, and modules, are properly cleaned up. It also logs the cleanup process and
            /// finalizes the logger. Call this method before exiting the game to release resources and avoid potential
            /// memory leaks.</remarks>
            public void Cleanup()
            {
                StopUpdateThread();
                DrawWorkers?.Dispose();
                DrawWorkers = null;
                Logger.Log("Exiting game, initiating cleanup");

                //Cleanup
                Game.DoCleanup();

                foreach (var serviceIdPair in Services)
                {
                    foreach (var servicePair in serviceIdPair.Value)
                    {
                        servicePair.Value.DoCleanup();
                    }
                }

                foreach (var hardware in Hardwares)
                {
                    hardware.Value.DoCleanup();
                }

                foreach (var module in Modules)
                {
                    module.DoCleanup();
                }

                Logger.Log("Bye bye\n\r\n\r");
                Logger.DoCleanup();
            }

            /// <summary>Injects context dependencies, registers, and initializes a TypeO object.</summary>
            public void InitializeObject(TypeOObject obj, TypeOObject from = null)
            {
                Logger.Log(LogLevel.Debug, $"Initializing obj '{obj.GetType().FullName}'" + (from != null ? $" from '{from.GetType().FullName}'" : ""));

                (obj as IHasContext)?.SetContext(this);
                SetHardwares(obj);
                SetServices(obj);
                SetLogger(obj);

                if (string.IsNullOrEmpty(obj.ID))
                {
                    obj.ID = Guid.NewGuid().ToString();
                }
                IDToTypeOObjectMap.Add((obj is IHasParent parent ? parent.FullParentPath : string.Empty) + obj.ID, obj);

                if (obj is IHasGame)
                {
                    Logger.Log(LogLevel.Ludacris, $"Injecting Game of type '{Game.GetType().FullName}' into {obj.GetType().FullName}");
                    (obj as IHasGame).Game = Game;
                }

                if ((obj is IHasData))
                {
                    var hasData = (obj as IHasData);

                    if (obj is Logic && from is IHasData)
                    {
                        (obj as IHasData).EntityData = (from as IHasData).EntityData;
                        Logger.Log(LogLevel.Ludacris, $"Injecting EntityData of type '{(obj as IHasData).EntityData.GetType().FullName}' from '{from.GetType().FullName}' into {obj.GetType().FullName}");
                    }
                    else
                    {
                        hasData.CreateData();
                        if (hasData.EntityData != null)
                        {
                            Logger.Log(LogLevel.Ludacris, $"Creating EntityData of type '{(obj as IHasData).EntityData.GetType().FullName}' into {obj.GetType().FullName}");
                            hasData.EntityData.Initialize();
                        }
                        else
                        {
                            Logger.Log(LogLevel.Ludacris, $"EntityData creation skipped on {obj.GetType().FullName}");
                        }
                    }

                    if ((obj as IHasData).EntityData == null)
                    {
                        Logger.Log(LogLevel.Warning, $"EntityData is null in {obj.GetType().FullName}");
                    }
                }

                if (obj is IHasScene)
                {
                    if (from is Scene)
                    {
                        (obj as IHasScene).Scene = from as Scene;
                    }
                    else
                    {
                        (obj as IHasScene).Scene = (from as IHasScene)?.Scene;
                    }
                    Logger.Log(LogLevel.Ludacris, $"Injecting Scene of type '{(obj as IHasScene).Scene?.GetType().FullName}' from '{from.GetType().FullName}' into {obj.GetType().FullName}");
                    if ((obj as IHasScene).Scene == null)
                    {
                        Logger.Log(LogLevel.Warning, $"Scene is null in {obj.GetType().FullName}");
                    }
                }

                if (obj is IHasEntity)
                {
                    (obj as IHasEntity).Entity = from as Entity;
                    Logger.Log(LogLevel.Ludacris, $"Injecting Entity of type '{(obj as IHasEntity).Entity?.GetType().FullName}' from '{from.GetType().FullName}' into {obj.GetType().FullName}");

                    if ((obj as IHasEntity).Entity == null)
                    {
                        Logger.Log(LogLevel.Warning, $"Entity is null in {obj.GetType().FullName}");
                    }
                }

                if (obj is IHasEntities)
                {
                    var hasEntities = obj as IHasEntities;

                    hasEntities.Entities = new EntityList();
                    hasEntities.Entities.Parent = obj;
                    Logger.Log(LogLevel.Ludacris, $"Creating EntityList in {obj.GetType().FullName}");
                    InitializeObject(hasEntities.Entities, obj);
                }

                if (obj is TypeOObject typeObject)
                {
                    Logger.Log(LogLevel.Debug, $"´Calling Initialize on '{obj.GetType().FullName}'" + (from != null ? $" from '{from.GetType().FullName}'" : ""));
                    typeObject.DoInitialize();
                }
            }

            /// <summary>Removes an initialized object from the context's identifier map.</summary>
            public void DestroyObject(TypeOObject obj)
            {
                IDToTypeOObjectMap.Remove((obj is IHasParent parent ? parent.FullParentPath : string.Empty) + obj.ID);
            }

            private void SetHardwares(object obj)
            {
                foreach (var property in GetInjectionProperties(obj.GetType()).Hardwares)
                {
                    if (!Hardwares.TryGetValue(property.PropertyType, out var hardware))
                    {
                        var message = $"Hardware type '{property.PropertyType.Name}' is not loaded for '{obj.GetType().Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new InvalidOperationException(message);
                    }

                    Logger.Log(LogLevel.Ludacris, $"Hardware '{hardware.GetType().FullName}' injected to property '{property.Name}' on object '{obj.GetType().FullName}'");
                    property.SetValue(obj, hardware);
                }
            }

            private void SetServices(object obj)
            {
                foreach (var injection in GetInjectionProperties(obj.GetType()).Services)
                {
                    var property = injection.Property;
                    if (!Services.TryGetValue(property.PropertyType, out var services))
                    {
                        var message = $"Service type '{property.PropertyType.Name}' is not loaded for '{obj.GetType().Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new InvalidOperationException(message);
                    }

                    if (!services.TryGetValue(injection.Id, out var service))
                    {
                        var message = $"Service type '{property.PropertyType.Name}' with ID '{injection.Id}' is not loaded for '{obj.GetType().Name}'";
                        Logger.Log(LogLevel.Fatal, message);
                        throw new InvalidOperationException(message);
                    }

                    Logger.Log(LogLevel.Ludacris, $"Service '{service.GetType().FullName}' injected to property '{property.Name}' on object '{obj.GetType().FullName}'");
                    property.SetValue(obj, service);
                }
            }

            private void SetLogger(TypeOObject obj)
            {
                var property = GetInjectionProperties(obj.GetType()).Logger;
                if (property == null)
                    return;

                Logger.Log(LogLevel.Ludacris, $"Logger injected to property '{property.Name}' on object '{obj.GetType().FullName}'");
                property.SetValue(obj, Logger);
            }

            private InjectionProperties GetInjectionProperties(Type type)
            {
                return _injectionProperties.GetOrAdd(type, static objectType =>
                {
                    var properties = objectType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    return new InjectionProperties
                    {
                        Hardwares = properties
                            .Where(property => property.PropertyType.GetInterface(nameof(IHardware)) != null)
                            .ToArray(),
                        Services = properties
                            .Where(property => property.PropertyType.IsSubclassOf(typeof(Service)))
                            .Select(property => (property, property.GetCustomAttribute<ServiceId>()?.Id ?? string.Empty))
                            .ToArray(),
                        Logger = properties.FirstOrDefault(property => property.PropertyType == typeof(ILogger))
                    };
                });
            }

            internal Drawable CreateDrawable(Type type, TypeOObject obj, DrawStack drawStack, DrawableOption<Drawable> option, string id = null)
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Drawable of type '{type.FullName}' into {obj.GetType().FullName}");

                var drawable = Activator.CreateInstance(type) as Drawable;
                drawable.Parent = obj;
                drawable.ContentLoader = Game.ContentLoader;
                drawable.ID = id;

                InitializeObject(drawable, obj);
                option?.Create(drawable);

                if (drawStack != null)
                {
                    drawStack.Push(drawable);
                }

                return drawable;
            }

            internal D CreateDrawable<D>(TypeOObject obj, DrawStack drawStack, DrawableOption<D> option, string id = null) where D : Drawable, new()
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Drawable of type '{typeof(D).FullName}' into {obj.GetType().FullName}");

                var drawable = new D()
                {
                    ID = id,
                    Parent = obj,
                    ContentLoader = Game.ContentLoader
                };

                InitializeObject(drawable, obj);
                option?.Create(drawable);

                if (drawStack != null)
                {
                    drawStack.Push(drawable);
                }

                return drawable;
            }

            internal void DestroyDrawable(Drawable drawable, DrawStack drawStack)
            {
                if (drawStack != null)
                {
                    drawStack.Pop(drawable);
                }
                drawable.DoCleanup();
                DestroyObject(drawable);
            }

            internal L CreateLogic<L>(TypeOObject obj, UpdateLoop updateLoop, LogicOption<L> option) where L : Logic, new()
            {
                Logger.Log(LogLevel.Ludacris, $"Creating Logic of type '{typeof(L).FullName}' into {obj.GetType().FullName}");

                var logic = new L()
                {
                    Parent = obj
                };

                InitializeObject(logic, obj);
                option?.Create(logic);

                if (updateLoop != null)
                {
                    updateLoop.Push(logic);
                }

                return logic;
            }

            internal void DestroyLogic(Logic logic, UpdateLoop updateLoop)
            {
                if (updateLoop != null)
                {
                    updateLoop.Pop(logic);
                    logic.DoCleanup();
                }
                DestroyObject(logic);
            }

            /// <summary>Registers a new service instance under the specified identifier.</summary>
            public void AddService<S>(string id = "") where S : Service, new()
            {
                //Instantiate the Service
                var service = new S();
                var ServiceType = typeof(S);

                if (!Services.ContainsKey(ServiceType))
                {
                    Services.Add(ServiceType, new Dictionary<string, Service>());
                }

                if (Services[ServiceType].ContainsKey(id))
                {
                    var message = $"Service of type '{ServiceType.Name}' already have key Id '{id}'";
                    Logger.Log(LogLevel.Fatal, message);
                    throw new ArgumentException(message);
                }

                Services[ServiceType].Add(id, service);
            }

            /// <summary>Gets a registered service by type and identifier, or <see langword="null"/> if none is registered.</summary>
            public S GetService<S>(string id = "") where S : Service, new()
            {
                var ServiceType = typeof(S);
                if (!Services.ContainsKey(ServiceType))
                {
                    Logger.Log(LogLevel.Warning, $"Service of type '{ServiceType.Name}' does not exist");
                    return null;
                }
                if (!Services[ServiceType].ContainsKey(id))
                {
                    Logger.Log(LogLevel.Warning, $"Service of type '{ServiceType.Name}' does not exist with id '{id}'");
                    return null;
                }

                return Services[ServiceType][id] as S;
            }

            /// <summary>Gets a registered object by identifier if it is of the requested type.</summary>
            public O GetTypeOObjectByID<O>(string id) where O : TypeOObject
            {
                if (!IDToTypeOObjectMap.ContainsKey(id))
                {
                    Logger.Log(LogLevel.Warning, $"TypeOObject with id '{id}' does not exist");
                    return null;
                }

                var typeOObject = IDToTypeOObjectMap[id] as O;
                if (typeOObject == null)
                    Logger.Log(LogLevel.Warning, $"TypeOObject with id '{id}' is not of type '{typeof(O).FullName}'");
                return typeOObject;
            }

            /// <summary>Returns the objects currently registered with this context.</summary>
            public List<TypeOObject> ListAllTypeOObjects()
            {
                return IDToTypeOObjectMap.Values.ToList();
            }
        }
    }
}
