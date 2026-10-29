using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    /// <summary>
    /// Lets content dragged from the content panel be dropped on a content property editor.
    /// </summary>
    public class ContentDropBehavior : AvaloniaObject
    {
        public static readonly AttachedProperty<bool> EnabledProperty =
            AvaloniaProperty.RegisterAttached<ContentDropBehavior, Control, bool>("Enabled");

        public static bool GetEnabled(Control control) => control.GetValue(EnabledProperty);
        public static void SetEnabled(Control control, bool value) => control.SetValue(EnabledProperty, value);

        static ContentDropBehavior()
        {
            EnabledProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                if (e.NewValue is not true)
                    return;

                DragDrop.SetAllowDrop(control, true);
                control.AddHandler(DragDrop.DragOverEvent, DragOver);
                control.AddHandler(DragDrop.DropEvent, Drop);
            });
        }

        private static string GetAcceptedPath(object sender, out ContentPropertyNode node)
        {
            node = (sender as Control)?.DataContext as ContentPropertyNode;
            var path = ViewerDragPayload.ContentPath;
            if (node == null || node.ReadOnly || path == null)
                return null;

            var project = ViewModelBase.ResourceModel.Get<INodeSelectionModel>()?.Project;
            if (project == null)
                return null;

            var relative = ViewerDropHandler.GetRelativePath(project, path);
            if (relative == null)
                return null;

            var extension = System.IO.Path.GetExtension(relative).TrimStart('.');
            var accepted = ViewModelBase.ResourceModel.Get<IContentProvider>()
                .GetAssociationsForEngineType(node.Property.Type)
                .Any(a => string.Equals(a.Extension, extension, System.StringComparison.OrdinalIgnoreCase));
            return accepted ? relative : null;
        }

        private static void DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects = GetAcceptedPath(sender, out _) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private static void Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            var path = GetAcceptedPath(sender, out var node);
            if (path != null)
                node.Value = path;
        }
    }
}
