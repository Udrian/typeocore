using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeOEngine.Typedeaf.Core
{
    namespace Engine
    {
        /// <summary>
        /// Represents an abstract base class for options used to create drawable objects of type <typeparamref name="D"/>.
        /// </summary>
        /// <remarks>This class serves as a foundation for defining configuration options specific to
        /// drawable objects. It is intended to be extended by concrete implementations that provide additional
        /// properties or behavior tailored to specific drawable types.</remarks>
        /// <typeparam name="D">The type of drawable object that this option is associated with. Must derive from <see cref="Drawable"/>.</typeparam>
        public abstract class DrawableOption<D> : CreateOption<D> where D : Drawable { }
    }
}
