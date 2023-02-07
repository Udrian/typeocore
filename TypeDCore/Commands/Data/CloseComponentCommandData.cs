using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal class CloseComponentCommandData
    {
        public Project Project { get; set; }
        public Component Component { get; set; }
    }
}
