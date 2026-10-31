using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {
        /// <summary>Provides a collection of child entities.</summary>
        public interface IHasEntities
        {
            /// <summary>Gets or sets the child entity collection.</summary>
            public EntityList Entities { get; set; }
        }
    }
}
