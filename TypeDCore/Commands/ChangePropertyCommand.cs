using System;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;

namespace TypeDCore.Commands
{
    internal class ChangePropertyCommand : CustomCommand<ChangePropertyCommandData>, IDisposable
    {
        private IHookModel HookModel { get; set; }
        private ISaveModel SaveModel { get; set; }
        private IProjectModel ProjectModel { get; set; }
        private ICommandHistoryModel CommandHistoryModel { get; set; }
        private INodeSelectionModel NodeSelectionModel { get; set; }
        private readonly IDisposable Registration;

        public ChangePropertyCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            HookModel = resourceModel.Get<IHookModel>();
            SaveModel = resourceModel.Get<ISaveModel>();
            ProjectModel = resourceModel.Get<IProjectModel>();
            CommandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            NodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            Registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "component.change-property",
                "Change property",
                "Change a property on the opened component.",
                "global",
                string.Empty,
                this);
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ChangePropertyCommandData parameter)
        {
            return parameter?.Project != null
                && NodeSelectionModel.SelectedNode != null
                && !NodeSelectionModel.SelectedNode.ReadOnly
                && parameter.Property != null
                && !parameter.Property.ReadOnly;
        }

        public override void Execute(ChangePropertyCommandData parameter)
        {
            var property = parameter.Property;
            var component = NodeSelectionModel.SelectedNode;
            var previousValue = property.Value;
            var newValue = parameter.Value;
            if (Equals(previousValue, newValue))
                return;

            ApplyValue(parameter.Project, component, property, newValue);
            CommandHistoryModel.Record(
                "Change property",
                $"Changed {component.Name}.{property.Name} from {FormatValue(previousValue)} to {FormatValue(newValue)}.",
                () => ApplyValue(parameter.Project, component, property, previousValue),
                () => ApplyValue(parameter.Project, component, property, newValue));
        }

        private void ApplyValue(Project project, Component component, Property property, object value)
        {
            property.Value = value;
            HookModel.Shoot(new PropertyChangedHook
            {
                Component = component,
                Property = property
            });

            var componentToSave = component;
            while (componentToSave.ParentComponent != null)
                componentToSave = componentToSave.ParentComponent;

            var context = SaveModel.GetSaveContext<ComponentSaveContext>(project);
            if (!context.Components.Contains(componentToSave))
            {
                context.Components.Add(componentToSave);
                SaveModel.AddSave<ComponentSaveContext>();
                ProjectModel.SaveCode(componentToSave.Template.Code);
            }
        }

        private static string FormatValue(object value) => value?.ToString() ?? "null";
    }
}
