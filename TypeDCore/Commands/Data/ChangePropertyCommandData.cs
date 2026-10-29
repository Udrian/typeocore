using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    public class ChangePropertyCommandData
    {
        public Project Project { get; set; }
        public Property Property { get; set; }
        /// <summary>
        /// The component owning the property. Defaults to the selected node.
        /// </summary>
        public Component Component { get; set; }
        public object Value { get; set; }
        /// <summary>
        /// Also applies the value to the same-named property of the other selected nodes.
        /// </summary>
        public bool ApplyToSelection { get; set; }
    }
}
