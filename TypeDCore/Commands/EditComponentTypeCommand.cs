using System.Text.Json;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TypeD.Commands;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;
using TypeDCore.View.Dialogs.Project;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeDCore.Commands
{
    internal sealed class EditComponentTypeCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        private readonly IComponentProvider Components;
        private readonly IProjectModel Projects;
        private readonly IPropertyModel Properties;
        private readonly IHookModel Hooks;
        private readonly ILogModel Log;
        private readonly ICommandHistoryModel History;
        private readonly INodeSelectionModel Selection;
        private readonly IDisposable Registration;

        public EditComponentTypeCommand(IResourceModel resources) : base(resources)
        {
            Components = resources.Get<IComponentProvider>();
            Projects = resources.Get<IProjectModel>();
            Properties = resources.Get<IPropertyModel>();
            Hooks = resources.Get<IHookModel>();
            Log = resources.Get<ILogModel>();
            History = resources.Get<ICommandHistoryModel>();
            Selection = resources.Get<INodeSelectionModel>();
            Registration = resources.Get<IEditorCommandRegistry>().Register(
                "component.edit-type", "Edit component", "Edit the selected component type.", "global", "",
                this, () => new ComponentCommandData
                {
                    Project = Selection.Project,
                    Component = Selection.SelectedComponentType
                });
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ComponentCommandData parameter)
        {
            if (parameter?.Project == null || parameter.Component is not { ReadOnly: false, Removed: false } component
                || component.BaseInheritedComponent == null)
                return false;
            var root = component.RootBaseComponent.FullName;
            return root == typeof(Entity).FullName || root == typeof(Scene).FullName || root == typeof(Drawable).FullName;
        }

        public override async void Execute(ComponentCommandData parameter)
        {
            try
            {
                await ExecuteAsync(parameter);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException
                or InvalidOperationException or ArgumentException or AggregateException or System.ComponentModel.Win32Exception)
            {
                Log.Log($"Could not edit component: {exception}");
                await MessageBoxManager.GetMessageBoxStandard("Edit component",
                    $"Could not edit the component: {exception.Message}", ButtonEnum.Ok, Icon.Error)
                    .ShowWindowDialogAsync(ViewModelBase.MainWindow);
            }
        }

        internal Task ExecuteAsync(ComponentCommandData parameter) => ExecuteWithDialogsAsync(parameter,
            dialog => dialog.ShowDialog<bool>(ViewModelBase.MainWindow),
            async description => await MessageBoxManager.GetMessageBoxStandard("Confirm component changes", description,
                ButtonEnum.YesNo, Icon.Warning).ShowWindowDialogAsync(ViewModelBase.MainWindow) == ButtonResult.Yes);

        internal async Task ExecuteWithDialogsAsync(ComponentCommandData parameter,
            Func<CreateComponentTypeDialog, Task<bool>> showEditor, Func<string, Task<bool>> confirm)
            => await ExecuteWithScanAsync(parameter, showEditor, confirm, ScanReferencesAsync);

        private static async Task<PropertyCodeRename> ScanReferencesAsync(Project project, string oldName,
            IReadOnlyList<(string OldName, string NewName)> propertyRenames, string className, string componentNamespace)
        {
            var progress = new PropertyReferenceScanDialog();
            var closed = progress.ShowDialog(ViewModelBase.MainWindow);
            try
            {
                return await Task.Run(() => PropertyCodeRename.CreateComponentAsync(project, oldName, propertyRenames,
                    className, componentNamespace));
            }
            finally
            {
                progress.Complete();
                await closed;
            }
        }

        internal async Task ExecuteWithScanAsync(ComponentCommandData parameter,
            Func<CreateComponentTypeDialog, Task<bool>> showEditor, Func<string, Task<bool>> confirm,
            Func<Project, string, IReadOnlyList<(string OldName, string NewName)>, string, string, Task<PropertyCodeRename>> scan)
        {
            if (!CanExecute(parameter))
                throw new InvalidOperationException("The selected component is no longer editable.");

            var project = parameter.Project;
            var selected = parameter.Component;
            var roots = Components.ListAll(project);
            if (!roots.Any(root => root.FullName == selected.FullName))
                throw new InvalidOperationException("The selected component definition no longer exists.");
            if (Selection.Project == project && Selection.OpenedComponent is { } opened)
            {
                while (opened.ParentComponent != null)
                    opened = opened.ParentComponent;
                if (roots.Any(root => root.FullName == opened.FullName))
                {
                    roots.RemoveAll(root => root.FullName == opened.FullName);
                    roots.Add(opened);
                }
            }

            var definition = roots.Single(root => root.FullName == selected.FullName);
            if (definition.ReadOnly || definition.Removed || definition.BaseInheritedComponent == null)
                throw new InvalidOperationException("The selected component definition is no longer editable.");
            var oldName = definition.FullName;
            var oldBase = definition.BaseInheritedComponent.FullName;
            var oldIcon = definition.Icon;
            var oldInterfaces = definition.Interfaces.ToArray();
            var originalProperties = definition.CustomProperties.ToArray();
            var originalJson = JsonSerializer.Serialize(originalProperties, JSON.CreateOptions());
            var dialog = new CreateComponentTypeDialog(project, definition, Components.GetBaseTypeComponents());
            if (!await showEditor(dialog))
                return;

            var draft = dialog.ViewModel;
            if (!CanExecute(parameter) || definition.ReadOnly || definition.Removed || definition.FullName != oldName
                || definition.BaseInheritedComponent?.FullName != oldBase || definition.Icon != oldIcon
                || !definition.Interfaces.SequenceEqual(oldInterfaces)
                || !definition.CustomProperties.SequenceEqual(originalProperties)
                || JsonSerializer.Serialize(definition.CustomProperties, JSON.CreateOptions()) != originalJson)
                throw new InvalidOperationException("The component changed while the edit dialog was open.");
            var identityChanged = draft.ResultName != oldName;
            var interfaces = oldInterfaces.ToList();
            void SetCapability(Type type, bool enabled)
            {
                if (!enabled)
                    interfaces.RemoveAll(item => item == type);
                else if (!interfaces.Contains(type) && !draft.ParentComponent.HasInterface(type))
                    interfaces.Add(type);
            }
            SetCapability(typeof(IUpdatable), draft.IsEntity && draft.ComponentUpdatable);
            SetCapability(typeof(IDrawable), draft.IsEntity && draft.ComponentDrawable);
            var rebase = draft.ParentComponent.FullName != oldBase || !interfaces.SequenceEqual(oldInterfaces)
                ? new ComponentRebasePlan(project, Projects, Properties, roots, oldName, draft.ParentComponent, interfaces)
                : null;
            var propertyChanged = draft.CustomProperties.Count != originalProperties.Length
                || draft.CustomProperties.Where((item, index) => index >= originalProperties.Length
                    || !ReferenceEquals(draft.OriginalProperty(item), originalProperties[index])
                    || JsonSerializer.Serialize(item, JSON.CreateOptions()) != JsonSerializer.Serialize(originalProperties[index], JSON.CreateOptions())).Any();
            ComponentCustomPropertyPlan propertyPlan = null;
            if (propertyChanged)
            {
                var available = Properties.List().ToList();
                var desired = draft.CustomProperties.Select(property =>
                {
                    var type = available.SingleOrDefault(item => item.Type.FullName == property.TypeName
                        && item.Type == item.ValueType)?.Type
                        ?? throw new InvalidOperationException($"Property type '{property.TypeName}' is no longer available.");
                    return (OriginalName: draft.OriginalProperty(property)?.Name, Property: property, Type: type);
                }).ToList();
                if (rebase != null)
                    rebase.Apply();
                try
                {
                    propertyPlan = new ComponentCustomPropertyPlan(roots, oldName, desired, Properties);
                }
                finally
                {
                    if (rebase != null)
                        rebase.Undo();
                }
            }
            var propertyRenames = draft.CustomProperties
                .Select(property => (Original: draft.OriginalProperty(property), Property: property))
                .Where(item => item.Original != null && item.Original.Name != item.Property.Name)
                .Select(item => (OldName: item.Original.Name, NewName: item.Property.Name)).ToList();
            PropertyCodeRename rename = null;
            if (propertyRenames.Count != 0 || identityChanged)
            {
                if (ResourceModel.Get<ISaveModel>().AnythingToSave)
                    throw new InvalidOperationException("Save the project before renaming a component or custom property.");
                rename = await scan(project, oldName, propertyRenames, identityChanged ? draft.ComponentName : null,
                    identityChanged && draft.FullNamespace != definition.Namespace ? draft.FullNamespace : null);
            }
            ComponentIdentityPlan identity = null;
            if (identityChanged)
            {
                if (rebase != null) rebase.Apply();
                var propertyApplied = false;
                try
                {
                    if (propertyPlan != null)
                    {
                        propertyPlan.Apply();
                        propertyApplied = true;
                    }
                    identity = new ComponentIdentityPlan(project, roots, oldName, draft.FullNamespace, draft.ComponentName);
                }
                finally
                {
                    if (propertyApplied) propertyPlan.Undo();
                    if (rebase != null) rebase.Undo();
                }
            }
            var iconChanges = oldIcon == draft.IconName
                ? new List<(Component Component, string Before)>()
                : roots.SelectMany(RemoveCustomPropertyPlan.Traverse).Distinct()
                    .Where(component => component.FullName == oldName)
                    .Select(component => (Component: component, Before: component.Icon)).ToList();
            if (propertyPlan == null && iconChanges.Count == 0 && rebase == null && identity == null)
                return;

            var affected = (propertyPlan?.AffectedComponents ?? Array.Empty<Component>())
                .Concat(rebase?.AffectedComponents ?? Array.Empty<Component>())
                .Concat(identity?.AffectedComponents ?? Array.Empty<Component>())
                .Concat(iconChanges.Select(change => change.Component)).Distinct().ToHashSet();
            var affectedRoots = roots.Where(root => RemoveCustomPropertyPlan.Traverse(root).Any(affected.Contains)
                || identity?.AffectedRoots.Contains(root) == true).ToList();
            var identityFiles = identity == null ? null : new ComponentIdentityFiles(project, Components,
                ResourceModel.Get<ISaveModel>(), affectedRoots, definition, draft.ResultName, identity.ChangesProject, rename);
            var description = $"Changes to {oldName}:\n"
                + (iconChanges.Count == 0 ? "" : "- Change icon\n")
                + (identity == null ? "" : $"- Rename component: {oldName} -> {draft.ResultName}\n")
                + (rebase == null ? "" : $"- Change base type or capabilities: {oldBase} -> {draft.ParentComponent.FullName}\n")
                + (propertyPlan == null ? "" : string.Concat(draft.CustomProperties.Select(property =>
                    draft.OriginalProperty(property) is { } original
                        ? $"- Edit {original.Name} -> {property.Name}\n"
                        : $"- Add {property.Access} property {property.Name}\n"))
                    + string.Concat(originalProperties.Where(original =>
                        !draft.CustomProperties.Any(item => ReferenceEquals(draft.OriginalProperty(item), original)))
                        .Select(original => $"- Remove property {original.Name}\n")))
                + "\nAffected component files:\n"
                + string.Join("\n", affectedRoots.Select(root => Path.GetRelativePath(project.Location, Components.GetPath(project, root))))
                + (rebase?.PreservedOverrides.Count > 0
                    ? "\n\nValues preserved for manual repair:\n" + string.Join("\n", rebase.PreservedOverrides) : "")
                + (propertyPlan?.PreservedValues.Count > 0
                    ? "\n\nCustom property values preserved for manual repair:\n" + string.Join("\n", propertyPlan.PreservedValues) : "")
                + (rename?.Paths.Count > 0 ? "\n\nSource files to update:\n"
                    + string.Join("\n", rename.Paths.Select(path => Path.GetRelativePath(project.Location, path))) : "")
                + (identityFiles == null ? "" : "\n\nThe identity change is saved immediately. Save the project before undo or redo.\nFiles involved:\n"
                    + string.Join("\n", identityFiles.Paths.Select(path => Path.GetRelativePath(project.Location, path))))
                + "\n\nGenerated code will also be updated. Check handwritten code and child compatibility for required repairs.";
            if (!await confirm(description))
                return;
            if (rename != null && ResourceModel.Get<ISaveModel>().AnythingToSave)
                throw new InvalidOperationException("Save the project before renaming a component or custom property.");
            if (!CanExecute(parameter) || definition.ReadOnly || definition.Removed || definition.FullName != oldName
                || definition.BaseInheritedComponent?.FullName != oldBase
                || definition.Icon != oldIcon
                || !definition.Interfaces.SequenceEqual(oldInterfaces)
                || !definition.CustomProperties.SequenceEqual(originalProperties)
                || JsonSerializer.Serialize(definition.CustomProperties, JSON.CreateOptions()) != originalJson)
                throw new InvalidOperationException("The component changed while the confirmation was open.");

            void Persist()
            {
                if (identity == null)
                {
                    foreach (var root in affectedRoots)
                    {
                        Components.Save(project, root);
                        if (propertyPlan != null || rebase != null)
                            Projects.SaveCode(root.Template.Code);
                    }
                }
                Projects.BuildComponentTree(project);
                foreach (var component in affected)
                    Hooks.Shoot(new ComponentPropertiesChangedHook { Component = component });
                if (iconChanges.Count != 0)
                    Hooks.Shoot(new ComponentIconChangedHook { Project = project, Component = definition });
            }

            ComponentFileChangeSet identityChanges = null;
            var templates = new List<(Component Component, TypeD.Components.ComponentTemplate Before,
                TypeD.Components.ComponentTemplate After)>();
            void RefreshTemplates()
            {
                try
                {
                    foreach (var node in roots.SelectMany(RemoveCustomPropertyPlan.Traverse).Distinct()
                        .Where(node => node.FullName == draft.ResultName
                            || node.BaseInheritedComponent?.FullName == draft.ResultName))
                    {
                        var before = node.Template;
                        var after = node.CreateInitializedTemplate(project, Projects);
                        templates.Add((node, before, after));
                        node.Template = after;
                    }
                }
                catch
                {
                    foreach (var item in templates)
                        item.Component.Template = item.Before;
                    templates.Clear();
                    throw;
                }
            }

            void RestoreTemplates()
            {
                if (templates.Any(item => !ReferenceEquals(item.Component.Template, item.After)))
                    throw new InvalidOperationException("A component template changed since the edit was applied.");
                foreach (var item in templates)
                    item.Component.Template = item.Before;
                templates.Clear();
            }

            void Apply()
            {
                using var lease = identity == null ? null : ResourceModel.Get<ISaveModel>().BeginExclusiveUpdate();
                if (iconChanges.Any(change => change.Component.Icon != change.Before))
                    throw new InvalidOperationException("A component icon changed since the edit was prepared.");
                var propertyApplied = false;
                var iconsApplied = false;
                var rebased = false;
                var renamed = false;
                var identityApplied = false;
                var refreshed = false;
                var filesApplied = false;
                try
                {
                    if (rename != null && identity == null)
                    {
                        rename.Apply();
                        renamed = true;
                    }
                    if (rebase != null)
                    {
                        rebase.Apply();
                        rebased = true;
                    }
                    if (propertyPlan != null)
                    {
                        propertyPlan.Apply();
                        propertyApplied = true;
                    }
                    if (identity != null)
                    {
                        identity.Apply();
                        identityApplied = true;
                        RefreshTemplates();
                        refreshed = true;
                    }
                    foreach (var change in iconChanges)
                        change.Component.Icon = draft.IconName;
                    iconsApplied = iconChanges.Count != 0;
                    if (identity != null)
                    {
                        identityChanges ??= identityFiles.CreateChangeSet();
                        identityChanges.Apply();
                        filesApplied = true;
                    }
                    Persist();
                    if (identity != null)
                        ProjectSaveState.Record(project);
                }
                catch (Exception failure)
                {
                    try
                    {
                        if (filesApplied)
                            identityChanges.Undo();
                        if (iconsApplied)
                            foreach (var change in iconChanges)
                                change.Component.Icon = change.Before;
                        if (refreshed)
                            RestoreTemplates();
                        if (identityApplied)
                            identity.Undo();
                        if (propertyApplied)
                            propertyPlan.Undo();
                        if (rebased)
                            rebase.Undo();
                        if (renamed)
                            rename.Undo();
                        if (propertyApplied || iconsApplied || rebased || identityApplied)
                            Persist();
                    }
                    catch (Exception rollback)
                    {
                        throw new AggregateException("The edit failed and its rollback also failed.", failure, rollback);
                    }
                    throw;
                }
            }

            void Undo()
            {
                using var lease = identity == null ? null : ResourceModel.Get<ISaveModel>().BeginExclusiveUpdate();
                if (iconChanges.Any(change => change.Component.Icon != draft.IconName))
                    throw new InvalidOperationException("A component icon changed since the edit was applied.");
                var propertyUndone = false;
                var iconsUndone = false;
                var rebaseUndone = false;
                var renameUndone = false;
                var filesUndone = false;
                var templatesRestored = false;
                var identityUndone = false;
                try
                {
                    if (identity != null)
                    {
                        identityChanges.Undo();
                        filesUndone = true;
                    }
                    else if (rename != null)
                    {
                        rename.Undo();
                        renameUndone = true;
                    }
                    foreach (var change in iconChanges)
                        change.Component.Icon = change.Before;
                    iconsUndone = iconChanges.Count != 0;
                    if (identity != null)
                    {
                        RestoreTemplates();
                        templatesRestored = true;
                        identity.Undo();
                        identityUndone = true;
                    }
                    if (propertyPlan != null)
                    {
                        propertyPlan.Undo();
                        propertyUndone = true;
                    }
                    if (rebase != null)
                    {
                        rebase.Undo();
                        rebaseUndone = true;
                    }
                    Persist();
                    if (identity != null)
                        ProjectSaveState.Record(project);
                }
                catch (Exception failure)
                {
                    try
                    {
                        if (rebaseUndone)
                            rebase.Apply();
                        if (propertyUndone)
                            propertyPlan.Apply();
                        if (identityUndone)
                            identity.Apply();
                        if (templatesRestored)
                            RefreshTemplates();
                        if (renameUndone)
                            rename.Apply();
                        if (iconsUndone)
                            foreach (var change in iconChanges)
                                change.Component.Icon = draft.IconName;
                        if (filesUndone)
                            identityChanges.Apply();
                        if (propertyUndone || iconsUndone || rebaseUndone || identityUndone)
                            Persist();
                    }
                    catch (Exception rollback)
                    {
                        throw new AggregateException("The undo failed and its rollback also failed.", failure, rollback);
                    }
                    throw;
                }
            }

            Apply();
            History.Record("Edit component", $"Edited {oldName}.", Undo, Apply);
            if (draft.PropertyPreference is { } preference)
            {
                try
                {
                    var json = preference.RememberFields
                        ? JsonSerializer.Serialize(preference.Property.DefaultValue, preference.Type, JSON.CreateOptions()) : null;
                    AddCustomPropertyCommand.SavePreference(ResourceModel, preference.Property, preference.Type,
                        preference.RememberFields, json);
                }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException
                    or InvalidOperationException or ArgumentException)
                {
                    Log.Log($"Component was edited, but property preferences could not be saved: {exception}");
                    await MessageBoxManager.GetMessageBoxStandard("Edit component",
                        $"The component was edited, but property preferences could not be saved: {exception.Message}",
                        ButtonEnum.Ok, Icon.Warning).ShowWindowDialogAsync(ViewModelBase.MainWindow);
                }
            }
        }
    }
}
