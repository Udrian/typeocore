using TypeD.Models.Interfaces;
using TypeD.Models.Data;

namespace TypeDCore.Commands
{
    internal sealed class PasteNodeCommand : NodeClipboardCommand
    {
        public PasteNodeCommand(IResourceModel resources)
            : base(resources, "node.paste", "Paste node", "Ctrl+V") { }

        // The system clipboard is asynchronous; keyboard invocation must read it afresh, even after another app changed it.
        public override bool CanExecute(NodeClipboardCommandData parameter) =>
            parameter?.Project != null && parameter.Node is { ReadOnly: false, Removed: false };
        public override void Execute(NodeClipboardCommandData parameter) => Run(() => Clipboard.PasteAsync(parameter));
    }
}
