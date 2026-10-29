using System;
using System.Collections.Generic;
using System.Linq;
using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core;

namespace TypeDCore.Commands
{
    internal class AddNodeCommand : CustomCommand<AddNodeCommandData>, IDisposable
    {
        // Models
        IComponentModel ComponentModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        INodeSelectionModel NodeSelectionModel { get; set; }
        private readonly IDisposable Registration;

        // Constructors
        public AddNodeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            NodeSelectionModel = ResourceModel.Get<INodeSelectionModel>();
            Registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "node.add-child",
                "Add child node",
                "Add a child node to the selected node.",
                "global",
                "Ctrl+Shift+A",
                this,
                () => new AddNodeCommandData { Project = NodeSelectionModel.Project });
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(AddNodeCommandData parameter)
        {
            return parameter?.Project != null && GetParents().Count > 0;
        }

        private List<TypeD.Models.Data.Component> GetParents()
        {
            var selected = NodeSelectionModel.SelectedNodes.ToList();
            if (selected.Count == 0 && NodeSelectionModel.SelectedNode != null)
                selected.Add(NodeSelectionModel.SelectedNode);
            return selected.Where(node => !node.ReadOnly && !node.Removed).ToList();
        }

        public override async void Execute(AddNodeCommandData parameter)
        {
            var dialog = new ComponentSelectorDialog(parameter.Project);

            var parents = GetParents();
            var parentComponent = NodeSelectionModel.SelectedNode is { } primary && parents.Contains(primary) ? primary : parents.FirstOrDefault();
            if (parentComponent == null)
                return;

            var excludeComponent = parentComponent;
            while(excludeComponent.ParentComponent != null)
            {
                excludeComponent = excludeComponent.ParentComponent;
            }

            dialog.ViewModel.NameFilter.Exclude = $"{excludeComponent.FullName};";
            dialog.ViewModel.TypeFilter.Exclude = $"{typeof(Game).FullName};{typeof(TypeOEngine.Typedeaf.Core.Scene).FullName};";

            parentComponent.Template.ChildrenFilter(dialog.ViewModel.TypeFilter);
            dialog.ViewModel.ExcludeAbstract = true;

            dialog.ViewModel.UpdateFilter();
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                var selected = dialog.ViewModel.SelectedComponent;
                var undoActions = new List<Action>();
                var redoActions = new List<Action>();
                foreach (var parent in parents)
                {
                    var filter = new TypeD.Helpers.FilterHelper();
                    parent.Template?.ChildrenFilter(filter);
                    if (!ReferenceEquals(parent, parentComponent)
                        && !(filter.Filters ?? string.Empty).Contains($"{selected.RootBaseComponent.FullName};"))
                        continue;

                    var previousChildren = new HashSet<TypeD.Models.Data.Component>(parent.Children);
                    ComponentModel.Add(parameter.Project, parent, selected);
                    var addedComponent = parent.Children.LastOrDefault(child => !previousChildren.Contains(child));
                    if (addedComponent == null)
                        throw new InvalidOperationException("The selected component was not added to its parent.");

                    var addedParent = parent;
                    undoActions.Add(() => ComponentModel.Remove(parameter.Project, addedComponent));
                    redoActions.Add(() => ComponentModel.AddExisting(parameter.Project, addedParent, addedComponent));
                }

                if (undoActions.Count == 0)
                    return;

                undoActions.Reverse();
                CommandHistoryModel.Record(
                    "Add node",
                    undoActions.Count == 1
                        ? $"Added {selected.FullName} to {parentComponent.FullName}."
                        : $"Added {selected.FullName} to {undoActions.Count} nodes.",
                    () => undoActions.ForEach(action => action()),
                    () => redoActions.ForEach(action => action()));
            }
        }
    }
}
