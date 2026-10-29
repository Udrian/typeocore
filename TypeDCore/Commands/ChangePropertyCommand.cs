using System;
using System.Collections.Generic;
using System.Linq;
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
            var newValue = parameter.Value;

            var targets = new List<(Component Component, Property Property, object Previous)> { (component, property, property.Value) };
            if (parameter.ApplyToSelection)
            {
                foreach (var other in NodeSelectionModel.SelectedNodes.Where(node => !ReferenceEquals(node, component) && !node.ReadOnly))
                {
                    var otherProperty = other.Properties.FirstOrDefault(p => p.Name == property.Name && p.Type == property.Type);
                    if (otherProperty != null && !otherProperty.ReadOnly)
                        targets.Add((other, otherProperty, otherProperty.Value));
                }
            }

            targets = targets.Where(target => !Equals(target.Previous, newValue)).ToList();
            if (targets.Count == 0)
                return;

            foreach (var target in targets)
                ApplyValue(parameter.Project, target.Component, target.Property, newValue);

            var description = targets.Count == 1
                ? $"Changed {targets[0].Component.Name}.{property.Name} from {FormatValue(targets[0].Previous)} to {FormatValue(newValue)}."
                : $"Changed {property.Name} to {FormatValue(newValue)} on {targets.Count} nodes.";
            CommandHistoryModel.Record(
                "Change property",
                description,
                () => targets.ForEach(target => ApplyValue(parameter.Project, target.Component, target.Property, target.Previous)),
                () => targets.ForEach(target => ApplyValue(parameter.Project, target.Component, target.Property, newValue)));
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
