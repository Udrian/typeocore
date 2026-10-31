using TypeOEngine.Typedeaf.Core.Attributes;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        /// <summary>Defines a component that can be paused and updated over time.</summary>
        public interface IUpdatable
        {
            /// <summary>
            /// Gets or sets a value indicating whether the component is paused.
            /// </summary>
            [TypeOProperty("Gets or sets a value indicating whether the component is paused.", false)]
            public bool Pause { get; set; }
            /// <summary>Updates the component using the elapsed time in seconds.</summary>
            void Update(double dt);
        }
    }
}