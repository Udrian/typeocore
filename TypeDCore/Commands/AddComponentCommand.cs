using System;
using System.Collections.Generic;
using System.Linq;
using TypeD.Commands;
using TypeD.ViewModel;
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
        ICommandHistoryModel CommandHistoryModel { get; set; }

        // Constructors
        public AddComponentCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentModel = ResourceModel.Get<IComponentModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
        }

        public override async void Execute(AddComponentCommandData parameter)
        {
            var dialog = new ComponentSelectorDialog(parameter.Project);

            var excludeComponent = parameter.ToComponent;
            while(excludeComponent.ParentComponent != null)
            {
                excludeComponent = excludeComponent.ParentComponent;
            }

            dialog.ViewModel.NameFilter.Exclude = $"{excludeComponent.FullName};";
            dialog.ViewModel.TypeFilter.Exclude = $"{typeof(Game).FullName};{typeof(TypeOEngine.Typedeaf.Core.Scene).FullName};";

            parameter.ToComponent.Template.ChildrenFilter(dialog.ViewModel.TypeFilter);

            dialog.ViewModel.UpdateFilter();
            if(await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) == true)
            {
                var previousChildren = new HashSet<TypeD.Models.Data.Component>(parameter.ToComponent.Children);
                ComponentModel.Add(parameter.Project, parameter.ToComponent, dialog.ViewModel.SelectedComponent);
                var addedComponent = parameter.ToComponent.Children.LastOrDefault(child => !previousChildren.Contains(child));
                if (addedComponent == null)
                    throw new InvalidOperationException("The selected component was not added to its parent.");

                CommandHistoryModel.Record(
                    "Add component",
                    $"Added {addedComponent.FullName} to {parameter.ToComponent.FullName}.",
                    () => ComponentModel.Remove(parameter.Project, addedComponent),
                    () => ComponentModel.AddExisting(parameter.Project, parameter.ToComponent, addedComponent));
            }
        }
    }
}
