using TypeD.Components;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeDCore.Code.Window;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Engine.Graphics;

namespace TypeDCore.Components
{
    /// <summary>
    /// Template of the window component. A window is a leaf node that can only live under a game.
    /// </summary>
    public class WindowComponentTemplate : ComponentTemplate<WindowCode>
    {
        /// <summary>
        /// A window does not accept any children.
        /// </summary>
        /// <param name="filter">The filter to apply the rules to.</param>
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Exclude += $"{typeof(Drawable).FullName};";
            filter.Exclude += $"{typeof(Entity).FullName};";
            filter.Exclude += $"{typeof(Window).FullName};";
        }

        /// <summary>
        /// A window does not accept any children.
        /// </summary>
        /// <param name="child">The component that would be added.</param>
        /// <returns>Always <see langword="false"/>.</returns>
        public override bool CanAddChild(Component child)
        {
            return false;
        }
    }
}
