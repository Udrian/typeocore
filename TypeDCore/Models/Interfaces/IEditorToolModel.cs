using TypeD.Models.Interfaces;
using TypeDCore.Models.Data;

namespace TypeDCore.Models.Interfaces
{
    public interface IEditorToolModel : IModel
    {
        EditorTool ActiveTool { get; set; }
        bool TryCapturePointer(object owner);
        void ReleasePointer(object owner);
    }
}
