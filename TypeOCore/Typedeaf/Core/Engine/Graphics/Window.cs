using TypeOEngine.Typedeaf.Core.Attributes;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Graphics
    {
        /// <summary>
        /// Describes a window of a game. It is the editor's window component type; the game creates the actual
        /// <see cref="Interfaces.IWindow"/> through the window service from these settings. It cannot be inherited.
        /// </summary>
        public sealed class Window
        {
            /// <summary>Gets or sets the window title.</summary>
            [TypeOProperty("The title of the window.", Group = "Window")]
            public string Title { get; set; } = "Window";

            /// <summary>Gets or sets the position of the window on the screen in pixels.</summary>
            [TypeOProperty("The position of the window on the screen in pixels.", Group = "Window")]
            public Vec2i Position { get; set; } = new Vec2i(100, 100);

            /// <summary>Gets or sets the size of the window in pixels.</summary>
            [TypeOProperty("The size of the window in pixels.", Group = "Window")]
            public Vec2i Size { get; set; } = new Vec2i(1024, 768);
        }
    }
}
