using TypeOEngine.Typedeaf.Core.Engine.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine.Services
    {
        /// <summary>
        /// Represents an abstract base class for services that operate within a specific context.
        /// </summary>
        /// <remarks>This class provides a foundation for services that require a shared context for their
        /// operations. Derived classes can access the context through the protected <see cref="Context"/> property.</remarks>
        public abstract class Service : TypeOObject, IHasContext
        {
            Context IHasContext.Context { get; set; }
            protected Context Context { get => (this as IHasContext).Context; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Service"/> class.
            /// </summary>
            /// <remarks>This constructor is protected and intended to be used by derived classes to
            /// initialize the base functionality of the <see cref="Service"/> class.</remarks>
            protected Service() { }
        }
    }
}
