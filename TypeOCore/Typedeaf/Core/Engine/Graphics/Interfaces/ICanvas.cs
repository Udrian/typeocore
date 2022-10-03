using TypeOEngine.Typedeaf.Core.Common;

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
            /// Canvas world matrix.
            /// </summary>
            public Matrix WorldMatrix { get; set; }

            /// <summary>
            /// Clears the Canvas.
            /// </summary>
            /// <param name="clearColor">Color to clear with.</param>
            public abstract void Clear(Color clearColor);

            /// <summary>
            /// Swaps the buffer and present everything that have been drawn to the Canvas.
            /// </summary>
            public abstract void Present();
        }
    }
}
