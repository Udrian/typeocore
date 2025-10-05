using TypeD.Models.Data;
using TypeDCore.Models.Data.DTO;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Represents a hook used to facilitate the creation of a <see cref="Component"/>.
    /// </summary>
    /// <remarks>This class provides mechanisms to initialize and manage the details required for creating a
    /// <see cref="Component"/>. It includes properties to specify the creation details and track whether the hook has
    /// been handled.</remarks>
    public class CreateComponentHook : Hook
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateComponentHook"/> class.
        /// </summary>
        /// <remarks>This constructor initializes the <see cref="CreateComponentDTO"/> property with
        /// default values: <see cref="CreateComponentDTO.Name"/> is set to an empty string,  <see
        /// cref="CreateComponentDTO.Namespace"/> is set to an empty string,  and <see
        /// cref="CreateComponentDTO.ParentComponent"/> is initialized as a new <see cref="Component"/>
        /// instance.</remarks>
        public CreateComponentHook()
        {
            CreateComponentDTO = new CreateComponentDTO()
            {
                Name = string.Empty,
                Namespace = string.Empty,
                ParentComponent = new Component()
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateComponentHook"/> class with the specified component
        /// creation data.
        /// </summary>
        /// <param name="createComponentDTO">The data transfer object containing the details required to create a component. Cannot be <see langword="null"/>.</param>
        public CreateComponentHook(CreateComponentDTO createComponentDTO)
        {
            CreateComponentDTO = createComponentDTO;
        }

        /// <summary>
        /// Gets or sets the data transfer object (DTO) used to create a new component.
        /// </summary>
        public CreateComponentDTO CreateComponentDTO { get; set; }

        /// <summary>
        /// True if the hook has been handled, meaning the <see cref="Component"/> creation has been processed and no further action is needed.
        /// </summary>
        public bool Handled { get; set; } = false;
    }
}
