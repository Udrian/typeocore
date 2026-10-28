using System.Collections.ObjectModel;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models
{
    internal class CommandHistoryModel : ICommandHistoryModel
    {
        public void Init(IResourceModel resourceModel)
        {
        }

        public ObservableCollection<CommandHistoryEntry> Entries { get; } = new();

        public void Record(string commandName, string context)
        {
            Entries.Insert(0, new CommandHistoryEntry(commandName, context));
        }
    }
}
