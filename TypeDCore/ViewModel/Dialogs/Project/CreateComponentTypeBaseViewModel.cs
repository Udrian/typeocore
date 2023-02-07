using Ookii.Dialogs.Wpf;
using System.IO;
using System.Linq;
using System.Windows;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.ViewModel.Dialogs.Project
{
    internal class CreateComponentTypeBaseViewModel : ViewModelBase
    {
        // Data
        TypeD.Models.Data.Project Project { get; set; }

        // Properties
        public string ComponentName { get; set; }
        public string ComponentNamespace { get; set; }

        public string ParentComponentFullName { get; set; }
        public Component ParentComponent { get; set; }
        public string ComponentBaseType { get; set; }

        // Constructors
        public CreateComponentTypeBaseViewModel(TypeD.Models.Data.Project project, string @namespace, string componentBaseType)
        {
            Project = project;

            ComponentBaseType = componentBaseType;
            ComponentNamespace = @namespace;
            ParentComponentFullName = ComponentBaseType;
        }

        // Functions
        public virtual bool Validate()
        {
            bool isValid = !string.IsNullOrEmpty(ComponentName) &&
                            ComponentName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1 &&
                            !ComponentName.Contains(" ") &&
                            (char.IsLetter(ComponentName.FirstOrDefault()) || ComponentName.StartsWith("_"));
            if (!isValid)
            {
                MessageBox.Show($"Invalid name '{ComponentName}'");
                return false;
            }

            if (File.Exists(@$"{Project.ProjectTypeOPath}\components\{Project.ProjectName}\{ComponentNamespace.Replace(".", "\\")}\{ComponentName}.component"))
            {
                MessageBox.Show($"'{ComponentNamespace}.{ComponentName}' already exists");
                return false;
            }

            return true;
        }

        public void OpenNamespace()
        {
            var folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @$"{Project.ProjectSourcePath}\{ComponentNamespace.Replace(".", "\\")}";
            if (folderBrowserDialog.ShowDialog() == true)
            {
                ComponentNamespace = folderBrowserDialog.SelectedPath.Replace("\\", ".").Substring(@$"{Project.ProjectSourcePath}\".Length);
                OnPropertyChanged(nameof(ComponentNamespace));
            }
        }

        public void OpenComponents()
        {
            var dialog = new ComponentSelectorDialog(Project);
            dialog.ViewModel.TypeFilter.Filters = $"{ComponentBaseType};";
            dialog.ViewModel.UpdateFilter();

            if (dialog.ShowDialog() == true && dialog.ViewModel.SelectedComponent != null)
            {
                OnParentComponentSet(dialog.ViewModel.SelectedComponent);
                OnPropertyChanged(nameof(ParentComponentFullName));
            }
        }

        public virtual void OnParentComponentSet(Component parentComponent)
        {
            ParentComponent = parentComponent;
            ParentComponentFullName = ParentComponent.FullName;
        }
    }
}
