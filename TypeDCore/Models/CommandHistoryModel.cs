using System;
using System.Collections.ObjectModel;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models
{
    internal class CommandHistoryModel : ICommandHistoryModel
    {
        private int _currentIndex = -1;

        public void Init(IResourceModel resourceModel)
        {
        }

        public ObservableCollection<CommandHistoryEntry> Entries { get; } = new();
        public bool CanUndo => _currentIndex >= 0 && _currentIndex < Entries.Count;
        public bool CanRedo => _currentIndex > 0;
        public string CurrentPosition => _currentIndex < 0 || _currentIndex >= Entries.Count
            ? "Before the first command"
            : Entries[_currentIndex].CommandName;
        public event EventHandler HistoryChanged;

        public void Record(string commandName, string context, Action undoAction, Action redoAction)
        {
            while (_currentIndex > 0)
            {
                Entries.RemoveAt(0);
                _currentIndex--;
            }

            Entries.Insert(0, new CommandHistoryEntry(commandName, context, undoAction, redoAction));
            _currentIndex = 0;
            UpdateEntryStates();
        }

        public bool Undo()
        {
            if (!CanUndo)
                return false;

            Entries[_currentIndex].Undo();
            _currentIndex++;
            UpdateEntryStates();
            return true;
        }

        public bool Redo()
        {
            if (!CanRedo)
                return false;

            var nextIndex = _currentIndex - 1;
            Entries[nextIndex].Redo();
            _currentIndex = nextIndex;
            UpdateEntryStates();
            return true;
        }

        private void UpdateEntryStates()
        {
            for (var index = 0; index < Entries.Count; index++)
            {
                Entries[index].SetHistoryState(index == _currentIndex, index < _currentIndex);
            }

            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
