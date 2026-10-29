using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.Hooks;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models
{
    /// <summary>
    /// Creates and updates nodes from components and content that are dropped in the viewer.
    /// </summary>
    internal class ViewerDropHandler
    {
        private readonly IResourceModel ResourceModel;

        public ViewerDropHandler(IResourceModel resourceModel)
        {
            ResourceModel = resourceModel;
        }

        public bool CanDrop(Project project, Component root)
        {
            if (project == null || root == null)
                return false;

            if (ViewerDragPayload.Component != null)
                return ViewerDragPayload.Component.TypeOBaseType != null;

            return GetContentAssociation(project) != null;
        }

        /// <summary>
        /// Performs the drop at a position in viewer coordinates. Returns whether anything changed.
        /// </summary>
        public bool Drop(Project project, Component root, double x, double y)
        {
            if (!CanDrop(project, root))
                return false;

            var hook = new ViewerHitTestHook { Project = project, X = x, Y = y };
            ResourceModel.Get<IHookModel>().Shoot(hook);
            var hit = ResolveVisible(hook.Result);

            if (ViewerDragPayload.Component != null)
                return DropComponent(project, root, hit, ViewerDragPayload.Component, x, y);

            return DropContent(project, root, hit, ViewerDragPayload.ContentPath, x, y);
        }

        internal static readonly Dictionary<Component, (double X, double Y)> PendingCenters = new();

        /// <summary>
        /// Positions the instance so its center ends up at the drop point. Returns the center relative to the parent.
        /// </summary>
        private (double X, double Y)? Center(Project project, Component parent, Component instance, double x, double y)
        {
            var positionProperty = instance.Properties.FirstOrDefault(p => p.Name == "Position" && p.Type == typeof(Vec2));
            if (positionProperty == null || positionProperty.ReadOnly)
                return null;

            var origin = new ViewerHitTestHook { Project = project, Target = parent };
            ResourceModel.Get<IHookModel>().Shoot(origin);

            var center = (X: x - origin.OriginX, Y: y - origin.OriginY);
            var size = instance.Properties.FirstOrDefault(p => p.Name == "Size" && p.Type == typeof(Vec2))?.ToValue<Vec2>() ?? Vec2.Zero;
            positionProperty.Value = new Vec2((float)(center.X - size.X / 2.0), (float)(center.Y - size.Y / 2.0));
            return center;
        }

        private bool DropComponent(Project project, Component root, Component hit, Component definition, double x, double y)
        {
            var parent = new[] { hit, root }.FirstOrDefault(candidate => Accepts(candidate, definition));
            if (parent == null)
                return false;

            var instance = ResourceModel.Get<IComponentProvider>().CreateInstance(project, definition);
            Center(project, parent, instance, x, y);
            AddNode(project, parent, instance, $"Added {definition.FullName} to {parent.FullName}.");
            return true;
        }

        private bool DropContent(Project project, Component root, Component hit, string contentPath, double x, double y)
        {
            var association = GetContentAssociation(project);
            if (association == null)
                return false;

            var relativePath = GetRelativePath(project, contentPath);
            if (relativePath == null)
                return false;

            if (hit != null && !hit.ReadOnly && !hit.Removed && IsOfType(hit, association.DefaultComponent))
            {
                var property = hit.Properties.FirstOrDefault(p => p.Name == association.DefaultProperty);
                if (property != null && !property.ReadOnly)
                    return ResourceModel.Get<IEditorCommandRegistry>().Execute(
                        "component.change-property",
                        new ChangePropertyCommandData { Project = project, Component = hit, Property = property, Value = relativePath });
            }

            var componentProvider = ResourceModel.Get<IComponentProvider>();
            var definition = componentProvider.Load(project, association.DefaultComponent)
                ?? componentProvider.GetBaseTypeComponents().FirstOrDefault(c => c.FullName == association.DefaultComponent);
            if (definition == null)
                return false;

            var parent = new[] { hit, root }.FirstOrDefault(candidate => Accepts(candidate, definition));
            if (parent == null)
                return false;

            // The value is set before the node is added so that adding, saving and loading the content is one undoable step.
            var instance = componentProvider.CreateInstance(project, definition);
            var contentProperty = instance.Properties.FirstOrDefault(p => p.Name == association.DefaultProperty);
            if (contentProperty == null || contentProperty.ReadOnly)
                return false;

            contentProperty.Value = relativePath;
            var center = Center(project, parent, instance, x, y);
            if (center != null)
                PendingCenters[instance] = center.Value;
            AddNode(project, parent, instance, $"Added {definition.FullName} with {relativePath} to {parent.FullName}.");
            return true;
        }

        private void AddNode(Project project, Component parent, Component instance, string description)
        {
            var componentModel = ResourceModel.Get<IComponentModel>();
            componentModel.AddExisting(project, parent, instance);

            ResourceModel.Get<ICommandHistoryModel>().Record(
                "Add node",
                description,
                () => componentModel.Remove(project, instance),
                () => componentModel.AddExisting(project, parent, instance));

            ResourceModel.Get<IHookModel>().Shoot(new ComponentFocusHook { Project = project, Component = instance, Root = false });
        }

        private ContentTypeAssociation GetContentAssociation(Project project)
        {
            var path = ViewerDragPayload.ContentPath;
            if (path == null)
                return null;

            var association = ResourceModel.Get<IContentProvider>().GetAssociation(Path.GetExtension(path).TrimStart('.').ToLowerInvariant());
            return association?.DefaultComponent != null && association.DefaultProperty != null ? association : null;
        }

        /// <summary>
        /// Gets the path relative to the content folder, in the format used by content properties, or null if the file is outside of it.
        /// </summary>
        public static string GetRelativePath(Project project, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var relative = Path.GetRelativePath(project.ProjectContentPath, path);
            return relative.StartsWith("..") || Path.IsPathRooted(relative) ? null : relative.Replace('\\', '/');
        }

        private static Component ResolveVisible(Component component)
        {
            while (component != null && !component.IsVisible)
                component = component.ParentComponent;
            return component;
        }

        private static bool IsOfType(Component component, string fullName)
        {
            for (var current = component; current != null; current = current.BaseInheritedComponent)
            {
                if (current.FullName == fullName)
                    return true;
            }
            return false;
        }

        private static bool Accepts(Component parent, Component child)
        {
            if (parent == null || parent.ReadOnly || parent.Removed || !parent.IsVisible || child?.TypeOBaseType == null)
                return false;
            if (child.TypeOBaseType == typeof(Game) || child.TypeOBaseType == typeof(Scene))
                return false;

            var top = parent;
            while (top.ParentComponent != null)
                top = top.ParentComponent;
            if (top.FullName == child.FullName)
                return false;

            var filter = new FilterHelper();
            parent.Template?.ChildrenFilter(filter);
            return (filter.Filters ?? string.Empty).Contains($"{child.TypeOBaseType.FullName};");
        }
    }
}
