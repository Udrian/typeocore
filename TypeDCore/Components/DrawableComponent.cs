using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    public static partial class CoreComponent
    {
        /// <summary>
        /// Creates and returns a new <see cref="Component"/> instance configured for the <see cref="Drawable"/> type.
        /// </summary>
        /// <remarks>The returned <see cref="Component"/> is preconfigured with the class name, namespace,
        /// and base type information of the <see cref="Drawable"/> type, as well as a default template of type 
        /// <see cref="DrawableComponentTemplate"/>.</remarks>
        /// <returns>A <see cref="Component"/> instance representing the <see cref="Drawable"/> type.</returns>
        public static Component DrawableComponent()
        {
            return new Component()
            {
                ClassName = typeof(Drawable).Name,
                Namespace = typeof(Drawable).Namespace,
                Template = new DrawableComponentTemplate()
            };
        }            
    }
}
