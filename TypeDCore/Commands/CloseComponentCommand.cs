using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class CloseComponentCommand : CustomCommand<CloseComponentCommandData>
    {
        // Models
        IComponentModel ComponentModel { get; set; }

        public CloseComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
        }

        public override void Execute(CloseComponentCommandData parameter)
        {
            if (parameter.Component == null) return;

            ComponentModel.Close(parameter.Project, parameter.Component);
        }
    }
}
