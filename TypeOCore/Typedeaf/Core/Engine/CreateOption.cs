namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Represents an abstract base class for creating objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>This class provides a contract for implementing object creation logic. Derived
        /// classes must override the <see cref="Create"/> method to define the specific creation behavior.</remarks>
        /// <typeparam name="T">The type of object to be created.</typeparam>
        public abstract class CreateOption<T>
        {
            /// <summary>
            /// Creates a new instance of the specified object in the underlying data store.
            /// </summary>
            /// <param name="obj">The object to create. Must not be <see langword="null"/>.</param>
            /// <returns><see langword="true"/> if the object was successfully created; otherwise, <see langword="false"/>.</returns>
            public abstract bool Create(T obj);
        }
    }
}
