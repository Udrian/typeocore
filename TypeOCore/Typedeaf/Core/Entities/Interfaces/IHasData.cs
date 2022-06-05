using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {
        public interface IHasData
        {
            public IEntityData EntityData { get; set; }
            public void CreateData();
        }

        public interface IHasData<D> : IHasData where D : IEntityData
        {
            IEntityData IHasData.EntityData { get => EntityData; set => EntityData = (D)value; }
            public new D EntityData { get; set; }

            void IHasData.CreateData()
            {
                if(!typeof(D).IsAbstract)
                    EntityData = (D)Activator.CreateInstance(typeof(D));
            }
        }
    }
}
