using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class CreateEntityTypeCommand : CustomCommand<CreateComponentCommandData>
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }

        // Constructors
        public CreateEntityTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
        }

        public override void Execute(CreateComponentCommandData parameter)
        {
            var dialog = new CreateEntityTypeDialog(parameter.Project, parameter.Namespace);
            if(dialog.ShowDialog() == true)
            {
                TypeDCoreProjectModel.CreateEntity(parameter.Project, dialog.ViewModel.ComponentName, dialog.ViewModel.ComponentNamespace, dialog.ViewModel.ParentComponent, dialog.ViewModel.ComponentUpdatable, dialog.ViewModel.ComponentDrawable);
            }
        }
    }
}
