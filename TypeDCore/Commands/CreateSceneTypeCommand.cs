using TypeD.ViewModel;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class CreateSceneTypeCommand : CustomCommand<CreateComponentCommandData>
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }

        // Constructors
        public CreateSceneTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
        }

        public override async void Execute(CreateComponentCommandData parameter)
        {
            var dialog = new CreateSceneTypeDialog(parameter.Project, parameter.Namespace);
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                TypeDCoreProjectModel.CreateScene(parameter.Project, dialog.ViewModel.ComponentName, dialog.ViewModel.ComponentNamespace, dialog.ViewModel.ParentComponent);
                CommandHistoryModel.Record(
                    "Create scene type",
                    $"Created {dialog.ViewModel.ComponentNamespace}.{dialog.ViewModel.ComponentName} based on {dialog.ViewModel.ParentComponent?.FullName ?? "an unspecified base type"}.");
            }
        }
    }
}
