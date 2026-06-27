namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Represents an abstract base class for creating options specific to a type of logic.
        /// </summary>
        /// <typeparam name="L">The type of logic that this option applies to. Must derive from <see cref="Logic"/>.</typeparam>
        public abstract class LogicOption<L> : CreateOption<L> where L : Logic
        {
        }
    }
}
