using System;
using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Interfaces;
using TypeD.Models.Data;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.DTO;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeDCore.Components;
using TypeD.Models.Providers.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities;

namespace TypeDCore.Commands
{
    internal class CreateEntityTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        IComponentProvider ComponentProvider { get; set; }
        IHookModel HookModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        private readonly IDisposable _registration;

        // Constructors
        public CreateEntityTypeCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            HookModel = ResourceModel.Get<IHookModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.create-entity",
                "Create entity type",
                "Create a new entity component type.",
                "global",
                string.Empty,
                this,
                () => new CreateComponentCommandData(project, "Entities"));
        }

        public void Dispose() => _registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            var options = await ResourceModel.Get<IEditorDialogModel>().CreateComponentAsync(parameter.Project, parameter.Namespace, CoreComponent.EntityComponent());
            if (options != null)
            {
                if (!HookModel.Shoot(new CreateComponentHook(new CreateComponentDTO()
                {
                    Name = options.Name,
                    Namespace = options.Namespace,
                    ParentComponent = options.ParentComponent
                })).Handled)
                {
                    if (options.BaseType == typeof(Entity).FullName)
                    {
                        var className = options.Name;
                        var @namespace = options.Namespace;
                        var parent = options.ParentComponent;
                        var updatable = options.Updatable;
                        var drawable = options.Drawable;
                        var icon = options.Icon;
                        var component = TypeDCoreProjectModel.CreateEntity(
                            parameter.Project, className, @namespace, parent, updatable, drawable, icon);
                        CommandHistoryModel.Record(
                            "Create entity type",
                            $"Created {component.FullName} based on {parent?.FullName ?? "an unspecified base type"}.",
                            () => ComponentProvider.Delete(parameter.Project, component),
                            () => component = TypeDCoreProjectModel.CreateEntity(
                                parameter.Project, className, @namespace, parent, updatable, drawable, icon));
                    }
                }
            }
        }
    }
}
