using TypeD.Commands;
using System.Text.Json;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Components;
using TypeDCore.Models.Data.DTO;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data.SettingContexts;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.Commands
{
    internal class CreateComponentTypeCommand : CustomCommand<CreateComponentCommandData>, IDisposable
    {
        private readonly IDisposable registration;

        public CreateComponentTypeCommand(Project project, IResourceModel resources) : base(resources)
        {
            registration = resources.Get<IEditorCommandRegistry>().Register(
                "component.create", "Create component", "Create a new component type.", "global", string.Empty,
                this, () => new CreateComponentCommandData(project, null));
        }

        public void Dispose() => registration.Dispose();

        public override async void Execute(CreateComponentCommandData parameter)
        {
            await ExecuteAsync(parameter, parameter.BaseComponent, ResourceModel);
        }

        internal static async Task ExecuteAsync(CreateComponentCommandData parameter, Component initialRoot, IResourceModel resources)
        {
            var options = await resources.Get<IEditorDialogModel>().CreateComponentAsync(parameter.Project, parameter.Namespace, initialRoot);
            if (options == null)
                return;

            var model = resources.Get<ITypeDCoreProjectModel>();
            Func<List<CustomProperty>, Component> createBase;
            string kind;
            if (options.BaseType == typeof(Entity).FullName)
            {
                kind = "entity";
                createBase = custom => model.CreateEntity(parameter.Project, options.Name, options.Namespace,
                    options.ParentComponent, options.Updatable, options.Drawable, options.Icon, custom);
            }
            else if (options.BaseType == typeof(Scene).FullName)
            {
                kind = "scene";
                createBase = custom => model.CreateScene(parameter.Project, options.Name, options.Namespace, options.ParentComponent, options.Icon, custom);
            }
            else if (options.BaseType == typeof(Drawable).FullName)
            {
                kind = "drawable";
                createBase = custom => model.CreateDrawable(parameter.Project, options.Name, options.Namespace, options.ParentComponent, options.Icon, custom);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported component root type '{options.BaseType}'.");
            }

            var propertyDefinitions = options.CustomProperties.Count == 0
                ? Array.Empty<IPropertyDefinition>() : resources.Get<IPropertyModel>().List().ToArray();
            var properties = options.CustomProperties.Select(property =>
            {
                var definition = propertyDefinitions.SingleOrDefault(item =>
                    item.Type.FullName == property.TypeName && item.Type == item.ValueType);
                if (definition == null)
                    throw new InvalidOperationException($"Property type '{property.TypeName}' is no longer available.");
                return (Property: new CustomProperty(property), Type: definition.Type,
                    Value: JsonSerializer.SerializeToElement(property.DefaultValue, definition.Type, JSON.CreateOptions()));
            }).ToList();
            var preferenceJson = options.PropertyPreference is { RememberFields: true } preference
                ? JsonSerializer.Serialize(preference.Property.DefaultValue, preference.Type, JSON.CreateOptions()) : null;

            if (kind != "scene" && resources.Get<IHookModel>().Shoot(new CreateComponentHook(new CreateComponentDTO
                {
                    Name = options.Name,
                    Namespace = options.Namespace,
                    ParentComponent = options.ParentComponent,
                    CustomProperties = properties.Select(item => new CustomProperty(item.Property)).ToList()
                })).Handled)
                return;

            Component create()
            {
                var copies = properties.Select(item =>
                {
                    return new CustomProperty(item.Property)
                    {
                        DefaultValue = item.Value.Deserialize(item.Type, JSON.CreateOptions())
                    };
                }).ToList();
                return createBase(copies);
            }

            var component = create();
            resources.Get<ICommandHistoryModel>().Record(
                $"Create {kind} type",
                $"Created {component.FullName} based on {options.ParentComponent?.FullName ?? "an unspecified base type"}.",
                () => resources.Get<IComponentProvider>().Delete(parameter.Project, component),
                () => component = create());
            var settings = resources.Get<ISettingModel>();
            var creationSettings = settings.GetContext<ComponentCreationSettingContext>(SettingLevel.User);
            creationSettings.RememberFields.Value = options.RememberFields;
            if (options.RememberFields)
            {
                creationSettings.LastBaseTypes.Value[options.BaseType] = options.ParentComponent.FullName;
                creationSettings.RootType.Value = options.BaseType;
                creationSettings.Namespace.Value = options.Namespace;
                creationSettings.Icon.Value = options.Icon;
                if (options.BaseType == typeof(Entity).FullName)
                {
                    creationSettings.Updatable.Value = options.Updatable;
                    creationSettings.Drawable.Value = options.Drawable;
                }
            }
            settings.SetContext(creationSettings);
            if (options.PropertyPreference is { } propertyPreference)
                AddCustomPropertyCommand.SavePreference(resources, propertyPreference.Property, propertyPreference.Type,
                    propertyPreference.RememberFields, preferenceJson);
        }
    }
}
