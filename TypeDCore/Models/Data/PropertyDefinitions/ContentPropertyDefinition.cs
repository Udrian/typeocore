using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TypeDCore.View.Dialogs.Project;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using EngineContent = TypeOEngine.Typedeaf.Core.Engine.Contents.Content;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class ContentPropertyNode : PropertyNode
    {
        public ContentPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed)
        {
            BrowseCommand = new AsyncRelayCommand(BrowseAsync);
            ClearCommand = new RelayCommand(() => Value = null);
        }

        public ICommand BrowseCommand { get; }
        public ICommand ClearCommand { get; }

        public override object Value
        {
            get => base.Value ?? string.Empty;
            set
            {
                var path = string.IsNullOrEmpty(value as string) ? null : (string)value;
                base.Value = path;
            }
        }

        private async Task BrowseAsync()
        {
            if (ReadOnly)
                return;

            var project = ResourceModel.Get<INodeSelectionModel>()?.Project;
            var contentProvider = ResourceModel.Get<IContentProvider>();
            if (project == null || contentProvider == null)
                return;

            var extensions = contentProvider.GetAssociationsForEngineType(Property.Type).Select(a => a.Extension);
            var dialog = new ContentSelectorDialog(project, extensions);
            await dialog.ShowDialog(MainWindow);
            if (dialog.SelectedPath != null)
                Value = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// Handles every engine content type (Texture, Font, ...), registered under the engine's Content base type. The
    /// value is stored as a path relative to the project's content folder, and loaded through the ContentLoader using
    /// the engine implementation associated with the file extension.
    /// </summary>
    public class ContentPropertyDefinition : PropertyDefinition<string, ContentPropertyNode>
    {
        private readonly IContentProvider ContentProvider;

        public ContentPropertyDefinition(IContentProvider contentProvider)
        {
            ContentProvider = contentProvider;
        }

        public override Type Type => typeof(EngineContent);

        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/ContentPropertyTemplate.axaml";
        protected override string TemplateKey => "ContentPropertyTemplate";

        public override string ToCode(string value)
        {
            var association = GetAssociation(value);
            if (association == null)
                return "null";

            var path = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"ContentLoader.LoadContent<{association.EngineImplementationType.FullName.Replace('+', '.')}>(\"{path}\")";
        }

        public ContentTypeAssociation GetAssociation(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            return ContentProvider.GetAssociation(Path.GetExtension(path).TrimStart('.').ToLowerInvariant());
        }
    }
}
