using TypeDCore.Commands.Data;
using TypeD.Models.Providers.Interfaces;
using TypeD.Commands;
using TypeD.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class DeleteComponentTypeCommand : CustomCommand<ComponentCommandData>
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }

        // Constructors
        public DeleteComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
        }

        public override void Execute(ComponentCommandData parameter)
        {
            //var result = MessageBox.Show($"Do you want to delete '{parameter.Component.FullName}'?", "Deleting", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            //if (result == MessageBoxResult.Yes)
            {
                var componentName = parameter.Component.FullName;
                var component = parameter.Component;
                ComponentProvider.Delete(parameter.Project, component);
                CommandHistoryModel.Record(
                    "Delete component type",
                    $"Deleted {componentName}.",
                    () => ComponentProvider.Restore(parameter.Project, component),
                    () => ComponentProvider.Delete(
                        parameter.Project,
                        ComponentProvider.Load(parameter.Project, componentName) ?? component));
            }
        }
    }
}
