using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.Entity;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    public class EntityComponent : ComponentTemplate<EntityCode>
    {
        // Constructors
        public override void Init()
        {
        }

        // Functions
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Filters += $"{typeof(Drawable).FullName};";
        }
    }
}
