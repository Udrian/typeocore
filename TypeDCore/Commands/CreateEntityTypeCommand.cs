using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.DTO;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;
using TypeD.Models.Providers.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeDCore.Commands
{
    internal class CreateEntityTypeCommand : CustomCommand<CreateComponentCommandData>
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        IComponentProvider ComponentProvider { get; set; }
        IHookModel HookModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }

        // Constructors
        public CreateEntityTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            HookModel = ResourceModel.Get<IHookModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
        }

        public override async void Execute(CreateComponentCommandData parameter)
        {
            var dialog = new CreateEntityTypeDialog(parameter.Project, parameter.Namespace);
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
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
                        var className = dialog.ViewModel.ComponentName;
                        var @namespace = dialog.ViewModel.ComponentNamespace;
                        var parent = dialog.ViewModel.ParentComponent;
                        var updatable = dialog.ViewModel.ComponentUpdatable;
                        var drawable = dialog.ViewModel.ComponentDrawable;
                        var component = TypeDCoreProjectModel.CreateEntity(
                            parameter.Project, className, @namespace, parent, updatable, drawable);
                        CommandHistoryModel.Record(
                            "Create entity type",
                            $"Created {component.FullName} based on {parent?.FullName ?? "an unspecified base type"}.",
                            () => ComponentProvider.Delete(parameter.Project, component),
                            () => component = TypeDCoreProjectModel.CreateEntity(
                                parameter.Project, className, @namespace, parent, updatable, drawable));
                    }
                }
            }
        }
    }
}
