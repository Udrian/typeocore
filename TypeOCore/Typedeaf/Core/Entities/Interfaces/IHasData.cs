using System;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {
        /// <summary>Provides entity data and a method for creating it.</summary>
        public interface IHasData
        {
            /// <summary>Gets or sets the data associated with the implementing object.</summary>
            public IEntityData EntityData { get; set; }
            /// <summary>Creates the data associated with the implementing object.</summary>
            public void CreateData();
        }

        /// <summary>Provides entity data of a specific type.</summary>
        /// <typeparam name="D">The entity data type.</typeparam>
        public interface IHasData<D> : IHasData where D : IEntityData
        {
            IEntityData IHasData.EntityData { get => EntityData; set => EntityData = (D)value; }
            /// <summary>Gets or sets the strongly typed entity data.</summary>
            public new D EntityData { get; set; }

            void IHasData.CreateData()
            {
                if(!typeof(D).IsAbstract)
                    EntityData = (D)Activator.CreateInstance(typeof(D));
            }
        }
    }
}
