using TypeD.Commands;
using TypeD.ViewModel;
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

        public override async void Execute(OpenComponentCommandData parameter)
        {
            if(parameter.Component == null)
            {
                var componentSelectorDialog = new ComponentSelectorDialog(parameter.Project);
                await componentSelectorDialog.ShowDialog(ViewModelBase.MainWindow);
                parameter.Component = componentSelectorDialog.ViewModel.SelectedComponent;
            }

            if (parameter.Component == null) return;

            ComponentModel.Open(parameter.Project, parameter.Component);
        }
    }
}
