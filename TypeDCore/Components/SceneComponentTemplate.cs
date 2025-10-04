using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.Scene;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeDCore.Components
{
    /// <summary>
    /// Represents a template for scene components, providing initialization and filtering logic specific to
    /// scene-related entities.
    /// </summary>
    /// <remarks>This class extends <see cref="ComponentTemplate{T}"/> with functionality tailored to
    /// components of type <see cref="SceneCode"/>. It includes methods for initializing the template and applying
    /// filters to child entities.</remarks>
    public class SceneComponentTemplate : ComponentTemplate<SceneCode>
    {
        // Functions
        /// <summary>
        /// Applies a filter to include child entities of the specified type.
        /// </summary>
        /// <remarks>This method appends a filter string for the current entity type to the <see cref="FilterHelper.Filters"/> property.</remarks>
        /// <param name="filter">The <see cref="FilterHelper"/> instance to which the filter is applied. This parameter cannot be <see langword="null"/>.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Entity).FullName};";
        }
    }
}
