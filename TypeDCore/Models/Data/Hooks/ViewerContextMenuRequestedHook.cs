using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Fired when the user right clicks a component in the viewer
    /// </summary>
    public class ViewerContextMenuRequestedHook : Hook
    {
        /// <summary>
        /// The project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }
        /// <summary>
        /// The component whose context menu was requested.
        /// </summary>
        public Component Component { get; set; }

        /// <summary>
        /// Creates a viewer context-menu request.
        /// </summary>
        public ViewerContextMenuRequestedHook() { }
    }
}
