using TypeOEngine.Typedeaf.Core.Common;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces;

namespace TypeOEngine.Typedeaf.Core.Engine.Graphics
{
    /// <inheritdoc/>
    public abstract class Canvas : TypeOObject, ICanvas
    {
        /// <inheritdoc/>
        public IWindow Window { get; set; }

        /// <inheritdoc/>
        public Rectangle Viewport { get; set; }

        /// <inheritdoc/>
        public Vec3 WorldTranslation { get; set; }

        /// <summary>
        /// Canvas constructor.
        /// </summary>
        /// <param name="window">Window that the Canvas is attached to</param>
        /// <param name="viewport">Canvas viewport</param>
        public Canvas(IWindow window, Rectangle viewport)
        {
            Window = window;
            Viewport = viewport;
            WorldTranslation = Vec3.Zero;
        }

        /// <inheritdoc/>
        public abstract void Clear(Color clearColor);

        /// <inheritdoc/>
        public abstract void PreDraw();

        /// <inheritdoc/>
        public abstract void Present();

        /// <inheritdoc/>
        public abstract void PostDraw();

        /// <inheritdoc/>
        public Texture Screenshot() { return Screenshot(Viewport); }

        /// <inheritdoc/>
        public abstract Texture Screenshot(Rectangle screenRect);
    }
}
