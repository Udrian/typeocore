using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class OpenComponentCommand : CustomCommand<OpenComponentCommandData>
    {
        // Models
        IComponentModel ComponentModel { get; set; }

        public OpenComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
        }

        public override void Execute(OpenComponentCommandData parameter)
        {
            if(parameter.Component == null)
            {
                var componentSelectorDialog = new ComponentSelectorDialog(parameter.Project);
                componentSelectorDialog.ShowDialog();
                parameter.Component = componentSelectorDialog.ViewModel.SelectedComponent;
            }

            if (parameter.Component == null) return;

            ComponentModel.Open(parameter.Project, parameter.Component);
        }
    }
}
