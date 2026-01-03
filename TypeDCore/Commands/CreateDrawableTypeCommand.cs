using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.DTO;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using System.Threading.Tasks;

namespace TypeDCore.Commands
{
    internal class CreateDrawableTypeCommand : CustomCommand<CreateComponentCommandData>
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        IHookModel HookModel { get; set; }

        // Constructors
        public CreateDrawableTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            HookModel = ResourceModel.Get<IHookModel>();
        }

        public override async void Execute(CreateComponentCommandData parameter)
        {
            var dialog = new CreateDrawableTypeDialog(parameter.Project, parameter.Namespace);
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                if (!HookModel.Shoot(new CreateComponentHook(new CreateComponentDTO()
                {
                    Name = dialog.ViewModel.ComponentName,
                    Namespace = dialog.ViewModel.ComponentNamespace,
                    ParentComponent = dialog.ViewModel.ParentComponent
                })).Handled)
                {
                    if (dialog.ViewModel.ComponentBaseType == typeof(Drawable).FullName)
                    {
                        TypeDCoreProjectModel.CreateDrawable(parameter.Project, dialog.ViewModel.ComponentName, dialog.ViewModel.ComponentNamespace, dialog.ViewModel.ParentComponent);
                    }
                }
            }
        }
    }
}
