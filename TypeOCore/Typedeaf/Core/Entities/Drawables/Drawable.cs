using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

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
        public abstract class Drawable : TypeOObject, IDrawable, IHasParent<Entity>
        {
            public Entity Parent { get; internal set; }

            /// <summary>
            /// Gets the <see cref="ContentLoader"/> instance used to load content for the application.
            /// </summary>
            public ContentLoader ContentLoader { get; internal set; }

            public bool Hidden { get; set; }

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
