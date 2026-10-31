using TypeOEngine.Typedeaf.Core.Entities.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        /// <summary>Base class for data associated with an entity.</summary>
        public abstract class EntityData : IEntityData
        {
            /// <summary>Initializes a new entity data instance.</summary>
            public EntityData() { }

            /// <summary>Initializes the data after it has been created for an entity.</summary>
            public abstract void Initialize();
        }
    }
}