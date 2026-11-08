using System;
using System.Collections.Generic;
using System.Linq;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class AddCustomPropertyCommand : CustomCommand<ComponentCommandData>, IDisposable
    {
        // Models
        IComponentProvider ComponentProvider { get; set; }
        IProjectModel ProjectModel { get; set; }
        IPropertyModel PropertyModel { get; set; }
        IHookModel HookModel { get; set; }
        ILogModel LogModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        INodeSelectionModel NodeSelectionModel { get; set; }
        private readonly IDisposable Registration;

        // Constructors
        public AddCustomPropertyCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            ProjectModel = ResourceModel.Get<IProjectModel>();
            PropertyModel = ResourceModel.Get<IPropertyModel>();
            HookModel = ResourceModel.Get<IHookModel>();
            LogModel = ResourceModel.Get<ILogModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            NodeSelectionModel = ResourceModel.Get<INodeSelectionModel>();
            Registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.add-property",
                "Add property",
                "Add a custom property to the selected component.",
                "global",
                string.Empty,
                this,
                () => new ComponentCommandData { Project = NodeSelectionModel.Project });
        }

        public void Dispose() => Registration.Dispose();

        public override bool CanExecute(ComponentCommandData parameter)
        {
            var node = GetTargetNode(parameter);
            if (parameter?.Project == null || node == null)
                return false;

            // The property is added to the component's own definition, which the node is an instance of.
            var definition = node.ParentComponent == null ? node : ComponentProvider.Load(parameter.Project, node.FullName);
            if (definition == null || definition.ReadOnly)
                return false;

            return true;
        }

        private Component GetTargetNode(ComponentCommandData parameter)
        {
            return parameter.Component ?? NodeSelectionModel.SelectedNode ?? NodeSelectionModel.OpenedComponent;
        }

        public override async void Execute(ComponentCommandData parameter)
        {
            var node = GetTargetNode(parameter);
            if (node == null)
                return;

            // The property is added to the component's own definition, which the node is an instance of.
            var definition = node.ParentComponent == null ? node : ComponentProvider.Load(parameter.Project, node.FullName);
            if (definition == null || definition.ReadOnly)
            {
                LogModel.Log($"Properties can not be added to '{node.FullName}' because it is not a component of this project.");
                return;
            }

            var takenNames = new HashSet<string>(definition.Properties.Select(property => property.Name));
            for (var declaring = definition; declaring != null; declaring = declaring.BaseInheritedComponent)
            {
                foreach (var customProperty in declaring.CustomProperties)
                    takenNames.Add(customProperty.Name);
            }

            var result = await ResourceModel.Get<IEditorDialogModel>().AddCustomPropertyAsync(PropertyModel.List(), takenNames);
            if (result == null)
                return;
            var custom = result.Value.Property;
            var type = result.Value.Type;

            var targets = new List<Component> { definition };
            if (!ReferenceEquals(node, definition))
                targets.Add(node);

            void Apply()
            {
                definition.CustomProperties.Add(custom);
                if (custom.Access == CustomPropertyAccess.Public)
                {
                    foreach (var target in targets)
                    {
                        var property = new Property
                        {
                            Name = custom.Name,
                            Description = custom.Description,
                            Group = custom.Group,
                            DeclaringTypeName = definition.FullName,
                            Type = type,
                            Value = custom.DefaultValue,
                            FromComponent = definition
                        };
                        target.Properties.Add(property);
                        target.OveriddenProperties.Add(new Property(property));
                    }
                }
                SaveDefinition(parameter.Project, definition);
                HookModel.Shoot(new ComponentPropertiesChangedHook { Component = node });
            }

            void Revert()
            {
                definition.CustomProperties.Remove(custom);
                foreach (var target in targets)
                {
                    target.Properties.RemoveAll(property => property.Name == custom.Name && ReferenceEquals(property.FromComponent, definition));
                    target.OveriddenProperties.RemoveAll(property => property.Name == custom.Name && ReferenceEquals(property.FromComponent, definition));
                }
                SaveDefinition(parameter.Project, definition);
                HookModel.Shoot(new ComponentPropertiesChangedHook { Component = node });
            }

            Apply();
            CommandHistoryModel.Record(
                "Add property",
                $"Added {custom.Access.ToString().ToLowerInvariant()} property {custom.Name} to {definition.FullName}.",
                Revert,
                Apply);
        }

        private void SaveDefinition(Project project, Component definition)
        {
            ComponentProvider.Save(project, definition);
            ProjectModel.SaveCode(definition.Template.Code);
        }
    }
}
