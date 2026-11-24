namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        /// <summary>Marks an updatable component that must run on the game-loop thread.</summary>
        /// <remarks>Use for platform APIs that require thread affinity, such as native window event polling.</remarks>
        public interface IMainThreadUpdatable : IUpdatable
        {
        }
    }
}
