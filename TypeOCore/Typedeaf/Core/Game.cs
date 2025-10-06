using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    /// <summary>
    /// Represents the base class for a game, providing core functionality for managing game logic, rendering, content loading, and scenes.
    /// </summary>
    /// <remarks>The <see cref="Game"/> class serves as the foundation for creating games. It provides 
    /// essential components such as drawable and logic managers, a content loader, and a scene list. Derived classes
    /// must implement the <see cref="Update(double)"/> and <see cref="Draw"/> methods to define the game's update and
    /// rendering behavior. The game operates within a <see cref="Context"/>, which manages the overall state and
    /// lifecycle of the game. The <see cref="Exit"/> method can be used to terminate the game gracefully.</remarks>
    public abstract class Game : TypeOObject, IHasContext
    {
        Context IHasContext.Context { get; set; }
        private Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

        /// <summary>
        /// Gets the name associated with the current context.
        /// </summary>
        public string Name { get { return Context.Name; } }

        /// <summary>
        /// Gets the manager responsible for handling a collection of drawable objects.
        /// </summary>
        public DrawableManager<Drawable> Drawables { get; private set; }

        /// <summary>
        /// Gets the logic manager responsible for handling the application's core logic operations.
        /// </summary>
        public LogicManager Logics { get; private set; }

        /// <summary>
        /// Gets the <see cref="ContentLoader"/> instance used to load and manage content resources.
        /// </summary>
        public ContentLoader ContentLoader { get; private set; }

        /// <summary>
        /// Gets the collection of scenes available in the application.
        /// </summary>
        public SceneList Scenes { get; private set; }

        /// <summary>
        /// Gets or sets the main application window.
        /// </summary>
        public IWindow MainWindow { get; set; }

        /// <summary>
        /// Gets the primary canvas used for rendering or drawing operations.
        /// </summary>
        public ICanvas MainCanvas { get { return MainWindow?.Canvas; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Game"/> class.
        /// </summary>
        /// <remarks>This constructor is protected and intended to be used by derived classes to
        /// initialize the base state of a game.</remarks>
        protected Game() { }

        /// <summary>
        /// Initializes the core components of the application, including managers, loaders, and scenes.
        /// </summary>
        /// <remarks>This method sets up the necessary components for the application to function, such as
        /// drawable and logic managers, the content loader, and the scene list. It ensures that these components are
        /// properly initialized and registered with the application context.</remarks>
        protected override void Initialize()
        {
            Drawables = new DrawableManager<Drawable>(null, this);
            Context.InitializeObject(Drawables, this);
            Logics = new LogicManager(null, this);
            Context.InitializeObject(Logics, this);
            ContentLoader = new ContentLoader(Context.ContentBinding);
            Context.InitializeObject(ContentLoader);
            Scenes = new SceneList(this);
            Context.InitializeObject(Scenes);
        }

        /// <summary>
        /// Updates the state of the object based on the elapsed time.
        /// </summary>
        /// <remarks>This method is intended to be called periodically to update the object's state.
        /// The specific behavior of the update depends on the implementation in a derived class.</remarks>
        /// <param name="dt">The time, in seconds, that has elapsed since the last update. Must be a non-negative value.</param>
        public abstract void Update(double dt);

        /// <summary>
        /// Performs the drawing operation for the object.
        /// </summary>
        /// <remarks>This method must be implemented by derived classes to define the specific drawing
        /// behavior.</remarks>
        public abstract void Draw();

        /// <summary>
        /// Terminates the current operation and exits the context.
        /// </summary>
        /// <remarks>This method delegates the exit operation to the underlying context. Ensure that any
        /// necessary cleanup is performed before calling this method.</remarks>
        public void Exit() { Context.Exit(); }
    }
}