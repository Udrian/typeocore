using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        public abstract class EntityData : IEntityData
        {
            public EntityData() { }

            public abstract void Initialize();
        }
    }
}