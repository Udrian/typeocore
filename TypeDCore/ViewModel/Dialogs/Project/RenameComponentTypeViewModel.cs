using System.Windows;
using TypeD.Models.Data;
using TypeD.ViewModel;

namespace TypeDCore.ViewModel.Dialogs.Project
{
    internal class RenameComponentTypeViewModel : ViewModelBase
    {
        // Data
        public string OldName { get; set; }
        public string Name { get; set; }

        // Constructors
        public RenameComponentTypeViewModel(FrameworkElement element, Component component) : base(element)
        {
            OldName = Name = component.ClassName;
        }
    }
}
