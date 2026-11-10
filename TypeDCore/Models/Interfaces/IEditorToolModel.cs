using TypeD.Models.Interfaces;
using TypeDCore.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Interfaces
{
    /// <summary>
    /// Tracks the active viewer tool, pointer capture and the viewer display state.
    /// </summary>
    public interface IEditorToolModel : IModel
    {
        /// <summary>
        /// Gets or sets the tool currently active in the viewer.
        /// </summary>
        EditorTool ActiveTool { get; set; }
        /// <summary>
        /// Gets or sets whether the multi-select modifier (Ctrl) is held down, updated by the viewer control.
        /// </summary>
        bool MultiSelectModifierDown { get; set; }
        /// <summary>
        /// Gets or sets whether bounding boxes are drawn in the viewer.
        /// </summary>
        bool ShowBoundingBoxes { get; set; }
        /// <summary>
        /// Gets or sets which bounding boxes are drawn in the viewer.
        /// </summary>
        BoundingBoxDisplayMode BoundingBoxDisplayMode { get; set; }
        /// <summary>
        /// Gets or sets the color of selected bounding boxes.
        /// </summary>
        Color SelectedBoundingBoxColor { get; set; }
        /// <summary>
        /// Gets or sets the color of unselected bounding boxes.
        /// </summary>
        Color UnselectedBoundingBoxColor { get; set; }
        /// <summary>
        /// Gets or sets the color of the bounding boxes of parents of selected nodes.
        /// </summary>
        Color ParentBoundingBoxColor { get; set; }
        /// <summary>
        /// Gets or sets the color of the bounding boxes of children of selected nodes.
        /// </summary>
        Color ChildBoundingBoxColor { get; set; }
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
