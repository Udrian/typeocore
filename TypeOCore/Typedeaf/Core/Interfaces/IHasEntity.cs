using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        public interface IHasEntity
        {
            public Entity Entity { get; set; }
        }

        public interface IHasEntity<E> : IHasEntity where E : Entity
        {
            Entity IHasEntity.Entity { get => Entity; set => Entity = value as E; }
            public new E Entity { get; set; }
        }
    }
}
