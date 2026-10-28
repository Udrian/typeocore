using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal class ChangePropertyCommandData
    {
        public Project Project { get; set; }
        public Component Component { get; set; }
        public Property Property { get; set; }
        public object Value { get; set; }
    }
}
