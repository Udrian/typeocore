using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeDCore.Components
{
    public static partial class CoreComponent
    {
        /// <summary>
        /// Creates and returns a new <see cref="Component"/> configured for the <see cref="DrawStack"/> type. A draw
        /// stack cannot be inherited from and can only be added to a scene.
        /// </summary>
        /// <returns>A <see cref="Component"/> instance representing the <see cref="DrawStack"/> type.</returns>
        public static Component DrawStackComponent()
        {
            return new Component()
            {
                ClassName = typeof(DrawStack).Name,
                Namespace = typeof(DrawStack).Namespace,
                Template = new DrawStackComponentTemplate(),
                Icon = "drawable.png"
            };
        }
    }
}
