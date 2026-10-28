using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;
using TypeDCore.View.Panels;
using System.Linq;

namespace TypeDCore.ViewModel.Panels
{
    public class TypeToDataTemplateConverter : IValueConverter
    {
        private Dictionary<Type, IDataTemplate> TemplateCache { get; } = new Dictionary<Type, IDataTemplate>();

        /// <summary>
        /// Creates the template for a property type, or returns null if the type has no definition.
        /// </summary>
        public Func<Type, IDataTemplate?>? TemplateResolver { get; set; }

        public IDataTemplate? DefaultTemplate { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            Type type = null;
            if (value is PropertyNode node)
            {
                type = node.Type ?? node.Value?.GetType();
            }

            if (type != null)
            {
                if (!TemplateCache.TryGetValue(type, out var template))
                {
                    template = TemplateResolver?.Invoke(type);
                    if (template != null)
                        TemplateCache[type] = template;
                }
                if (template != null)
                    return template;
            }

            return DefaultTemplate;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }

    internal partial class PropertiesViewModel : ViewModelBase
    {
        // Models
        public IHookModel HookModel { get; set; }
        public IEditorCommandRegistry EditorCommandRegistry { get; set; }
        public IPropertyModel PropertyModel { get; set; }

        // Properties
        private Project Project { get; set; }
        private Component Component { get; set; }
        public ObservableCollection<PropertyNode> PropertyNodes { get; set; }
        public TypeToDataTemplateConverter Converter { get; set; }

        private PropertiesPanel PropertiesPanel { get; set; }

        // Constructors
        public PropertiesViewModel(PropertiesPanel element, Project project) : base(element)
        {
            PropertiesPanel = element;
            Project = project;

            HookModel = ResourceModel.Get<IHookModel>();
            EditorCommandRegistry = ResourceModel.Get<IEditorCommandRegistry>();
            PropertyModel = ResourceModel.Get<IPropertyModel>();

            Converter = new TypeToDataTemplateConverter
            {
                TemplateResolver = type => PropertyModel.Get(type)?.CreateTemplate(),
                // Fallback: edit unknown types as strings.
                DefaultTemplate = PropertyModel.Get(typeof(string))?.CreateTemplate()
            };
            PropertiesPanel.Resources["TypeToDataTemplateConverter"] = Converter;

            PropertyNodes = new ObservableCollection<PropertyNode>();

            HookModel.AddHook<ComponentFocusHook>((hook) =>
            {
                if(Component == hook.Component)
                    return;
                Component = hook.Component;
                PropertyNodes.Clear();
                foreach(var property in Component.Properties)
                {
                    Action<PropertyNode, object> valueChanged = (node, value) =>
                    {
                        var executed = EditorCommandRegistry.Execute("component.change-property", new ChangePropertyCommandData
                        {
                            Project = Project,
                            Property = node.Property,
                            Value = value
                        });
                        if (!executed)
                            throw new InvalidOperationException($"Could not execute the change-property command for '{node.Property.Name}'.");
                    };
                    var definition = PropertyModel.Get(property.Type);
                    PropertyNodes.Add(definition?.CreateNode(Component, property, valueChanged) ?? new PropertyNode(Component, property, valueChanged));
                }
            });

            HookModel.AddHook<PropertyChangedHook>((hook) =>
            {
                if (!ReferenceEquals(Component, hook.Component) || hook.Property == null)
                    return;

                var propertyNode = PropertyNodes.FirstOrDefault(node => ReferenceEquals(node.Property, hook.Property))
                    ?? PropertyNodes.FirstOrDefault(node => node.Property.Name == hook.Property.Name);
                propertyNode?.RefreshValue();
            });

            HookModel.AddHook<CloseComponentHook>((hook) =>
            {
                Component = null;
                PropertyNodes.Clear();
            });
        }
    }
}
