using TypeDCore.Commands.Data;
using TypeD.Models.Providers.Interfaces;
using TypeD.Commands;
using System.Windows;
using TypeD.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class DeleteComponentTypeCommand : CustomCommand<ComponentCommandData>
    {
        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Constructors
        public DeleteComponentTypeCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
        }

        public override void Execute(ComponentCommandData parameter)
        {
            var result = MessageBox.Show($"Do you want to delete '{parameter.Component.FullName}'?", "Deleting", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                ComponentProvider.Delete(parameter.Project, parameter.Component);
            }
        }
    }
}
