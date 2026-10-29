using System;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class CloseComponentCommand : CustomCommand<CloseComponentCommandData>, IDisposable
    {
        // Models
        IComponentModel ComponentModel { get; set; }
        private readonly INodeSelectionModel _nodeSelectionModel;
        private readonly IDisposable _registration;

        public CloseComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
            _nodeSelectionModel = ResourceModel.Get<INodeSelectionModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.close",
                "Close component",
                "Close the opened component.",
                "global",
                string.Empty,
                this,
                () => new CloseComponentCommandData
                {
                    Project = _nodeSelectionModel.Project,
                    Component = _nodeSelectionModel.OpenedComponent
                });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(CloseComponentCommandData parameter)
        {
            return parameter?.Project != null && parameter.Component != null;
        }

        public override void Execute(CloseComponentCommandData parameter)
        {
            if (parameter.Component == null) return;

            ComponentModel.Close(parameter.Project, parameter.Component);
        }
    }
}
