using TypeOEngine.Typedeaf.Core.Attributes;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Base class for Initializing and cleanup, will automatically call both Initialize and Cleanup on object when relevant. Do not use constructor in a TypeOObject class to access TypeO objects.
        /// </summary>
        public abstract class TypeOObject
        {
            /// <summary>
            /// Gets the unique identifier.
            /// </summary>
            [TypeOProperty("Gets the unique identifier.", "")]
            public string ID { get; internal set; }

            /// <summary>
            /// Gets or sets the human readable name of the object.
            /// </summary>
            [TypeOProperty("Gets or sets the human readable name of the object.", "")]
            public virtual string Name { get; set; }

            /// <summary>
            /// Gets a value indicating whether the object has been successfully initialized.
            /// </summary>
            public bool Initialized { get; internal set; }

            internal void DoInitialize()
            {
                Initialize();
                Initialized = true;
            }

            /// <summary>
            /// Called on object after all TypeO objects have been loaded and referenced.
            /// </summary>
            protected abstract void Initialize();

            internal void DoCleanup()
            {
                Cleanup();
                Initialized = false;
            }

            /// <summary>
            /// Called when object is ready to be cleaned up.
            /// </summary>
            protected abstract void Cleanup();
        }
    }
}
