using System.Text.Json;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal sealed class EditCustomPropertyCommand : CustomCommand<ChangePropertyCommandData>, IDisposable
    {
        private readonly IComponentProvider ComponentProvider;
        private readonly IProjectModel ProjectModel;
        private readonly IPropertyModel PropertyModel;
        private readonly IHookModel HookModel;
        private readonly ILogModel LogModel;
        private readonly ICommandHistoryModel History;
        private readonly INodeSelectionModel Selection;
        private readonly IDisposable Registration;

        public EditCustomPropertyCommand(IResourceModel resources) : base(resources)
        {
            ComponentProvider = resources.Get<IComponentProvider>();
            ProjectModel = resources.Get<IProjectModel>();
            PropertyModel = resources.Get<IPropertyModel>();
            HookModel = resources.Get<IHookModel>();
            LogModel = resources.Get<ILogModel>();
            History = resources.Get<ICommandHistoryModel>();
            Selection = resources.Get<INodeSelectionModel>();
            Registration = resources.Get<IEditorCommandRegistry>().Register(
                "component.edit-custom-property", "Edit property",
                "Edit a custom property on its declaring component.", "global", "", this);
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ChangePropertyCommandData parameter) =>
            parameter?.Project != null && RemoveCustomPropertyCommand.CanRemove(parameter.Component, parameter.Property);

        public override async void Execute(ChangePropertyCommandData parameter)
        {
            try
            {
                if (!CanExecute(parameter))
                    throw new InvalidOperationException("The selected property is no longer editable.");
                var node = parameter.Component;
                var oldName = parameter.Property.Name;
                var declaringName = node.FullName;
                var roots = ComponentProvider.ListAll(parameter.Project);
                var openedRoot = node;
                while (openedRoot.ParentComponent != null)
                    openedRoot = openedRoot.ParentComponent;
                roots.RemoveAll(component => component.FullName == openedRoot.FullName);
                roots.Add(openedRoot);
                var definition = roots.FirstOrDefault(component => component.FullName == declaringName);
                var existing = definition?.CustomProperties.FirstOrDefault(property => property.Name == oldName);
                if (definition == null || definition.ReadOnly || existing == null)
                    throw new InvalidOperationException($"The definition of '{declaringName}.{oldName}' is no longer available.");
                var original = new CustomProperty(existing);
                var names = new HashSet<string>(definition.Properties.Select(property => property.Name));
                for (var current = definition; current != null; current = current.BaseInheritedComponent)
                    foreach (var custom in current.CustomProperties)
                        names.Add(custom.Name);
                names.Remove(oldName);

                var definitions = PropertyModel.List().ToList();
                var components = roots.Concat(ComponentProvider.GetBaseTypeComponents())
                    .Concat(new[] { Selection.OpenedComponent, Selection.SelectedNode, Selection.SelectedComponentType })
                    .Where(component => component != null);
                var dialog = new AddCustomPropertyDialog(definitions, names, components, original);
                if (!await dialog.ShowDialog<bool>(ViewModelBase.MainWindow))
                    return;
                var updated = dialog.ViewModel.Build();
                var newType = dialog.ViewModel.SelectedType.Type;
                var plan = new EditCustomPropertyPlan(roots, declaringName, original, updated, newType);
                PropertyCodeRename rename = null;
                if (original.Name != updated.Name)
                {
                    var progress = new PropertyReferenceScanDialog();
                    var closed = progress.ShowDialog(ViewModelBase.MainWindow);
                    try
                    {
                        rename = await Task.Run(() => PropertyCodeRename.CreateAsync(parameter.Project, declaringName, original.Name, updated.Name));
                    }
                    finally
                    {
                        progress.Complete();
                        await closed;
                    }
                }
                var files = rename?.Paths.Select(path => Path.GetRelativePath(parameter.Project.Location, path))
                    ?? Enumerable.Empty<string>();
                var preview = new PropertyChangePreviewDialog(plan,
                    definitions.FirstOrDefault(definition => definition.Type == newType), files,
                    original.TypeName != updated.TypeName, original.Access != updated.Access,
                    updated.Access != CustomPropertyAccess.Public);
                if (!await preview.ShowDialog<bool>(ViewModelBase.MainWindow))
                    return;
                if (!CanExecute(parameter) || !ReferenceEquals(existing, definition.CustomProperties.FirstOrDefault(property => property.Name == oldName)))
                    throw new InvalidOperationException("The property changed while the confirmation was open.");

                void Persist()
                {
                    foreach (var root in plan.AffectedRoots)
                    {
                        ComponentProvider.Save(parameter.Project, root);
                        ProjectModel.SaveCode(root.Template.Code);
                    }
                    foreach (var component in plan.AffectedComponents)
                        HookModel.Shoot(new ComponentPropertiesChangedHook { Component = component });
                }

                void Apply()
                {
                    rename?.Apply();
                    var applied = false;
                    try
                    {
                        plan.Apply();
                        applied = true;
                        Persist();
                    }
                    catch
                    {
                        try
                        {
                            if (applied)
                            {
                                plan.Undo();
                                Persist();
                            }
                        }
                        finally { rename?.Undo(); }
                        throw;
                    }
                }

                void Undo()
                {
                    rename?.Undo();
                    var undone = false;
                    try
                    {
                        plan.Undo();
                        undone = true;
                        Persist();
                    }
                    catch
                    {
                        try
                        {
                            if (undone)
                            {
                                plan.Apply();
                                Persist();
                            }
                        }
                        finally { rename?.Apply(); }
                        throw;
                    }
                }

                Apply();
                History.Record("Edit property", $"Edited {declaringName}.{oldName}.", Undo, Apply);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                or JsonException or InvalidOperationException or ArgumentException or System.ComponentModel.Win32Exception
                || exception.GetType() == typeof(Exception))
            {
                LogModel.Log($"Could not edit custom property: {exception}");
                var box = MessageBoxManager.GetMessageBoxStandard("Edit property",
                    $"Could not edit the property: {exception.Message}", ButtonEnum.Ok, Icon.Error);
                await box.ShowWindowDialogAsync(ViewModelBase.MainWindow);
            }
        }
    }
}
