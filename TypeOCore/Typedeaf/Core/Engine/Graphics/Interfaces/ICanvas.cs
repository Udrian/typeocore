using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Graphics.Interfaces
    {
        /// <summary>
        /// Base Canvas interface
        /// </summary>.
        public interface ICanvas
        {
            /// <summary>
            /// Window attached to the Canvas.
            /// </summary>
            public IWindow Window { get; set; }
            
            /// <summary>
            /// Rectangular area of Window attach to Canvas.
            /// </summary>
            public abstract Rectangle Viewport { get; set; }

            /// <summary>
            /// Gets or sets the world-space translation of the Canvas.
            /// </summary>
            public Vec3 WorldTranslation { get; set; }

            /// <summary>
            /// Clears the Canvas.
            /// </summary>
            /// <param name="clearColor">Color to clear with.</param>
            public abstract void Clear(Color clearColor);

            /// <summary>
            /// Called before Draw is called
            /// </summary>
            public abstract void PreDraw();

            /// <summary>
            /// Called after Draw is called
            /// </summary>
            public abstract void PostDraw();

            /// <summary>
            /// Swaps the buffer and present everything that have been drawn to the Canvas.
            /// </summary>
            public abstract void Present();

            /// <summary>
            /// Creates a screenshot of the entire canvas
            /// </summary>
            /// <returns>The texture containing the screenshot</returns>
            public abstract Texture Screenshot();

            /// <summary>
            /// Creates a screenshot of a part of the canvas
            /// </summary>
            /// <param name="screenRect">The rectangle bounds of the screenshot</param>
            /// <returns>The texture containing the screenshot</returns>
            public abstract Texture Screenshot(Rectangle screenRect);
        }
    }
}
