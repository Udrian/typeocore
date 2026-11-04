using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// The camera actions a viewer can be asked to perform.
    /// </summary>
    public enum ViewerCameraAction
    {
        /// <summary>
        /// Moves the camera to a component, zooming out if it doesn't fit.
        /// </summary>
        TeleportToSelection,
        /// <summary>
        /// Resets the camera to its default position and zoom.
        /// </summary>
        ResetView
    }

    /// <summary>
    /// Asks the viewer displaying a project to move its camera.
    /// </summary>
    public class ViewerCameraHook : Hook
    {
        /// <summary>
        /// The project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }
        /// <summary>
        /// The camera action to perform.
        /// </summary>
        public ViewerCameraAction Action { get; set; }
        /// <summary>
        /// The component to teleport to, or null to use the viewer's selected node.
        /// </summary>
        public Component Target { get; set; }

        /// <summary>
        /// Creates a viewer camera request.
        /// </summary>
        public ViewerCameraHook() { }
    }
}
