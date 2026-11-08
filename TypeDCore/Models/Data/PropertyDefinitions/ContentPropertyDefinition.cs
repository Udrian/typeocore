using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TypeD.Models.Interfaces;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeOEngine.Typedeaf.Core.Engine.Contents;
using EngineContent = TypeOEngine.Typedeaf.Core.Engine.Contents.Content;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    /// <summary>
    /// Exposes a content path and browse and clear commands to the property editor.
    /// </summary>
    public class ContentPropertyNode : PropertyNode
    {
        /// <summary>
        /// Creates a node for a content component property.
        /// </summary>
        public ContentPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed)
        {
            BrowseCommand = new AsyncRelayCommand(BrowseAsync);
            ClearCommand = new RelayCommand(() => Value = null);
        }

        /// <summary>
        /// Gets the command that opens the content picker.
        /// </summary>
        public ICommand BrowseCommand { get; }
        /// <summary>
        /// Gets the command that clears the content path.
        /// </summary>
        public ICommand ClearCommand { get; }

        /// <summary>
        /// Gets or sets the content path, using an empty string when no path is set.
        /// </summary>
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
            var path = await ResourceModel.Get<IEditorDialogModel>().SelectContentAsync(project, extensions);
            if (path != null)
                Value = path;
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

        /// <summary>
        /// Creates a definition that uses the supplied content associations.
        /// </summary>
        public ContentPropertyDefinition(IContentProvider contentProvider)
        {
            ContentProvider = contentProvider;
        }

        /// <summary>
        /// Gets the engine content base type handled by this definition.
        /// </summary>
        public override Type Type => typeof(EngineContent);

        /// <summary>
        /// Gets the content property editor template URI.
        /// </summary>
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/ContentPropertyTemplate.axaml";
        /// <summary>
        /// Gets the content property editor template key.
        /// </summary>
        protected override string TemplateKey => "ContentPropertyTemplate";

        /// <summary>
        /// Converts a content path to a C# content-loading expression.
        /// </summary>
        public override string ToCode(string value)
        {
            var association = GetAssociation(value);
            if (association == null)
                return "null";

            var path = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"ContentLoader.LoadContent<{association.EngineImplementationType.FullName.Replace('+', '.')}>(\"{path}\")";
        }

        /// <summary>
        /// Finds the content association for a path's file extension.
        /// </summary>
        public ContentTypeAssociation GetAssociation(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            return ContentProvider.GetAssociation(Path.GetExtension(path).TrimStart('.').ToLowerInvariant());
        }
    }
}
