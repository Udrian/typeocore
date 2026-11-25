using TypeD.Models.Data;
using TypeDCore.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>Requests a keyboard transform of the selected viewer components.</summary>
    public class ViewerNudgeHook : Hook
    {
        /// <summary>Gets or sets the project displayed in the viewer.</summary>
        public Project Project { get; set; }
        /// <summary>Gets or sets the selected components.</summary>
        public IReadOnlyList<Component> Components { get; set; }
        /// <summary>Gets or sets the active transform tool.</summary>
        public EditorTool Tool { get; set; } = EditorTool.Move;
        /// <summary>Gets or sets the requested direction and step.</summary>
        public Vec2 Delta { get; set; }
        /// <summary>Gets or sets whether a resize should shrink the selected edge.</summary>
        public bool Shrink { get; set; }
        /// <summary>Gets or sets whether the viewer module handled the request.</summary>
        public bool Handled { get; set; }
    }
}
