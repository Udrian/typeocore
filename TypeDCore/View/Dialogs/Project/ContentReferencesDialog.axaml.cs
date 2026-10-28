using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace TypeDCore.View.Dialogs.Project
{
    public enum ContentReferencesResult
    {
        Cancel,
        Ignore,
        Confirm
    }

    /// <summary>
    /// Lists the components and node paths that reference a content file about to be renamed or deleted.
    /// </summary>
    public partial class ContentReferencesDialog : Window
    {
        public ContentReferencesResult Result { get; private set; } = ContentReferencesResult.Cancel;

        public ContentReferencesDialog(string message, string confirmText, IEnumerable<string> references)
        {
            InitializeComponent();
            tbMessage.Text = message;
            btnConfirm.Content = confirmText;
            lbReferences.ItemsSource = new List<string>(references);
            KeyDown += (_, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }

        private void Confirm_Click(object sender, RoutedEventArgs e) => Finish(ContentReferencesResult.Confirm);
        private void Ignore_Click(object sender, RoutedEventArgs e) => Finish(ContentReferencesResult.Ignore);
        private void Cancel_Click(object sender, RoutedEventArgs e) => Finish(ContentReferencesResult.Cancel);

        private void Finish(ContentReferencesResult result)
        {
            Result = result;
            Close();
        }
    }
}
