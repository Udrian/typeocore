using System.Windows;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class SetStartSceneCommand : CustomCommand<ComponentCommandData>
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }

        // Constructors
        public SetStartSceneCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
        }

        public override void Execute(ComponentCommandData parameter)
        {
            var result = MessageBox.Show($"Do you want to set '{parameter.Component.FullName}' as start scene?", "Set Start Scene", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                TypeDCoreProjectModel.SetStartScene(parameter.Project, parameter.Component);
            }
        }
    }
}
