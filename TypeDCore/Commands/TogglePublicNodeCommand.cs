using System;
using System.Collections.Generic;
using System.Linq;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class TogglePublicNodeCommand : CustomCommand<RemoveNodeCommandData>, IDisposable
    {
        private readonly IComponentModel _componentModel;
        private readonly ICommandHistoryModel _commandHistoryModel;
        private readonly INodeSelectionModel _nodeSelectionModel;
        private readonly IDisposable _registration;

        public TogglePublicNodeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            _componentModel = resourceModel.Get<IComponentModel>();
            _commandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            _nodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "node.toggle-public",
                "Toggle node public/private",
                "Toggle whether the selected nodes are exposed to components that contain their component.",
                "global",
                null,
                this,
                () => new RemoveNodeCommandData { Project = _nodeSelectionModel.Project });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(RemoveNodeCommandData parameter)
        {
            return parameter?.Project != null && GetTargets().Count > 0;
        }

        private List<TypeD.Models.Data.Component> GetTargets()
        {
            var selected = _nodeSelectionModel.SelectedNodes.ToList();
            if (selected.Count == 0 && _nodeSelectionModel.SelectedNode != null)
                selected.Add(_nodeSelectionModel.SelectedNode);
            return selected.Where(node => node.CanTogglePublic).ToList();
        }

        public override void Execute(RemoveNodeCommandData parameter)
        {
            var components = GetTargets();
            if (components.Count == 0)
                return;

            var project = parameter.Project;
            var makePublic = components.Any(component => !component.Public);
            var previous = components.Select(component => component.Public).ToList();

            void Apply(bool isPublic) => components.ForEach(component => _componentModel.SetPublic(project, component, isPublic));
            void Undo()
            {
                for (var i = 0; i < components.Count; i++)
                    _componentModel.SetPublic(project, components[i], previous[i]);
            }

            Apply(makePublic);
            _commandHistoryModel.Record(
                makePublic ? "Make node public" : "Make node private",
                components.Count == 1
                    ? $"Made {components[0].FullName} {(makePublic ? "public" : "private")}."
                    : $"Made {components.Count} nodes {(makePublic ? "public" : "private")}.",
                Undo,
                () => Apply(makePublic));
        }
    }
}
