using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Models.Data.PropertyDefinitions;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Models
{
    /// <summary>
    /// A content property, on a node of a component, that points at a content file.
    /// </summary>
    internal record ContentReference(Component Root, Component Node, Property Property, string NodePath);

    /// <summary>
    /// Finds and updates the content properties in all components that reference a content file.
    /// </summary>
    internal class ContentReferenceUpdater
    {
        private readonly IResourceModel Resources;

        public ContentReferenceUpdater(IResourceModel resources)
        {
            Resources = resources;
        }

        public static string ToRelativePath(Project project, string absolutePath)
        {
            return Path.GetRelativePath(project.ProjectContentPath, absolutePath).Replace('\\', '/');
        }

        public List<ContentReference> Find(Project project, string relativePath)
        {
            var propertyModel = Resources.Get<IPropertyModel>();
            var references = new List<ContentReference>();
            foreach (var root in ListRoots(project))
                Collect(root, root, root.Name, relativePath, propertyModel, references);
            return references;
        }

        public void Update(Project project, IEnumerable<ContentReference> references, string newRelativePath)
        {
            var hooks = Resources.Get<IHookModel>();
            var saveModel = Resources.Get<ISaveModel>();
            var projectModel = Resources.Get<IProjectModel>();
            var opened = Resources.Get<INodeSelectionModel>().OpenedComponent;

            foreach (var reference in references)
            {
                reference.Property.Value = newRelativePath;

                // Only the opened component is live in the viewer and properties panel.
                if (ReferenceEquals(reference.Root, opened))
                    hooks.Shoot(new PropertyChangedHook { Component = reference.Node, Property = reference.Property });

                var context = saveModel.GetSaveContext<ComponentSaveContext>(project);
                if (!context.Components.Contains(reference.Root))
                {
                    context.Components.RemoveAll(c => c.FullName == reference.Root.FullName);
                    context.Components.Add(reference.Root);
                    saveModel.AddSave<ComponentSaveContext>();
                    projectModel.SaveCode(reference.Root.Template.Code);
                }
            }
        }

        // Prefers the in-memory instances so unsaved edits are not lost.
        private List<Component> ListRoots(Project project)
        {
            var opened = Resources.Get<INodeSelectionModel>().OpenedComponent;
            var context = Resources.Get<ISaveModel>().GetSaveContext<ComponentSaveContext>(project);

            return Resources.Get<IComponentProvider>().ListAll(project)
                .Select(c => ReferenceEquals(opened, null) || opened.FullName != c.FullName
                    ? context.Components.Find(x => x.FullName == c.FullName) ?? c
                    : opened)
                .ToList();
        }

        private static void Collect(Component root, Component node, string path, string relativePath, IPropertyModel propertyModel, List<ContentReference> references)
        {
            foreach (var property in node.Properties)
            {
                if (property.Value is not string value
                    || propertyModel.Get(property.Type) is not ContentPropertyDefinition
                    || !string.Equals(value.Replace('\\', '/'), relativePath, StringComparison.OrdinalIgnoreCase))
                    continue;
                references.Add(new ContentReference(root, node, property, $"{path} ({property.Name})"));
            }

            foreach (var child in node.Children)
                Collect(root, child, $"{path}/{child.Name}", relativePath, propertyModel, references);
        }
    }
}
