using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    /// <summary>
    /// Parameter of the viewer camera commands.
    /// </summary>
    public class ViewerCameraCommandData
    {
        /// <summary>
        /// The project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }
        /// <summary>
        /// The component to teleport to, or null to use the selected node.
        /// </summary>
        public Component Component { get; set; }
    }
}
