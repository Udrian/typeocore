using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;

namespace TypeDCore.ViewModel.Dialogs.Project
{
    internal class ComponentSelectorViewModel : ViewModelBase
    {
        // Data
        TypeD.Models.Data.Project Project { get; set; }

        // Models
        IComponentModel ComponentModel { get; set; }

        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Properties
        public List<Component> AllComponents { get; set; }
        public ObservableCollection<Component> FilteredComponents { get; set; }
        public Component SelectedComponent { get; set; }
        public FilterHelper TypeFilter { get; set; }
        public FilterHelper NameFilter { get; set; }

        // Constructors
        public ComponentSelectorViewModel(FrameworkElement element, TypeD.Models.Data.Project project) : base(element)
        {
            TypeFilter = new FilterHelper();
            NameFilter = new FilterHelper();

            Project = project;

            ComponentModel = ResourceModel.Get<IComponentModel>();

            ComponentProvider = ResourceModel.Get<IComponentProvider>();

            AllComponents = ComponentProvider.ListAll(Project);
            var componentBaseTypes = ComponentProvider.GetBaseTypeComponents();
            AllComponents.AddRange(componentBaseTypes);
            FilteredComponents = new ObservableCollection<Component>(AllComponents);
        }

        // Functions
        public void UpdateFilter()
        {
            FilteredComponents.Clear();

            foreach (var component in AllComponents)
            {
                if (TypeFilter.Filter(ComponentModel.GetBaseType(component).FullName))
                {
                    continue;
                }
                if(NameFilter.Filter(component.FullName))
                {
                    continue;
                }

                FilteredComponents.Add(component);
            }
        }
    }
}
