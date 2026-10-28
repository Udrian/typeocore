using System.Collections.ObjectModel;
using System.Collections.Specialized;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.View.Panels;

namespace TypeDCore.ViewModel.Panels
{
    internal sealed class CommandHistoryViewModel : ViewModelBase
    {
        public ObservableCollection<CommandHistoryEntry> Entries { get; }

        public bool HasEntries => Entries.Count > 0;
        public bool HasNoEntries => !HasEntries;

        public CommandHistoryViewModel(CommandHistoryPanel element) : base(element)
        {
            var historyModel = ResourceModel.Get<ICommandHistoryModel>();

            Entries = historyModel.Entries;
            Entries.CollectionChanged += OnEntriesChanged;
        }

        private void OnEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasEntries));
            OnPropertyChanged(nameof(HasNoEntries));
        }
    }
}
