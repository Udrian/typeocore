using System;
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
            return parameter?.Project != null && _nodeSelectionModel.SelectedNode?.Removed == true;
        }

        public override void Execute(RemoveNodeCommandData parameter)
        {
            var component = _nodeSelectionModel.SelectedNode;
            if (component == null || !component.Removed)
                return;

            var parent = component.ParentComponent;
            _componentModel.SetRemoved(parameter.Project, component, false);
            _commandHistoryModel.Record(
                "Restore node",
                $"Restored {component.FullName} in {parent?.FullName}.",
                () => _componentModel.SetRemoved(parameter.Project, component, true),
                () => _componentModel.SetRemoved(parameter.Project, component, false));
        }
    }
}
