using TypeD.Models.Interfaces;
using TypeDCore.Models.Data;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models
{
    internal sealed class EditorToolModel : IEditorToolModel
    {
        private object _pointerOwner;

        public EditorTool ActiveTool { get; set; }
        public bool MultiSelectModifierDown { get; set; }
        public bool ShowBoundingBoxes { get; set; }
        public BoundingBoxDisplayMode BoundingBoxDisplayMode { get; set; }
        public Color SelectedBoundingBoxColor { get; set; }
        public Color UnselectedBoundingBoxColor { get; set; }
        public Color ParentBoundingBoxColor { get; set; }
        public Color ChildBoundingBoxColor { get; set; }

        public void Init(IResourceModel resourceModel)
        {
            ActiveTool = EditorTool.Selection;
            ShowBoundingBoxes = true;
            BoundingBoxDisplayMode = BoundingBoxDisplayMode.All;
            SelectedBoundingBoxColor = Color.Green;
            UnselectedBoundingBoxColor = Color.Red;
            ParentBoundingBoxColor = Color.Yellow;
            ChildBoundingBoxColor = Color.Cyan;
        }

        public bool TryCapturePointer(object owner)
        {
            if (_pointerOwner != null)
                return false;

            _pointerOwner = owner;
            return true;
        }

        public void ReleasePointer(object owner)
        {
            if (ReferenceEquals(_pointerOwner, owner))
                _pointerOwner = null;
        }
    }
}
