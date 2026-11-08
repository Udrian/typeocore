using System;
using TypeD.Commands;
using TypeD.ViewModel;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.DTO;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Interfaces;
using TypeDCore.Components;
using TypeD.Models.Providers.Interfaces;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Commands
{
    internal class CreateDrawableTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        IComponentProvider ComponentProvider { get; set; }
        IHookModel HookModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        private readonly IDisposable _registration;

        // Constructors
        public CreateDrawableTypeCommand(Project project, IResourceModel resourceModel) : base(resourceModel)
        {
            TypeDCoreProjectModel = ResourceModel.Get<ITypeDCoreProjectModel>();
            ComponentProvider = ResourceModel.Get<IComponentProvider>();
            HookModel = ResourceModel.Get<IHookModel>();
            CommandHistoryModel = ResourceModel.Get<ICommandHistoryModel>();
            _registration = ResourceModel.Get<IEditorCommandRegistry>().Register(
                "component.create-drawable",
                "Create drawable type",
                "Create a new drawable component type.",
                "global",
                string.Empty,
                this,
                () => new CreateComponentCommandData(project, "Drawables"));
        }

        public void Dispose() => _registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            var options = await ResourceModel.Get<IEditorDialogModel>().CreateComponentAsync(parameter.Project, parameter.Namespace, CoreComponent.DrawableComponent());
            if (options != null)
            {
                if (!HookModel.Shoot(new CreateComponentHook(new CreateComponentDTO()
                {
                    Name = options.Name,
                    Namespace = options.Namespace,
                    ParentComponent = options.ParentComponent
                })).Handled)
                {
                    if (options.BaseType == typeof(Drawable).FullName)
                    {
                        var className = options.Name;
                        var @namespace = options.Namespace;
                        var parent = options.ParentComponent;
                        var icon = options.Icon;
                        var component = TypeDCoreProjectModel.CreateDrawable(
                            parameter.Project, className, @namespace, parent, icon);
                        CommandHistoryModel.Record(
                            "Create drawable type",
                            $"Created {component.FullName} based on {parent?.FullName ?? "an unspecified base type"}.",
                            () => ComponentProvider.Delete(parameter.Project, component),
                            () => component = TypeDCoreProjectModel.CreateDrawable(
                                parameter.Project, className, @namespace, parent, icon));
                    }
                }
            }
        }
    }
}
