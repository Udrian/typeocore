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
                "node.add",
                "Add node",
                "Add a child node to the selected node.",
                "global",
                string.Empty,
                this,
                () => new AddNodeCommandData { Project = NodeSelectionModel.Project });
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(AddNodeCommandData parameter)
        {
            return parameter?.Project != null && NodeSelectionModel.SelectedNode != null;
        }

        public override async void Execute(AddNodeCommandData parameter)
        {
            var dialog = new ComponentSelectorDialog(parameter.Project);

            var parentComponent = NodeSelectionModel.SelectedNode;
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

            dialog.ViewModel.UpdateFilter();
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                var previousChildren = new HashSet<TypeD.Models.Data.Component>(parentComponent.Children);
                ComponentModel.Add(parameter.Project, parentComponent, dialog.ViewModel.SelectedComponent);
                var addedComponent = parentComponent.Children.LastOrDefault(child => !previousChildren.Contains(child));
                if (addedComponent == null)
                    throw new InvalidOperationException("The selected component was not added to its parent.");

                CommandHistoryModel.Record(
                    "Add node",
                    $"Added {addedComponent.FullName} to {parentComponent.FullName}.",
                    () => ComponentModel.Remove(parameter.Project, addedComponent),
                    () => ComponentModel.AddExisting(parameter.Project, parentComponent, addedComponent));
            }
        }
    }
}
