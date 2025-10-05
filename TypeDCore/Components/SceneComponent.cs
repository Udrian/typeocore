using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core;

namespace TypeDCore.Components
{
    public static partial class CoreComponent
    {
        /// <summary>
        /// Creates and returns a new <see cref="Component"/> instance configured for the <see cref="Scene"/> type.
        /// </summary>
        /// <remarks>The returned <see cref="Component"/> is pre-configured with the class name,
        /// namespace, and base type information of the <see cref="Scene"/> class, as well as a default template of type
        /// <see cref="SceneComponentTemplate"/>.</remarks>
        /// <returns>A <see cref="Component"/> instance representing the <see cref="Scene"/> type.</returns>
        public static Component SceneComponent()
        {
            return new Component()
            {
                ClassName = typeof(Scene).Name,
                Namespace = typeof(Scene).Namespace,
                Template = new SceneComponentTemplate()
            };
        }
    }
}
