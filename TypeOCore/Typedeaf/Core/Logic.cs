using TypeOEngine.Typedeaf.Core.Engine;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    /// <summary>
    /// Represents the base class for implementing logic components that can be updated over time.
    /// </summary>
    /// <remarks>This class provides a foundation for logic components that are associated with an entity and
    /// can be paused or updated. Derived classes must implement the <see cref="Update(double)"/> method to define
    /// specific update behavior.</remarks>
    public abstract class Logic : TypeOObject, IUpdatable
    {
        /// <summary>
        /// Gets or sets the parent entity of the current entity.
        /// </summary>
        public Entity Parent { get; set; } //TODO: Look over this

        /// <summary>
        /// Gets or sets a value indicating whether the operation is paused.
        /// </summary>
        public bool Pause { get; set; }

        /// <summary>
        /// Updates the state of the object based on the elapsed time.
        /// </summary>
        /// <remarks>This method is intended to be overridden in a derived class to implement custom update logic.</remarks>
        /// <param name="dt">The time, in seconds, that has elapsed since the last update. Must be a non-negative value.</param>
        public abstract void Update(double dt);
    }
}
