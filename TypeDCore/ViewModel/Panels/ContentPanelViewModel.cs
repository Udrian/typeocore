using Avalonia.Controls;
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

            public Content Content { get; private set; }

            public void SetContent(Content content)
            {
                Content = content;
                Title = content.Name;
                Path = content.Path;
                Thumbnail = content.Thumbnail;
            }
        }

        public List<ImageItem> Items { get; }

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
