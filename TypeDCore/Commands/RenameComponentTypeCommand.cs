using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class RenameComponentTypeCommand : CustomCommand<ComponentCommandData>
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Constructors
        public RenameComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
        }

        public override void Execute(ComponentCommandData parameter)
        {
            var dialog = new RenameComponentTypeDialog(parameter.Component);
            if (dialog.ShowDialog() == true)
            {
                ComponentProvider.Rename(parameter.Project, parameter.Component, dialog.ViewModel.Name);
            }
        }
    }
}
