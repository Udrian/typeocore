using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    /// <summary>
    /// Represents an abstract base class for a scene in a game or application, providing core functionality for
    /// managing entities, drawing, updating, and transitioning between scenes.
    /// </summary>
    /// <remarks>A scene serves as a container for game logic, drawable objects, and other resources required
    /// to represent a specific state or screen in the application. It provides mechanisms for updating game logic,
    /// rendering visuals, and handling transitions between scenes. <para> Derived classes must implement the
    /// <see cref="Update(double)"/> and <see cref="Draw"/> methods to define the scene's behavior and rendering logic.
    /// Additionally, the <see cref="OnEnter(Scene)"/> and <see cref="OnExit(Scene)"/> methods must be implemented to
    /// handle transitions between scenes. </para></remarks>
    public abstract class Scene : TypeOObject, IHasEntities, IHasContext
    {
        Context IHasContext.Context { get; set; }
        private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

        /// <summary>
        /// Gets the collection of scenes available in the application.
        /// </summary>
        public SceneList Scenes { get; internal set; }

        /// <summary>
        /// Gets the window associated with the current context.
        /// </summary>
        public IWindow Window { get; internal set; }

        /// <summary>
        /// Gets the canvas used for rendering graphical elements.
        /// </summary>
        public ICanvas Canvas { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ContentLoader"/> instance used to load content for the application.
        /// </summary>
        public ContentLoader ContentLoader { get; internal set; }

        /// <summary>
        /// Gets or sets the collection of entities managed by this instance.
        /// </summary>
        public EntityList Entities { get; set; } //TODO: Look over this

        /// <summary>
        /// Gets the draw stack used to manage and organize drawable elements in the game.
        /// </summary>
        public DrawStack DrawStack { get; private set; } //TODO: Should be able to create draw stack from Game maybe?

        /// <summary>
        /// Gets the update loop associated with the current instance.
        /// </summary>
        public UpdateLoop UpdateLoop { get; private set; } //TODO: Should be able to create Update loop from Game maybe?

        /// <summary>
        /// Gets the manager responsible for handling a collection of drawable objects.
        /// </summary>
        public DrawableManager<Drawable> Drawables { get; private set; }

        /// <summary>
        /// Gets the <see cref="LogicManager"/> instance used to manage application logic.
        /// </summary>
        public LogicManager Logics { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scene is paused.
        /// </summary>
        [TypeOProperty("Gets or sets a value indicating whether the scene is paused.")]
        public bool Pause         { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the scene should be hidden.
        /// </summary>
        [TypeOProperty("Gets or sets a value indicating whether the scene should be hidden.")]
        public bool Hide          { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        /// <remarks>This constructor initializes the <see cref="DrawStack"/> and <see cref="UpdateLoop"/>
        /// components, which are used to manage the drawing and update operations within the scene.</remarks>
        protected Scene()
        {
            DrawStack = new DrawStack();
            UpdateLoop = new UpdateLoop();
        }

        /// <summary>
        /// Initializes the drawable and logic managers for the current context.
        /// </summary>
        /// <remarks>This method sets up the <see cref="DrawableManager{T}"/> and <see cref="LogicManager"/> instances,
        /// associating them with the current context. It is typically called during
        /// the initialization phase of the application to prepare the drawing and update systems.</remarks>
        protected override void Initialize()
        {
            Drawables = new DrawableManager<Drawable>(DrawStack, this);
            Context.InitializeObject(Drawables, this);
            Logics = new LogicManager(UpdateLoop, this);
            Context.InitializeObject(Logics, this);
        }

        /// <summary>
        /// Updates the state of the object based on the elapsed time.
        /// </summary>
        /// <remarks>This method is intended to be overridden in derived classes to implement custom update logic.</remarks>
        /// <param name="dt">The time, in seconds, that has elapsed since the last update. Must be a non-negative value.</param>
        public abstract void Update(double dt);

        /// <summary>
        /// When overridden in a derived class, performs the drawing operation for the object.
        /// </summary>
        /// <remarks>This method must be implemented by derived classes to define the specific drawing
        /// behavior. The implementation may vary depending on the type of object being drawn.</remarks>
        public abstract void Draw();

        /// <summary>
        /// Executes logic when transitioning out of the current scene.
        /// </summary>
        /// <param name="to">The scene to which the transition is occurring. This parameter cannot be null.</param>
        public abstract void OnExit(Scene to);

        /// <summary>
        /// Executes logic when entering a new scene.
        /// </summary>
        /// <param name="from">The scene being exited. This parameter can be used to determine the transition context.
        /// May be <see langword="null"/> if there is no prior scene.</param>
        public abstract void OnEnter(Scene from);
    }
}
