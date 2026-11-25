using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data
{
    /// <summary>
    /// Copies and pastes colors through the system clipboard as #AARRGGBB text.
    /// </summary>
    public static class ColorClipboard
    {
        private static IClipboard GetClipboard()
        {
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            return TopLevel.GetTopLevel(lifetime?.MainWindow)?.Clipboard;
        }

        /// <summary>Copies a color to the clipboard.</summary>
        public static async Task CopyAsync(Avalonia.Media.Color color)
        {
            var clipboard = GetClipboard();
            if (clipboard != null)
                await ClipboardExtensions.SetTextAsync(clipboard, color.ToString());
        }

        /// <summary>Reads a color from the clipboard, or <c>null</c> when it does not hold one.</summary>
        public static async Task<Avalonia.Media.Color?> PasteAsync()
        {
            var clipboard = GetClipboard();
            if (clipboard == null)
                return null;
            var text = (await ClipboardExtensions.TryGetTextAsync(clipboard))?.Trim();
            return text != null && Avalonia.Media.Color.TryParse(text, out var color) ? color : null;
        }

        /// <summary>Adds copy and paste (context menu and Ctrl+C/Ctrl+V) to a color picker.</summary>
        public static void Attach(ColorPicker picker)
        {
            async Task Paste()
            {
                var color = await PasteAsync();
                if (color.HasValue)
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => picker.Color = color.Value);
            }

            var copy = new MenuItem { Header = "Copy color", InputGesture = Avalonia.Input.KeyGesture.Parse("Ctrl+C") };
            copy.Click += (_, _) => _ = CopyAsync(picker.Color);
            var paste = new MenuItem { Header = "Paste color", InputGesture = Avalonia.Input.KeyGesture.Parse("Ctrl+V") };
            paste.Click += (_, _) => _ = Paste();
            picker.ContextMenu = new ContextMenu { Items = { copy, paste } };
            picker.KeyDown += (_, e) =>
            {
                if (e.KeyModifiers != Avalonia.Input.KeyModifiers.Control)
                    return;
                if (e.Key == Avalonia.Input.Key.C)
                    _ = CopyAsync(picker.Color);
                else if (e.Key == Avalonia.Input.Key.V)
                    _ = Paste();
                else
                    return;
                e.Handled = true;
            };
        }
    }
}
