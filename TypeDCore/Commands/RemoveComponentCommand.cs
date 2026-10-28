using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Linq;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class RemoveComponentCommand : CustomCommand<RemoveComponentCommandData>
    {
        // Models
        IComponentModel ComponentModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }

        // Constructors
        public RemoveComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
        }

        public override async void Execute(RemoveComponentCommandData parameter)
        {
            string name = parameter.Component.Properties.FirstOrDefault(p => p.Name == "Name")?.Value as string;
            if (string.IsNullOrEmpty(name))
                name = parameter.Component.ClassName;
            var box = MessageBoxManager.GetMessageBoxStandard("Remove Component", $"Remove component '{name}'?", ButtonEnum.YesNoCancel, Icon.Question);
            var result = await box.ShowWindowDialogAsync(ViewModelBase.MainWindow);

            if (result != ButtonResult.Yes) return;

            var componentName = parameter.Component.FullName;
            var parent = parameter.Component.ParentComponent;
            ComponentModel.Remove(parameter.Project, parameter.Component);
            if (parent != null)
            {
                CommandHistoryModel.Record(
                    "Remove component",
                    $"Removed {componentName} from {parent.FullName}.",
                    () => ComponentModel.AddExisting(parameter.Project, parent, parameter.Component),
                    () => ComponentModel.Remove(parameter.Project, parameter.Component));
            }
        }
    }
}
