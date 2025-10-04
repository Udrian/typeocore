using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.Game;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    /// <summary>
    /// Represents a template for game components, providing initialization and filtering logic specific to game-related
    /// entities.
    /// </summary>
    /// <remarks>This class is designed to be used as a base for creating game component templates. It
    /// provides functionality to initialize the component and apply filtering rules to exclude certain types of child
    /// components, such as <see cref="Drawable"/> and <see cref="Entity"/>.</remarks>
    public class GameComponentTemplate : ComponentTemplate<GameCode>
    {
        // Functions
        /// <summary>
        /// Filters out specific types from the children collection during processing.
        /// </summary>
        /// <remarks>This method excludes types derived from <see cref="Drawable"/> and <see cref="Entity"/>
        /// from the children collection by appending their fully qualified names to the filter's
        /// exclusion list.</remarks>
        /// <param name="filter">The <see cref="FilterHelper"/> instance used to apply exclusion rules.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Exclude += $"{typeof(Drawable).FullName};";
            filter.Exclude += $"{typeof(Entity).FullName};";
        }
    }
}
