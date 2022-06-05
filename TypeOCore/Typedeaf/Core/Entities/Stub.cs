using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities
    {
        public abstract class Stub
        {
            public abstract void Initialize();

            public abstract Entity CreateEntity(EntityList entityList);
        }

        public abstract class Stub<E> : Stub where E : Entity, new()
        {
            public Stub() { }

            public override Entity CreateEntity(EntityList entityList)
            {
                var entity = entityList.Create<E>();

                InitializeEntity(entity);

                return entity;
            }

            protected abstract void InitializeEntity(E entity);
        }
    }
}