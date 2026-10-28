using TypeD.Commands;
using TypeD.ViewModel;
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
        ICommandHistoryModel CommandHistoryModel { get; set; }

        // Constructors
        public RenameComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
        }

        public override async void Execute(ComponentCommandData parameter)
        {
            var dialog = new RenameComponentTypeDialog(parameter.Component);
            if (await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                var previousName = parameter.Component.FullName;
                var newName = $"{parameter.Component.Namespace}.{dialog.ViewModel.Name}";
                ComponentProvider.Rename(parameter.Project, parameter.Component, dialog.ViewModel.Name);
                CommandHistoryModel.Record("Rename component type", $"Renamed {previousName} to {newName}.");
            }
        }
    }
}
