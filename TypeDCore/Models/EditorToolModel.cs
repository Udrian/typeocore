using TypeD.Models.Interfaces;
using TypeDCore.Models.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Models
{
    internal sealed class EditorToolModel : IEditorToolModel
    {
        private object _pointerOwner;

        public EditorTool ActiveTool { get; set; }

        public void Init(IResourceModel resourceModel)
        {
            ActiveTool = EditorTool.Selection;
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
