using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.DrawStack;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    /// <summary>
    /// Template of the draw stack component. A draw stack can only live under a scene and only holds entities and drawables.
    /// </summary>
    public class DrawStackComponentTemplate : ComponentTemplate<DrawStackCode>
    {
        /// <summary>
        /// A draw stack accepts entities and drawables.
        /// </summary>
        /// <param name="filter">The filter to apply the rules to.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
            filter.Filters += $"{typeof(Entity).FullName};";
        }
    }
}
