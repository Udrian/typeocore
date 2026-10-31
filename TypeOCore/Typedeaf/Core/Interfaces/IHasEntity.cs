using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        /// <summary>Provides a reference to an entity.</summary>
        public interface IHasEntity
        {
            /// <summary>Gets or sets the associated entity.</summary>
            public Entity Entity { get; set; }
        }

        /// <summary>Provides an entity reference of a specific type.</summary>
        /// <typeparam name="E">The entity type.</typeparam>
        public interface IHasEntity<E> : IHasEntity where E : Entity
        {
            Entity IHasEntity.Entity { get => Entity; set => Entity = value as E; }
            /// <summary>Gets or sets the strongly typed entity.</summary>
            public new E Entity { get; set; }
        }
    }
}
