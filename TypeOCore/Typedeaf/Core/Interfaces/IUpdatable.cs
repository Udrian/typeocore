using TypeOEngine.Typedeaf.Core.Attributes;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        public interface IUpdatable
        {
            /// <summary>
            /// Gets or sets a value indicating whether the component is paused.
            /// </summary>
            [TypeOProperty("Gets or sets a value indicating whether the component is paused.", false)]
            public bool Pause { get; set; }
            void Update(double dt);
        }
    }
}