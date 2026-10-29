using System;
using System.Collections.Generic;
using System.Linq;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class RestoreNodeCommand : CustomCommand<RemoveNodeCommandData>, IDisposable
    {
        private readonly IComponentModel _componentModel;
        private readonly ICommandHistoryModel _commandHistoryModel;
        private readonly INodeSelectionModel _nodeSelectionModel;
        private readonly IDisposable _registration;

        public RestoreNodeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            _componentModel = resourceModel.Get<IComponentModel>();
            _commandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            _nodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "node.restore",
                "Restore deleted node",
                "Restore the selected node that was removed from its parent.",
                "global",
                null,
                this,
                () => new RemoveNodeCommandData { Project = _nodeSelectionModel.Project });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(RemoveNodeCommandData parameter)
        {
            return parameter?.Project != null && GetRemoved().Count > 0;
        }

        private List<TypeD.Models.Data.Component> GetRemoved()
        {
            var selected = _nodeSelectionModel.SelectedNodes.ToList();
            if (selected.Count == 0 && _nodeSelectionModel.SelectedNode != null)
                selected.Add(_nodeSelectionModel.SelectedNode);
            return selected.Where(node => node.Removed).ToList();
        }

        public override void Execute(RemoveNodeCommandData parameter)
        {
            var components = GetRemoved();
            if (components.Count == 0)
                return;

            var project = parameter.Project;
            components.ForEach(component => _componentModel.SetRemoved(project, component, false));
            _commandHistoryModel.Record(
                "Restore node",
                components.Count == 1
                    ? $"Restored {components[0].FullName} in {components[0].ParentComponent?.FullName}."
                    : $"Restored {components.Count} nodes.",
                () => components.ForEach(component => _componentModel.SetRemoved(project, component, true)),
                () => components.ForEach(component => _componentModel.SetRemoved(project, component, false)));
        }
    }
}
