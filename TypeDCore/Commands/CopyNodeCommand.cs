using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal sealed class CopyNodeCommand : NodeClipboardCommand
    {
        public CopyNodeCommand(IResourceModel resources)
            : base(resources, "node.copy", "Copy node", "Ctrl+C") { }

        public override bool CanExecute(NodeClipboardCommandData parameter) => Clipboard.CanCopy(parameter);
        public override void Execute(NodeClipboardCommandData parameter) => Run(() => Clipboard.CopyAsync(parameter));
    }
}
