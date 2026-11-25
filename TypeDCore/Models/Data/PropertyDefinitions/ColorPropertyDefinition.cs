using System;
using System.Text.Json.Serialization;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    /// <summary>
    /// Exposes a color property value and its Avalonia color representation to the property editor.
    /// </summary>
    public class ColorPropertyNode : PropertyNode
    {
        /// <summary>
        /// Creates a node for a color component property.
        /// </summary>
        public ColorPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed)
        {
            CopyCommand = new RelayCommand(() => _ = CopyAsync());
            PasteCommand = new RelayCommand(() => _ = PasteAsync());
        }

        /// <summary>Copies the color to the clipboard as #AARRGGBB.</summary>
        public System.Windows.Input.ICommand CopyCommand { get; }

        /// <summary>Pastes a color from the clipboard into this property.</summary>
        public System.Windows.Input.ICommand PasteCommand { get; }

        private static Avalonia.Input.Platform.IClipboard GetClipboard()
        {
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            return Avalonia.Controls.TopLevel.GetTopLevel(lifetime?.MainWindow)?.Clipboard;
        }

        private async System.Threading.Tasks.Task CopyAsync()
        {
            var clipboard = GetClipboard();
            if (clipboard != null)
                await Avalonia.Input.Platform.ClipboardExtensions.SetTextAsync(clipboard, AvaloniaColor.ToString());
        }

        private async System.Threading.Tasks.Task PasteAsync()
        {
            var clipboard = GetClipboard();
            if (clipboard == null || ReadOnly)
                return;
            var text = (await Avalonia.Input.Platform.ClipboardExtensions.TryGetTextAsync(clipboard))?.Trim();
            if (text != null && Avalonia.Media.Color.TryParse(text, out var parsed))
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    AvaloniaColor = parsed;
                    OnPropertyChanged(nameof(AvaloniaColor));
                });
        }

        private sealed class RelayCommand : System.Windows.Input.ICommand
        {
            private readonly Action _action;
            public RelayCommand(Action action) { _action = action; }
            public event EventHandler CanExecuteChanged { add { } remove { } }
            public bool CanExecute(object parameter) => true;
            public void Execute(object parameter) => _action();
        }

        /// <summary>
        /// Refreshes the value and notifies bindings of the Avalonia color.
        /// </summary>
        public override void RefreshValue()
        {
            base.RefreshValue();
            OnPropertyChanged(nameof(AvaloniaColor));
        }

        /// <summary>
        /// Gets or sets the property value as an engine color.
        /// </summary>
        public override object Value
        {
            get
            {
                if (base.Value is Color colorValue)
                    return colorValue;
                return new Color(255, 0, 0, 0);
            }
            set
            {
                if (value is Color colorValue)
                    base.Value = colorValue;
            }
        }

        /// <summary>
        /// Gets or sets the value as an Avalonia color.
        /// </summary>
        public Avalonia.Media.Color AvaloniaColor
        {
            get
            {
                var color = (Color)Value;
                return Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            set
            {
                var color = new Color(value.A, value.R, value.G, value.B);
                if (!color.Equals((Color)Value))
                    Value = color;
            }
        }
    }

    /// <summary>
    /// Defines the editor and code representation for color properties.
    /// </summary>
    public class ColorPropertyDefinition : PropertyDefinition<Color, ColorPropertyNode>
    {
        private readonly JsonConverter _jsonConverter = new ColorJsonConverter();

        /// <summary>
        /// Gets the JSON converter used to serialize color values.
        /// </summary>
        public override JsonConverter JsonConverter => _jsonConverter;

        /// <summary>
        /// Gets the color property editor template URI.
        /// </summary>
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/ColorPropertyTemplate.axaml";
        /// <summary>
        /// Gets the color property editor template key.
        /// </summary>
        protected override string TemplateKey => "ColorPropertyTemplate";

        /// <summary>
        /// Converts a color to a C# expression containing its channels.
        /// </summary>
        public override string ToCode(Color value)
        {
            return $"new {typeof(Color).FullName}({value.A}, {value.R}, {value.G}, {value.B})";
        }
    }
}
