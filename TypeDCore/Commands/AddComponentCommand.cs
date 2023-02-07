using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;
using TypeOEngine.Typedeaf.Core;

namespace TypeDCore.Commands
{
    internal class AddComponentCommand : CustomCommand<AddComponentCommandData>
    {
        // Models
        IComponentModel ComponentModel { get; set; }

        // Constructors
        public AddComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
        }

        public override void Execute(AddComponentCommandData parameter)
        {
            var dialog = new ComponentSelectorDialog(parameter.Project);
            dialog.ViewModel.NameFilter.Exclude = $"{parameter.ToComponent.FullName};";
            dialog.ViewModel.TypeFilter.Exclude = $"{typeof(Game).FullName};{typeof(TypeOEngine.Typedeaf.Core.Scene).FullName};";

            parameter.ToComponent.Template.ChildrenFilter(dialog.ViewModel.TypeFilter);

            dialog.ViewModel.UpdateFilter();
            if(dialog.ShowDialog() == true)
            {
                ComponentModel.Add(parameter.Project, parameter.ToComponent, dialog.ViewModel.SelectedComponent);
            }
        }
    }
}
