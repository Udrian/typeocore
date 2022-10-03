using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Graphics.Interfaces
    {
        /// <summary>
        /// Base Window interface.
        /// </summary>
        public interface IWindow
        {
            /// <summary>
            /// Window title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Window size in pixels.
            /// </summary>
            public Vec2i Size { get; set; }

            /// <summary>
            /// Default canvas that will be used to paint on.
            /// </summary>
            public ICanvas Canvas { get; set; }
        }
    }
}