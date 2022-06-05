namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        public interface IUpdatable
        {
            public bool Pause { get; set; }
            void Update(double dt);
        }
    }
}