using Avalonia.Controls;
using TypeDCore.Models;
using TypeDCore.View.Dialogs.Project;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TypeD.Models.Interfaces;
using TypeDCore.Models.Interfaces;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;

namespace TypeDCore.ViewModel.Panels
{
    internal partial class ContentPanelViewModel : ViewModelBase
    {
        public partial class ImageItem : ViewModelBase
        {
            [ObservableProperty]
            private string _id;
            [ObservableProperty]
            private string _title;
            [ObservableProperty]
            private string _path;
            [ObservableProperty]
            private Bitmap _thumbnail;
            [ObservableProperty]
            private string _editName;
            [ObservableProperty]
            private bool _isEditing;

            public Content Content { get; private set; }

            public void SetContent(Content content)
            {
                Content = content;
                Title = content.Name;
                Path = content.Path;
                Thumbnail = content.Thumbnail;
            }

            public void BeginRename()
            {
                EditName = Title;
                IsEditing = true;
            }

            public void CancelRename()
            {
                IsEditing = false;
            }
        }

        public List<ImageItem> Items { get; }

        [ObservableProperty]
        private ImageItem _selectedItem;

        public void SetNoTreeActive()
        {
            ResourceModel.Get<INodeSelectionModel>().SetNoTreeActive();
        }

        public async Task<bool> CommitRename(ImageItem item)
        {
            var newName = item.EditName?.Trim();
            item.CancelRename();
            if (string.IsNullOrEmpty(newName) || newName == item.Title)
                return false;

            var extension = System.IO.Path.GetExtension(item.Path);
            var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(item.Path), newName + extension);
            if (newName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0 || System.IO.File.Exists(newPath))
            {
                ResourceModel.Get<ILogModel>().Log($"Could not rename content to '{newName}': invalid name or the file already exists.");
                return false;
            }

            var oldRelative = ContentReferenceUpdater.ToRelativePath(Project, item.Path);
            var newRelative = ContentReferenceUpdater.ToRelativePath(Project, newPath);
            var updater = new ContentReferenceUpdater(ResourceModel);
            var references = updater.Find(Project, oldRelative);
            if (references.Count > 0)
            {
                var result = await AskAboutReferences(
                    $"'{oldRelative}' is used by the following nodes. Confirm updates them to '{newRelative}', Ignore renames the file only.",
                    "Confirm", references);
                if (result == ContentReferencesResult.Cancel)
                    return false;
                if (result == ContentReferencesResult.Ignore)
                    references.Clear();
            }

            try
            {
                System.IO.File.Move(item.Path, newPath);
            }
            catch (Exception ex)
            {
                ResourceModel.Get<ILogModel>().Log($"Could not rename content '{item.Title}': {ex}");
                return false;
            }

            if (references.Count > 0)
                updater.Update(Project, references, newRelative);

            item.Content.Name = newName;
            item.Content.Path = newPath;
            item.SetContent(item.Content);
            UpdateFilter();
            return true;
        }

        public async Task<bool> Delete(ImageItem item)
        {
            var relative = ContentReferenceUpdater.ToRelativePath(Project, item.Path);
            var updater = new ContentReferenceUpdater(ResourceModel);
            var references = updater.Find(Project, relative);

            if (references.Count > 0)
            {
                var result = await AskAboutReferences(
                    $"'{relative}' is used by the following nodes. Confirm clears the content on them, Ignore deletes the file only.",
                    "Confirm", references);
                if (result == ContentReferencesResult.Cancel)
                    return false;
                if (result == ContentReferencesResult.Ignore)
                    references.Clear();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Delete Content", $"Delete '{System.IO.Path.GetFileName(item.Path)}'? This can not be undone.", ButtonEnum.YesNo, Icon.Warning);
                if (await box.ShowWindowDialogAsync(MainWindow) != ButtonResult.Yes)
                    return false;
            }

            try
            {
                System.IO.File.Delete(item.Path);
            }
            catch (Exception ex)
            {
                ResourceModel.Get<ILogModel>().Log($"Could not delete content '{item.Title}': {ex}");
                return false;
            }

            if (references.Count > 0)
                updater.Update(Project, references, null);

            Items.Remove(item);
            UpdateFilter();
            return true;
        }

        private async Task<ContentReferencesResult> AskAboutReferences(string message, string confirmText, List<ContentReference> references)
        {
            var dialog = new ContentReferencesDialog(message, confirmText, references.Select(r => $"{r.Root.FullName}: {r.NodePath}"));
            await dialog.ShowDialog(MainWindow);
            return dialog.Result;
        }

        public void ShowInExplorer(ImageItem item)
        {
            if (System.IO.File.Exists(item.Path))
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{item.Path}\"");
        }

        public ObservableCollection<ImageItem> FilteredItems { get; }

        string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();
                UpdateFilter();
            }
        }

        public ICommand ClearSearchCommand { get; }

        private Project Project { get; set; }
        // Providers
        private IContentProvider ContentProvider { get; set; }

        // Constructors
        public ContentPanelViewModel(Control element, Project project) : base(element)
        {
            Project = project;
            Items = new List<ImageItem>();
            FilteredItems = new ObservableCollection<ImageItem>();
            ClearSearchCommand = new RelayCommand(() => { SearchText = string.Empty; });

            ContentProvider = ResourceModel.Get<IContentProvider>();
        }

        public async Task LoadAllContent()
        {
            int i = 0;
            foreach (var filePath in ContentProvider.ListAllPaths(Project))
            {
                var content = await ContentProvider.GetContentAsync(filePath);
                var item = new ImageItem() { Id = i.ToString() };
                item.SetContent(content);
                Add(item);
                i++;
            }
        }

        void UpdateFilter()
        {
            FilteredItems.Clear();
            var q = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();
            var filteredItems = string.IsNullOrEmpty(q)
                ? Items.ToList()
                : Items.Where(i => (i.Title ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            foreach(var item in filteredItems)
            {
                FilteredItems.Add(item);
            }
        }

        public void Add(ImageItem item)
        {
            Items.Add(item);
            UpdateFilter();
        }
    }
}
