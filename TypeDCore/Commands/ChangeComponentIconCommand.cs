using System;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class ChangeComponentIconCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        private readonly IComponentProvider ComponentProvider;
        private readonly IProjectModel ProjectModel;
        private readonly IHookModel HookModel;
        private readonly ICommandHistoryModel CommandHistoryModel;
        private readonly INodeSelectionModel NodeSelectionModel;
        private readonly IDisposable Registration;

        public ChangeComponentIconCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = resourceModel.Get<IComponentProvider>();
            ProjectModel = resourceModel.Get<IProjectModel>();
            HookModel = resourceModel.Get<IHookModel>();
            CommandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            NodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            Registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "component.change-icon",
                "Change component icon",
                "Change the icon used for the selected component type.",
                "global",
                null,
                this,
                () => new ComponentCommandData
                {
                    Project = NodeSelectionModel.Project,
                    Component = NodeSelectionModel.SelectedComponentType
                });
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ComponentCommandData parameter)
        {
            return parameter?.Project != null && parameter.Component != null && !parameter.Component.ReadOnly;
        }

        public override async void Execute(ComponentCommandData parameter)
        {
            var project = parameter.Project;
            var component = parameter.Component;
            var previous = component.Icon;

            var dialog = new IconSelectorDialog(project, component.ResolvedIcon);
            if (await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) != true || dialog.SelectedIcon == null)
                return;

            var icon = dialog.SelectedIcon;
            if (icon == component.ResolvedIcon)
                return;

            SetIcon(project, component, icon);
            CommandHistoryModel.Record(
                "Change component icon",
                $"Changed the icon of {component.FullName} to {icon}.",
                () => SetIcon(project, component, previous),
                () => SetIcon(project, component, icon));
        }

        private void SetIcon(Project project, Component component, string icon)
        {
            component.Icon = icon;
            ComponentProvider.Save(project, component);
            ProjectModel.BuildComponentTree(project);
            HookModel.Shoot(new ComponentIconChangedHook() { Project = project, Component = component });
        }
    }
}
