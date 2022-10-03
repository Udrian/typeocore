using System;
using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.Core.Engine.Graphics
{

    /// <inheritdoc/>
    public class Canvas : ICanvas
    {
        /// <inheritdoc/>
        public IWindow Window { get; set; }
        /// <inheritdoc/>
        public Rectangle Viewport { get; set; }
        /// <inheritdoc/>
        public Matrix WorldMatrix { get; set; }

        /// <summary>
        /// Canvas constructor.
        /// </summary>
        /// <param name="window">Window that the Canvas is attached to</param>
        /// <param name="viewport">Canvas viewport</param>
        /// <param name="worldMatrix">Canvas World Matrix</param>
        public Canvas(IWindow window, Rectangle viewport, Matrix worldMatrix)
        {
            Window = window;
            Viewport = viewport;
            WorldMatrix = worldMatrix;
        }

        /// <inheritdoc/>
        public virtual void Clear(Color clearColor)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual void Present()
        {
            throw new NotImplementedException();
        }
    }
}
