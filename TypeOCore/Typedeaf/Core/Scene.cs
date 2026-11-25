using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;
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
        public SceneManager Scenes { get; internal set; }

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
        public DrawableManager<Drawable> Drawables { get; set; }

        /// <summary>
        /// Gets the <see cref="LogicManager"/> instance used to manage application logic.
        /// </summary>
        public LogicManager Logics { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scene is paused.
        /// </summary>
        [TypeOProperty("Gets or sets a value indicating whether the scene is paused.", Group = "Behavior")]
        public bool Pause         { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the scene should be hidden.
        /// </summary>
        [TypeOProperty("Gets or sets a value indicating whether the scene should be hidden.", Group = "Rendering")]
        public bool Hide          { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the clear color for the scene.
        /// </summary>
        [TypeOProperty("Gets or sets the clear color for the scene.", Group = "Rendering")]
        public Color ClearColor { get; set; } = Color.Black;

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
            DrawStack.DrawableManager = Drawables;
            Logics = new LogicManager(UpdateLoop, this);
            Context.InitializeObject(Logics, this);
        }

        private readonly Dictionary<string, DrawStack> _drawStacks = new();
        private readonly List<DrawStack> _drawStackOrder = new();

        /// <summary>
        /// Creates an additional draw stack with its own <see cref="DrawableManager{T}"/>. Entities and drawables created
        /// through it are drawn by the new stack instead of the scene's default <see cref="DrawStack"/>.
        /// </summary>
        /// <param name="id">Identifier used to find the draw stack again.</param>
        /// <param name="name">Name of the draw stack.</param>
        /// <param name="drawOrder">Order the stack is drawn in, lower values are drawn first. The default stack has order 0.</param>
        /// <param name="ignoreCamera">Draw the stack in screen space, ignoring the camera.</param>
        public DrawStack CreateDrawStack(string id, string name, int drawOrder = 0, bool ignoreCamera = false)
        {
            if (id != null && _drawStacks.TryGetValue(id, out var existing))
                return existing;

            var stack = new DrawStack
            {
                ID = id,
                Name = name,
                Parent = this,
                DrawOrder = drawOrder,
                IgnoreCamera = ignoreCamera,
                DrawWorkers = DrawStack.DrawWorkers
            };
            Context.InitializeObject(stack, this);
            stack.DrawableManager = new DrawableManager<Drawable>(stack, stack);
            Context.InitializeObject(stack.DrawableManager, stack);
            _drawStacks.Add(stack.ID, stack);
            _drawStackOrder.Add(stack);
            return stack;
        }

        /// <summary>Gets a draw stack created with <see cref="CreateDrawStack"/>, or null when it does not exist.</summary>
        public DrawStack GetDrawStack(string id)
        {
            return id != null && _drawStacks.TryGetValue(id, out var stack) ? stack : null;
        }

        /// <summary>Removes a draw stack created with <see cref="CreateDrawStack"/>.</summary>
        public void RemoveDrawStack(string id)
        {
            if (id != null && _drawStacks.Remove(id, out var stack))
            {
                _drawStackOrder.Remove(stack);
                Context.DestroyObject(stack);
            }
        }

        /// <summary>Draws the default draw stack and every created draw stack, ordered by <see cref="DrawStack.DrawOrder"/>.</summary>
        /// <param name="canvas">The canvas to draw to.</param>
        public void DrawAllStacks(ICanvas canvas)
        {
            if (_drawStackOrder.Count == 0)
            {
                DrawStack.Draw(canvas);
                return;
            }

            // OrderBy is stable, so the default stack goes before created stacks with the same order
            var stacks = new List<DrawStack>(_drawStackOrder.Count + 1) { DrawStack };
            stacks.AddRange(_drawStackOrder);
            foreach (var stack in stacks.OrderBy(s => s.DrawOrder))
                stack.Draw(canvas);
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
