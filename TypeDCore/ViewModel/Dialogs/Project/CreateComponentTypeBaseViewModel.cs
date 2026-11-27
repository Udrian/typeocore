using Avalonia.Controls;
using Avalonia.Platform.Storage;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeD.Models;
using TypeD.Helpers;
using TypeD.Models.Interfaces;

namespace TypeDCore.ViewModel.Dialogs.Project
{
    internal class CreateComponentTypeBaseViewModel : ViewModelBase
    {
        // Data
        protected TypeD.Models.Data.Project Project { get; }
        private string componentName;
        private string componentNamespace;

        // Properties
        public string ComponentName { get => componentName; set => SetProperty(ref componentName, value); }
        public string ComponentNamespace
        {
            get => componentNamespace;
            set
            {
                if (SetProperty(ref componentNamespace, value) && NamespaceSelectionError != null)
                {
                    NamespaceSelectionError = null;
                    OnPropertyChanged(nameof(NamespaceSelectionError));
                }
            }
        }
        public string NamespaceSelectionError { get; private set; }

        public string ParentComponentFullName { get; set; }
        public Component ParentComponent { get; set; }
        public string ComponentBaseType { get; set; }

        /// <summary>
        /// The icon explicitly chosen for the new component, or <see langword="null"/> to inherit the one of the parent component.
        /// </summary>
        public string IconName { get; protected set; }
        public Avalonia.Media.Imaging.Bitmap IconPreview
        {
            get { return IconCache.Get(Project, IconName ?? ParentComponent?.ResolvedIcon); }
        }
        public string IconText
        {
            get { return IconName ?? $"{ParentComponent?.ResolvedIcon ?? Component.DefaultIcon} (inherited)"; }
        }

        // Constructors
        public CreateComponentTypeBaseViewModel(Control element, TypeD.Models.Data.Project project, string @namespace, Component componentBaseType) : base(element)
        {
            Project = project;

            ComponentBaseType = componentBaseType.FullName;
            ComponentNamespace = @namespace;
            ParentComponentFullName = ComponentBaseType;
            ParentComponent = componentBaseType;
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
                //MessageBox.Show($"Invalid name '{ComponentName}'");
                return false;
            }

            if (File.Exists(@$"{Project.ProjectTypeOPath}\components\{Project.ProjectName}\{ComponentNamespace.Replace(".", "\\")}\{ComponentName}.component"))
            {
                //MessageBox.Show($"'{ComponentNamespace}.{ComponentName}' already exists");
                return false;
            }

            return true;
        }

        public async void OpenNamespace()
        {
            var files = await MainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Location Folder",
                SuggestedStartLocation = await MainWindow.StorageProvider.TryGetFolderFromPathAsync(Project.ProjectSourcePath),
                AllowMultiple = false

            });

            if (files.Count >= 1)
            {
                if (!TryGetNamespace(Project.ProjectSourcePath, files[0].Path.LocalPath, Project.ProjectName, out var selectedNamespace))
                {
                    NamespaceSelectionError = "Choose a folder inside the project source folder.";
                }
                else
                {
                    NamespaceSelectionError = null;
                    ComponentNamespace = selectedNamespace;
                }
                OnPropertyChanged(nameof(NamespaceSelectionError));
            }
        }

        internal static bool TryGetNamespace(string sourceFolder, string selectedFolder, string projectName, out string componentNamespace)
        {
            var relative = Path.GetRelativePath(sourceFolder, selectedFolder);
            if (Path.IsPathRooted(relative) || relative == ".." ||
                relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            {
                componentNamespace = null;
                return false;
            }

            componentNamespace = relative == "." ? projectName
                : projectName + "." + relative.Replace(Path.DirectorySeparatorChar, '.').Replace(Path.AltDirectorySeparatorChar, '.');
            return true;
        }

        public async Task OpenComponents()
        {
            var baseType = ComponentBaseType;
            var filter = new FilterHelper { Filters = baseType, ExactMatch = true };
            var component = await ResourceModel.Get<IEditorDialogModel>().SelectComponentAsync(Project, filter);
            if (component != null && ComponentBaseType == baseType && component.RootBaseComponent.FullName == baseType)
            {
                OnParentComponentSet(component);
                OnPropertyChanged(nameof(ParentComponentFullName));
            }
        }

        public virtual void OnParentComponentSet(Component parentComponent)
        {
            ParentComponent = parentComponent;
            ParentComponentFullName = ParentComponent.FullName;
            OnPropertyChanged(nameof(ParentComponent));
            OnPropertyChanged(nameof(ParentComponentFullName));
            OnPropertyChanged(nameof(IconPreview));
            OnPropertyChanged(nameof(IconText));
        }

        public async Task OpenIcons()
        {
            var icon = await ResourceModel.Get<IEditorDialogModel>().SelectIconAsync(Project, IconName ?? ParentComponent?.ResolvedIcon);
            if (icon != null)
            {
                IconName = icon;
                OnPropertyChanged(nameof(IconPreview));
                OnPropertyChanged(nameof(IconText));
            }
        }

        public void InheritIcon()
        {
            IconName = null;
            OnPropertyChanged(nameof(IconPreview));
            OnPropertyChanged(nameof(IconText));
        }
    }
}
