using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;

namespace TypeDCore.Components
{
    public static partial class CoreComponent
    {
        /// <summary>
        /// Creates and returns a new <see cref="Component"/> configured for the <see cref="Window"/> type. A window
        /// cannot be inherited from and can only be added to a game.
        /// </summary>
        /// <returns>A <see cref="Component"/> instance representing the <see cref="Window"/> type.</returns>
        public static Component WindowComponent()
        {
            return new Component()
            {
                ClassName = typeof(Window).Name,
                Namespace = typeof(Window).Namespace,
                Template = new WindowComponentTemplate(),
                Icon = "game.png"
            };
        }
    }
}
