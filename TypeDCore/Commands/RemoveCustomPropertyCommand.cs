using System.Text.Json;
using System.Text.RegularExpressions;
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
    internal sealed class RemoveCustomPropertyCommand : CustomCommand<ChangePropertyCommandData>, IDisposable
    {
        private readonly IComponentProvider ComponentProvider;
        private readonly IProjectModel ProjectModel;
        private readonly IHookModel HookModel;
        private readonly ILogModel LogModel;
        private readonly ICommandHistoryModel History;
        private readonly IDisposable Registration;

        public RemoveCustomPropertyCommand(IResourceModel resources) : base(resources)
        {
            ComponentProvider = resources.Get<IComponentProvider>();
            ProjectModel = resources.Get<IProjectModel>();
            HookModel = resources.Get<IHookModel>();
            LogModel = resources.Get<ILogModel>();
            History = resources.Get<ICommandHistoryModel>();
            Registration = resources.Get<IEditorCommandRegistry>().Register(
                "component.remove-custom-property", "Remove property",
                "Remove a custom property from its declaring component.", "global", "", this);
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ChangePropertyCommandData parameter)
        {
            return parameter?.Project != null && CanRemove(parameter.Component, parameter.Property);
        }

        internal static bool CanRemove(Component component, Property property)
        {
            return component is { ReadOnly: false, Removed: false }
                && property != null && component.Properties.Contains(property)
                && (property.DeclaringTypeName ?? property.FromComponent?.FullName) == component.FullName
                && component.CustomProperties.Any(custom => custom.Name == property.Name);
        }

        public override async void Execute(ChangePropertyCommandData parameter)
        {
            try
            {
                var node = parameter.Component;
                var propertyName = parameter.Property.Name;
                var declaringName = node.FullName;
                var roots = ComponentProvider.ListAll(parameter.Project);
                var openedRoot = node;
                while (openedRoot.ParentComponent != null)
                    openedRoot = openedRoot.ParentComponent;
                roots.RemoveAll(component => component.FullName == openedRoot.FullName);
                roots.Add(openedRoot);

                var definition = roots.FirstOrDefault(component => component.FullName == declaringName);
                if (definition == null || definition.ReadOnly
                    || !definition.CustomProperties.Any(custom => custom.Name == propertyName))
                    throw new InvalidOperationException($"The definition of '{declaringName}.{propertyName}' is no longer available.");

                var plan = new RemoveCustomPropertyPlan(roots, declaringName, propertyName);
                var files = FindPossibleCodeReferences(parameter.Project.Location, propertyName)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase);
                var dialog = new PropertyChangePreviewDialog($"{declaringName}.{propertyName}", plan, files);
                if (await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) != true)
                    return;
                if (!CanExecute(parameter) || !definition.CustomProperties.Any(custom => custom.Name == propertyName))
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
                    plan.Apply();
                    try
                    {
                        Persist();
                    }
                    catch
                    {
                        plan.Undo();
                        Persist();
                        throw;
                    }
                }

                void Undo()
                {
                    plan.Undo();
                    try
                    {
                        Persist();
                    }
                    catch
                    {
                        plan.Apply();
                        Persist();
                        throw;
                    }
                }

                Apply();
                History.Record("Remove property", $"Removed {declaringName}.{propertyName}.", Undo, Apply);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
                or JsonException or InvalidOperationException)
            {
                LogModel.Log($"Could not remove custom property: {exception}");
                var box = MessageBoxManager.GetMessageBoxStandard("Remove property",
                    $"Could not remove the property: {exception.Message}", ButtonEnum.Ok, Icon.Error);
                await box.ShowWindowDialogAsync(ViewModelBase.MainWindow);
            }
        }

        private static IEnumerable<string> FindPossibleCodeReferences(string projectDirectory, string propertyName)
        {
            var pattern = new Regex($@"(?<![\p{{L}}\p{{Nd}}_]){Regex.Escape(propertyName)}(?![\p{{L}}\p{{Nd}}_])");
            var pending = new Stack<string>();
            pending.Push(projectDirectory);
            while (pending.Count > 0)
            {
                var directory = pending.Pop();
                foreach (var child in Directory.EnumerateDirectories(directory))
                {
                    var name = Path.GetFileName(child);
                    if (name.Equals("bin", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("obj", StringComparison.OrdinalIgnoreCase)
                        || name.Equals(".git", StringComparison.OrdinalIgnoreCase)
                        || name.Equals(".vs", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("node_modules", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("TypeO", StringComparison.OrdinalIgnoreCase)
                        || (File.GetAttributes(child) & FileAttributes.ReparsePoint) != 0)
                        continue;
                    pending.Push(child);
                }

                foreach (var file in Directory.EnumerateFiles(directory, "*.cs"))
                {
                    if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                        continue;
                    if (File.ReadLines(file).Any(line => pattern.IsMatch(line)))
                        yield return Path.GetRelativePath(projectDirectory, file);
                }
            }
        }
    }
}
