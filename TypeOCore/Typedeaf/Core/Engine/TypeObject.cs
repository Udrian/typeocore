namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Base class for Initializing and cleanup, will automatically call both Initialize and Cleanup on object when relevant. Do not use constructor in a TypeOObject class to access TypeO objects.
        /// </summary>
        public abstract class TypeObject
        {
            internal void DoInitialize()
            {
                Initialize();
            }

            /// <summary>
            /// Called on object after all TypeO objects have been loaded and referenced.
            /// </summary>
            protected abstract void Initialize();

            internal void DoCleanup()
            {
                Cleanup();
            }

            /// <summary>
            /// Called when object is ready to be cleaned up.
            /// </summary>
            protected abstract void Cleanup();
        }
    }
}
