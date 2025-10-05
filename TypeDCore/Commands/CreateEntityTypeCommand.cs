using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.DTO;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeDCore.Commands
{
    internal class CreateEntityTypeCommand : CustomCommand<CreateComponentCommandData>
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        IHookModel HookModel { get; set; }

        // Constructors
        public CreateEntityTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            HookModel = ResourceModel.Get<IHookModel>();
        }

        public override void Execute(CreateComponentCommandData parameter)
        {
            var dialog = new CreateEntityTypeDialog(parameter.Project, parameter.Namespace);
            if(dialog.ShowDialog() == true)
            {
                if (!HookModel.Shoot(new CreateComponentHook(new CreateComponentDTO()
                {
                    Name = dialog.ViewModel.ComponentName,
                    Namespace = dialog.ViewModel.ComponentNamespace,
                    ParentComponent = dialog.ViewModel.ParentComponent
                })).Handled)
                {
                    if (dialog.ViewModel.ComponentBaseType == typeof(Entity).FullName)
                    {
                        TypeDCoreProjectModel.CreateEntity(parameter.Project, dialog.ViewModel.ComponentName, dialog.ViewModel.ComponentNamespace, dialog.ViewModel.ParentComponent, dialog.ViewModel.ComponentUpdatable, dialog.ViewModel.ComponentDrawable);
                    }
                }
            }
        }
    }
}
