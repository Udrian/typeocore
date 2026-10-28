using System;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class CloseComponentCommand : CustomCommand<CloseComponentCommandData>, IDisposable
    {
        // Models
        IComponentModel ComponentModel { get; set; }
        private readonly IDisposable _registration;

        public CloseComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.close",
                "Close component",
                "Close the specified component.",
                "global",
                string.Empty,
                this);
        }

        public void Dispose() => _registration.Dispose();

        public override void Execute(CloseComponentCommandData parameter)
        {
            if (parameter.Component == null) return;

            ComponentModel.Close(parameter.Project, parameter.Component);
        }
    }
}
