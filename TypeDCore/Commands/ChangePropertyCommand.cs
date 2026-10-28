using System;
using TypeD.Commands;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class ChangePropertyCommand : CustomCommand<ChangePropertyCommandData>, IDisposable
    {
        private readonly IHookModel _hookModel;
        private readonly ISaveModel _saveModel;
        private readonly IProjectModel _projectModel;
        private readonly ICommandHistoryModel _commandHistoryModel;
        private readonly IDisposable _registration;

        public ChangePropertyCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            _hookModel = resourceModel.Get<IHookModel>();
            _saveModel = resourceModel.Get<ISaveModel>();
            _projectModel = resourceModel.Get<IProjectModel>();
            _commandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "component.change-property",
                "Change property",
                "Change a property on the opened component.",
                "global",
                string.Empty,
                this);
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(ChangePropertyCommandData parameter)
        {
            return parameter?.Project != null
                && parameter.Component != null
                && parameter.Property != null
                && !parameter.Property.ReadOnly;
        }

        public override void Execute(ChangePropertyCommandData parameter)
        {
            var property = parameter.Property;
            var previousValue = property.Value;
            var newValue = parameter.Value;
            if (Equals(previousValue, newValue))
                return;

            ApplyValue(parameter.Project, parameter.Component, property, newValue);
            _commandHistoryModel.Record(
                "Change property",
                $"Changed {parameter.Component.Name}.{property.Name} from {FormatValue(previousValue)} to {FormatValue(newValue)}.",
                () => ApplyValue(parameter.Project, parameter.Component, property, previousValue),
                () => ApplyValue(parameter.Project, parameter.Component, property, newValue));
        }

        private void ApplyValue(Project project, Component component, Property property, object value)
        {
            property.Value = value;
            _hookModel.Shoot(new PropertyChangedHook
            {
                Component = component,
                Property = property
            });

            var componentToSave = component;
            while (componentToSave.ParentComponent != null)
                componentToSave = componentToSave.ParentComponent;

            var context = _saveModel.GetSaveContext<ComponentSaveContext>(project);
            if (!context.Components.Contains(componentToSave))
            {
                context.Components.Add(componentToSave);
                _saveModel.AddSave<ComponentSaveContext>();
                _projectModel.SaveCode(componentToSave.Template.Code);
            }
        }

        private static string FormatValue(object value) => value?.ToString() ?? "null";
    }
}
