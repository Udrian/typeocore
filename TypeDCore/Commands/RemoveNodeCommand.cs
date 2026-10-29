using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Linq;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class RemoveNodeCommand : CustomCommand<RemoveNodeCommandData>, IDisposable
    {
        private readonly IComponentModel _componentModel;
        private readonly ICommandHistoryModel _commandHistoryModel;
        private readonly INodeSelectionModel _nodeSelectionModel;
        private readonly IDisposable _registration;

        public RemoveNodeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            _componentModel = resourceModel.Get<IComponentModel>();
            _commandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            _nodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "node.remove",
                "Remove node",
                "Remove the selected node from its parent.",
                "global",
                "Delete",
                this,
                () => new RemoveNodeCommandData { Project = _nodeSelectionModel.Project });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(RemoveNodeCommandData parameter)
        {
            var selectedNode = _nodeSelectionModel.SelectedNode;
            return parameter?.Project != null
                && selectedNode != null
                && _nodeSelectionModel.IsComponentExplorerActive
                && !ReferenceEquals(selectedNode, _nodeSelectionModel.OpenedComponent)
                && selectedNode.ParentComponent != null
                && !selectedNode.Removed;
        }

        private static bool IsInheritedFromInstance(TypeD.Models.Data.Component component)
        {
            var parent = component.ParentComponent;
            return parent?.ParentComponent != null && parent.DefinitionChildren.Contains(component);
        }

        public override async void Execute(RemoveNodeCommandData parameter)
        {
            var component = _nodeSelectionModel.SelectedNode;
            if (component == null || ReferenceEquals(component, _nodeSelectionModel.OpenedComponent) || component.Removed)
                return;

            var name = component.Properties.FirstOrDefault(property => property.Name == "Name")?.Value as string;
            if (string.IsNullOrEmpty(name))
                name = component.ClassName;
            var box = MessageBoxManager.GetMessageBoxStandard("Remove Node", $"Remove node '{name}'?", ButtonEnum.YesNoCancel, Icon.Question);
            var result = await box.ShowWindowDialogAsync(ViewModelBase.MainWindow);
            if (result != ButtonResult.Yes)
                return;

            var componentName = component.FullName;
            var parent = component.ParentComponent;
            if (IsInheritedFromInstance(component))
            {
                _componentModel.SetRemoved(parameter.Project, component, true);
                _commandHistoryModel.Record(
                    "Remove node",
                    $"Removed inherited {componentName} from {parent.FullName}.",
                    () => _componentModel.SetRemoved(parameter.Project, component, false),
                    () => _componentModel.SetRemoved(parameter.Project, component, true));
                return;
            }

            _componentModel.Remove(parameter.Project, component);
            if (parent == null)
                return;

            _commandHistoryModel.Record(
                "Remove node",
                $"Removed {componentName} from {parent.FullName}.",
                () => _componentModel.AddExisting(parameter.Project, parent, component),
                () => _componentModel.Remove(parameter.Project, component));
        }
    }
}
