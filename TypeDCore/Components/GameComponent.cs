using TypeD.Components;
using TypeD.Helpers;
using TypeDCore.Code.Game;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Components
{
    public class GameComponent : ComponentTemplate<GameCode>
    {
        // Constructors
        public override void Init()
        {
        }

        // Functions
        public override void ChildrenFilter(FilterHelper filter)
        {
            filter.Exclude += $"{typeof(Drawable).FullName};";
            filter.Exclude += $"{typeof(Entity).FullName};";
        }
    }
}
