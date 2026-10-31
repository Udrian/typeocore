using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        /// <summary>Defines a factory for creating entities in an entity list.</summary>
        public abstract class Stub : TypeOObject
        {
            /// <summary>Creates an entity in the specified list.</summary>
            public abstract Entity CreateEntity(EntityList entityList);
        }

        /// <summary>Creates and initializes entities of a specific type.</summary>
        /// <typeparam name="E">The entity type to create.</typeparam>
        public abstract class Stub<E> : Stub where E : Entity, new()
        {
            /// <summary>Initializes a new entity stub.</summary>
            public Stub() { }

            /// <summary>Creates an entity of the configured type in the specified list.</summary>
            public override Entity CreateEntity(EntityList entityList)
            {
                var entity = entityList.Create<E>();

                InitializeEntity(entity);

                return entity;
            }

            /// <summary>Initializes the newly created entity.</summary>
            protected abstract void InitializeEntity(E entity);
        }
    }
}