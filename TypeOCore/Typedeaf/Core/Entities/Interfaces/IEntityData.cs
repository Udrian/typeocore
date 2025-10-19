namespace TypeOEngine.Typedeaf.Core
{
    namespace Entities.Interfaces
    {
        /// <summary>
        /// Defines the contract for initializing entity data.
        /// </summary>
        /// <remarks>This interface is intended to be implemented by classes that require an
        /// initialization step for their entity data. The <see cref="Initialize"/> method should be called to prepare
        /// the entity for use.</remarks>
        public interface IEntityData
        {
            /// <summary>
            /// Initializes the object to a usable state.
            /// </summary>
            /// <remarks>This method must be called before using the object. Failure to call this
            /// method may result in undefined behavior.</remarks>
            public void Initialize();
        }
    }
}