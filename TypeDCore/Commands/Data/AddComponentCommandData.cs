using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal class AddComponentCommandData
    {
        public Project Project { get; set; }
        public Component ToComponent { get; set; }
    }
}
