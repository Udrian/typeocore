using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Options = ListOptions(property.Type);
        }

        /// <summary>
        /// Content paths, relative to the project's content folder, that can be assigned to the property. The first entry is empty and clears the value.
        /// </summary>
        public List<string> Options { get; }

        public override object Value
        {
            get => base.Value ?? string.Empty;
            set
            {
                var path = string.IsNullOrEmpty(value as string) ? null : (string)value;
                base.Value = path;
            }
        }

        private static List<string> ListOptions(Type propertyType)
        {
            var options = new List<string> { string.Empty };

            var project = ResourceModel?.Get<INodeSelectionModel>()?.Project;
            var contentProvider = ResourceModel?.Get<IContentProvider>();
            if (project == null || contentProvider == null || !Directory.Exists(project.ProjectContentPath))
                return options;

            var extensions = contentProvider.GetAssociationsForEngineType(propertyType)
                .Select(a => a.Extension.ToLowerInvariant())
                .ToHashSet();

            options.AddRange(Directory.EnumerateFiles(project.ProjectContentPath, "*", SearchOption.AllDirectories)
                .Where(file => extensions.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant()))
                .Select(file => Path.GetRelativePath(project.ProjectContentPath, file).Replace('\\', '/'))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase));
            return options;
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
