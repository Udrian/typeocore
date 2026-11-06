using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {

        /// <summary>
        /// Default IHasEntities interface for Drawables. This interface provides a collection of child entities and a drawable manager for the child drawables.
        /// </summary>
        public interface IHasEntities : IHasEntities<Drawable> { }

        /// <summary>Provides a collection of child entities.</summary>
        public interface IHasEntities<D> where D : Drawable
        {
            /// <summary>Gets the child entity collection.</summary>
            public EntityList Entities { get; internal set; }
            /// <summary>
            /// Gets the drawable manager for the child drawables.
            /// </summary>
            public DrawableManager<D> Drawables { get; internal set; }
        }
    }
}
