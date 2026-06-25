using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal class RemoveComponentCommandData
    {
        public Project Project { get; set; }
        public Component Component { get; set; }
    }
}
