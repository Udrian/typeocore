using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Notifies listeners that the viewer for a project was unloaded.
    /// </summary>
    public class ViewerUnloadedHook : Hook
    {
        /// <summary>
        /// The project whose viewer was unloaded.
        /// </summary>
        public Project Project { get; set; }
    }
}
