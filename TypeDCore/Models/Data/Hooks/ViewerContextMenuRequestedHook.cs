using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Fired when the user right clicks a component in the viewer
    /// </summary>
    public class ViewerContextMenuRequestedHook : Hook
    {
        public Project Project { get; set; }
        public Component Component { get; set; }

        public ViewerContextMenuRequestedHook() { }
    }
}
