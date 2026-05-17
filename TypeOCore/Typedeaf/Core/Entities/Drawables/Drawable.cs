using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Attributes;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Drawables
    {
        /// <summary>
        /// Represents an abstract base class for objects that can be drawn on a canvas.
        /// </summary>
        /// <remarks>The <see cref="Drawable"/> class provides a foundation for drawable entities, 
        /// including properties to control visibility and draw order. Derived classes must implement the
        /// <see cref="Draw(ICanvas)"/> method to define the specific drawing behavior.</remarks>
        public abstract class Drawable : TypeOObject, IDrawable
        {
            public Entity Entity { get; internal set; } //TODO: Change to a anchor

            /// <summary>
            /// Gets or sets a value indicating whether the item is hidden.
            /// </summary>
            [TypeOProperty("Gets or sets a value indicating whether the item is hidden.", false)]
            public bool Hidden { get; set; }

            /// <summary>
            /// Gets or sets the draw order of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the draw order of the object.", 0)]
            public int DrawOrder { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Drawable"/> class.
            /// </summary>
            /// <remarks>This constructor is protected and intended to be used by derived classes to
            /// initialize the base state of a drawable object.</remarks>
            protected Drawable() { }

            /// <summary>
            /// Draws the object onto the specified canvas.
            /// </summary>
            /// <param name="canvas">The canvas on which the object will be drawn. Cannot be null.</param>-
            public abstract void Draw(ICanvas canvas);
        }
    }
}
