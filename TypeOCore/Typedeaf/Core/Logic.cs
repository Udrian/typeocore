using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeOEngine.Typedeaf.Core
{
    public abstract class Logic : IUpdatable
    {
        public Entity Parent { get; set; } //TODO: Look over this
        public bool Pause { get; set; }

        public abstract void Initialize();
        public abstract void Update(double dt);
    }
}
