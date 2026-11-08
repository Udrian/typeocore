using TypeD.Models.Interfaces;
using TypeD.Models.Data;

namespace TypeDCore.Commands
{
    internal sealed class DuplicateNodeCommand : NodeClipboardCommand
    {
        public DuplicateNodeCommand(IResourceModel resources)
            : base(resources, "node.duplicate", "Duplicate node", "Ctrl+D") { }

        public override bool CanExecute(NodeClipboardCommandData parameter) => Clipboard.CanDuplicate(parameter);
        public override void Execute(NodeClipboardCommandData parameter) => Run(() =>
        {
            Clipboard.Duplicate(parameter);
            return System.Threading.Tasks.Task.CompletedTask;
        });
    }
}
