using Avalonia.Controls;

namespace TypeDCore.View.Dialogs.Project
{
    internal partial class PropertyReferenceScanDialog : Window
    {
        private bool Finished;

        public PropertyReferenceScanDialog() => InitializeComponent();

        public void Complete()
        {
            Finished = true;
            Close();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            if (!Finished)
                e.Cancel = true;
        }
    }
}
