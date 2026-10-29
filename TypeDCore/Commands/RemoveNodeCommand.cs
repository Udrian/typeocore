using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
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
            return parameter?.Project != null
                && _nodeSelectionModel.IsComponentExplorerActive
                && GetRemovable().Count > 0;
        }

        private static bool IsInheritedFromInstance(TypeD.Models.Data.Component component) => component.IsInherited;

        private List<TypeD.Models.Data.Component> GetRemovable()
        {
            var selected = _nodeSelectionModel.SelectedNodes.ToList();
            if (selected.Count == 0 && _nodeSelectionModel.SelectedNode != null)
                selected.Add(_nodeSelectionModel.SelectedNode);

            var candidates = selected
                .Where(node => !ReferenceEquals(node, _nodeSelectionModel.OpenedComponent) && node.ParentComponent != null && !node.Removed)
                .ToList();
            return candidates.Where(node => !candidates.Any(other => !ReferenceEquals(other, node) && IsWithin(node, other))).ToList();
        }

        private static bool IsWithin(TypeD.Models.Data.Component component, TypeD.Models.Data.Component ancestor)
        {
            for (; component != null; component = component.ParentComponent)
            {
                if (ReferenceEquals(component, ancestor))
                    return true;
            }
            return false;
        }

        public override async void Execute(RemoveNodeCommandData parameter)
        {
            var components = GetRemovable();
            if (components.Count == 0)
                return;

            string message;
            if (components.Count == 1)
            {
                var name = components[0].Properties.FirstOrDefault(property => property.Name == "Name")?.Value as string;
                if (string.IsNullOrEmpty(name))
                    name = components[0].ClassName;
                message = $"Remove node '{name}'?";
            }
            else
            {
                message = $"Remove {components.Count} nodes?";
            }
            var box = MessageBoxManager.GetMessageBoxStandard("Remove Node", message, ButtonEnum.YesNoCancel, Icon.Question);
            var result = await box.ShowWindowDialogAsync(ViewModelBase.MainWindow);
            if (result != ButtonResult.Yes)
                return;

            var project = parameter.Project;
            var undoActions = new List<Action>();
            var redoActions = new List<Action>();
            foreach (var component in components)
            {
                var parent = component.ParentComponent;
                if (IsInheritedFromInstance(component))
                {
                    _componentModel.SetRemoved(project, component, true);
                    undoActions.Add(() => _componentModel.SetRemoved(project, component, false));
                    redoActions.Add(() => _componentModel.SetRemoved(project, component, true));
                    continue;
                }

                var index = parent.Children.IndexOf(component);
                _componentModel.Remove(project, component);
                undoActions.Add(() => _componentModel.AddExisting(project, parent, component, index));
                redoActions.Add(() => _componentModel.Remove(project, component));
            }

            undoActions.Reverse();
            var description = components.Count == 1
                ? $"Removed {components[0].FullName}."
                : $"Removed {components.Count} nodes.";
            _commandHistoryModel.Record(
                "Remove node",
                description,
                () => undoActions.ForEach(action => action()),
                () => redoActions.ForEach(action => action()));
        }
    }
}
