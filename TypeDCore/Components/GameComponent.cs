using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core;

namespace TypeDCore.Components
{
    public static partial class CoreComponent
    {
        /// <summary>
        /// Creates and returns a new <see cref="Component"/> instance configured for the <see cref="Game"/> class.
        /// </summary>
        /// <remarks>The returned <see cref="Component"/> includes metadata such as the class name,
        /// namespace, and base type of the <see cref="Game"/> class, as well as a default template of type
        /// <see cref="GameComponentTemplate"/>.</remarks>
        /// <returns>A <see cref="Component"/> instance representing the <see cref="Game"/> class.</returns>
        public static Component GameComponent()
        {
            return new Component()
            {
                ClassName = typeof(Game).Name,
                Namespace = typeof(Game).Namespace,
                Template = new GameComponentTemplate()
            };
        }
    }
}
