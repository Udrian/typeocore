using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeOEngine.Typedeaf.Core.Entities
{
    /// <summary>
    /// Represents an abstract base class for options used to configure the creation of an entity of type <typeparamref name="E"/>.
    /// </summary>
    /// <remarks>This class serves as a foundation for defining specific configuration options for creating
    /// entities. It extends <see cref="CreateOption{E}"/> and enforces that the entity type <typeparamref name="E"/>
    /// inherits from <see cref="Entity"/>.</remarks>
    /// <typeparam name="E">The type of the entity being configured. Must derive from <see cref="Entity"/>.</typeparam>
    public abstract class EntityOption<E> : CreateOption<E> where E : Entity { }
}
