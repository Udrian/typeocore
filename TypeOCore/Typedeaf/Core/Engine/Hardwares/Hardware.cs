using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Hardwares
    {
        /// <summary>
        /// Represents an abstract base class for hardware components, providing a common foundation for hardware-related objects.
        /// </summary>
        /// <remarks>This class implements the <see cref="IHasContext"/> interface, allowing it to
        /// associate a <see cref="Context"/> object with the hardware instance. Derived classes should extend this
        /// type to define specific hardware behaviors and properties.</remarks>
        public abstract class Hardware : TypeOObject, IHasContext
        {
            Context IHasContext.Context { get; set; }
            protected Context Context { get => (this as IHasContext).Context; set => (this as IHasContext).Context = value; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Hardware"/> class.
            /// </summary>
            /// <remarks>This constructor is protected and is intended to be used by derived
            /// classes.</remarks>
            protected Hardware() { }
        }
    }
}
