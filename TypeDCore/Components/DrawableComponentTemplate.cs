using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.Drawable;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    /// <summary>
    /// Represents a template for components that can be drawn, inheriting from <see cref="ComponentTemplate{T}"/>.
    /// </summary>
    /// <remarks>This class provides functionality for initializing drawable components and applying filters
    /// specific to drawable types. It is designed to work with components of type <see cref="DrawableCode"/>.</remarks>
    public class DrawableComponentTemplate : ComponentTemplate<DrawableCode>
    {
        // Functions
        /// <summary>
        /// Applies a filter to include only child elements of type <see cref="Drawable"/>.
        /// </summary>
        /// <param name="filter">The <see cref="FilterHelper"/> instance used to apply the filter. The method appends the fully qualified
        /// name of the <see cref="Drawable"/> type to the filter.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
