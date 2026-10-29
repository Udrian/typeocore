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

    internal class ComponentMove
    {
        public Component Node { get; set; }
        public Component Target { get; set; }
        public MovePosition Position { get; set; }
    }

    internal class ComponentMoveCommandData
    {
        public Project Project { get; set; }
        public Component Node { get; set; }
        /// <summary>
        /// All nodes to move together with <see cref="Node"/>. When null only <see cref="Node"/> is moved.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<Component> Nodes { get; set; }
        /// <summary>
        /// Explicit moves, executed in order as a single undoable step. Overrides Node, Nodes, Target and Position.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<ComponentMove> Moves { get; set; }
        /// <summary>
        /// The node to move relative to. When null a dialog asks for the target.
        /// </summary>
        public Component Target { get; set; }
        public MovePosition Position { get; set; } = MovePosition.Child;
    }
}
