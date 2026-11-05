using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>Asks a focused viewer for its node-command context.</summary>
    public class ViewerNodeContextHook : Hook
    {
        /// <summary>Gets or sets the viewer project.</summary>
        public Project Project { get; set; }
        /// <summary>Gets or sets whether the viewer has keyboard focus.</summary>
        public bool Focused { get; set; }
        /// <summary>Gets or sets the current pointer position in world coordinates.</summary>
        public Vec2 Position { get; set; }
    }
}
