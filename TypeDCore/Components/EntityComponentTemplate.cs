using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.Entity;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    /// <summary>
    /// Represents a template for creating and managing entity components of type <see cref="EntityCode"/>.
    /// </summary>
    /// <remarks>This class provides functionality to initialize the template and apply filtering logic for
    /// child components. It is designed to be used as a base for defining specific entity component
    /// behaviors.</remarks>
    public class EntityComponentTemplate : ComponentTemplate<EntityCode>
    {
        // Functions
        /// <summary>
        /// Applies a filter to include only child elements of type <see cref="Drawable"/>.
        /// </summary>
        /// <remarks>This method appends a filter string for the <see cref="Drawable"/> type to the <see cref="FilterHelper.Filters"/>
        /// property, ensuring that only child elements of this type are included in the filtering process.</remarks>
        /// <param name="filter">The <see cref="FilterHelper"/> instance used to apply the filter. This parameter cannot be <see langword="null"/>.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
