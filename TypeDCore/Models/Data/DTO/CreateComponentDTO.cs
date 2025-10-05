using TypeD.Models.Data;

namespace TypeDCore.Models.Data.DTO
{
    /// <summary>
    /// Represents the data transfer object (DTO) used to create a new <see cref="Component"/>.
    /// </summary>
    /// <remarks>This class is used to encapsulate the necessary information for creating a <see cref="Component"/>, 
    /// including its name, namespace, and parent <see cref="Component"/>. All properties are required.</remarks>
    public class CreateComponentDTO
    {
        /// <summary>
        /// Gets or sets the name of the <see cref="Component"/>.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the namespace for the current <see cref="Component"/>.
        /// </summary>
        public required string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the parent <see cref="Component"/> of the current <see cref="Component"/>.
        /// </summary>
        public required Component ParentComponent { get; set; }
    }
}
