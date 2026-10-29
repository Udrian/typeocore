using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal enum MovePosition
    {
        Before,
        After,
        Child,
        FirstChild
    }

    internal class ComponentMoveCommandData
    {
        public Project Project { get; set; }
        public Component Node { get; set; }
        /// <summary>
        /// The node to move relative to. When null a dialog asks for the target.
        /// </summary>
        public Component Target { get; set; }
        public MovePosition Position { get; set; } = MovePosition.Child;
    }
}
