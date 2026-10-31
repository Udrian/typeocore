using TypeD.Models.Interfaces;
using TypeDCore.Models.Data;

namespace TypeDCore.Models.Interfaces
{
    /// <summary>
    /// Tracks the active viewer tool and pointer capture.
    /// </summary>
    public interface IEditorToolModel : IModel
    {
        /// <summary>
        /// Gets or sets the tool currently active in the viewer.
        /// </summary>
        EditorTool ActiveTool { get; set; }
        /// <summary>
        /// Attempts to capture the pointer for an owner.
        /// </summary>
        bool TryCapturePointer(object owner);
        /// <summary>
        /// Releases pointer capture held by an owner.
        /// </summary>
        void ReleasePointer(object owner);
    }
}
