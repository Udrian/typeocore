using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeDCore.Components
{
    public static partial class CoreComponent
    {
        /// <summary>
        /// Creates and returns a new <see cref="Component"/> instance configured for the <see cref="Entity"/> type.
        /// </summary>
        /// <remarks>The returned <see cref="Component"/> includes metadata such as the class name,
        /// namespace, and base type of the <see cref="Entity"/> type. It also initializes the <see cref="Component.Template"/>
        /// property with an instance of <see cref="EntityComponentTemplate"/>.</remarks>
        /// <returns>A <see cref="Component"/> instance representing the <see cref="Entity"/> type.</returns>
        public static Component EntityComponent()
        {
            return new Component()
            {
                ClassName = typeof(Entity).Name,
                Namespace = typeof(Entity).Namespace,
                Template = new EntityComponentTemplate()
            };
        }
    }
}
