using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Models;

namespace TypeDCore.View.Dialogs.Project
{
    /// <summary>
    /// Lets the user pick one of the icons of the project, or import a new one from disk.
    /// </summary>
    public partial class IconSelectorDialog : Window
    {
        public class IconItem
        {
            public string Name { get; init; }
            public Bitmap Image { get; init; }
        }

        public string SelectedIcon { get; private set; }

        private readonly TypeD.Models.Data.Project Project;
        private readonly IIconModel IconModel;
        private string SelectName;

        internal IconSelectorDialog(TypeD.Models.Data.Project project, string currentIcon)
        {
            InitializeComponent();
            Project = project;
            IconModel = ViewModelBase.ResourceModel.Get<IIconModel>();
            SelectName = currentIcon;
            Refresh();
        }

        private void Refresh()
        {
            var filter = tbFilter.Text ?? string.Empty;
            var items = IconModel.List(Project)
                .Where(name => name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .Select(name => new IconItem { Name = name, Image = IconCache.Get(Project, name) })
                .ToList();
            lbIcons.ItemsSource = items;
            lbIcons.SelectedItem = items.FirstOrDefault(item => string.Equals(item.Name, SelectName, StringComparison.OrdinalIgnoreCase));
        }

        private void Accept()
        {
            if (lbIcons.SelectedItem is not IconItem selected)
                return;
            SelectedIcon = selected.Name;
            Close(true);
        }

        private async void Import_Click(object sender, RoutedEventArgs e)
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Import icon",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.webp" } }
                }
            });
            if (files.Count == 0)
                return;

            try
            {
                SelectName = IconModel.Import(Project, files[0].Path.LocalPath);
                tbFilter.Text = string.Empty;
                Refresh();
            }
            catch (Exception ex)
            {
                ViewModelBase.ResourceModel.Get<ILogModel>()?.Log($"Could not import icon: {ex.Message}");
            }
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Project != null)
                Refresh();
        }

        private void Ok_Click(object sender, RoutedEventArgs e) => Accept();

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close(false);

        private void Icons_DoubleTapped(object sender, TappedEventArgs e) => Accept();

        private void Icons_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;
            Accept();
            e.Handled = true;
        }
    }
}
