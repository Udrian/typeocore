using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Platform;
using Avalonia;
using Avalonia.Markup.Xaml;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.View.Panels;
using TypeD.Models.Data.SaveContexts;

namespace TypeDCore.ViewModel.Panels
{
    public class TypeToDataTemplateConverter : IValueConverter
    {
        private Dictionary<Type, IDataTemplate> TemplateMap { get; } = new Dictionary<Type, IDataTemplate>();

        public IDataTemplate? DefaultTemplate { get; set; }

        public void Register(Type type, IDataTemplate template)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (template == null) throw new ArgumentNullException(nameof(template));

            TemplateMap[type] = template;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            Type type = null;
            if (value is PropertiesViewModel.PropertyNode node)
            {
                type = node.Type ?? node.Value?.GetType();

            }

            if (type != null)
            {
                if (TemplateMap.TryGetValue(type, out var template))
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
        // Definitions
        public class PropertyNode : ViewModelBase
        {
            public Component Component { get; private set; }
            public Property Property { get; private set; }

            public string Name { get => Property.Name; }
            public object Value
            {
                get
                {
                    return Property.Value;
                }
                set
                {
                    Property.Value = value;
                    ValueChanged(this);
                }
            }
            public Type Type { get => Property.Type; }
            public string Description { get => Property.Description; }

            public Action<PropertyNode> ValueChanged;

            public PropertyNode(Component component, Property property, Action<PropertyNode> value_changed)
            {
                Component = component;
                Property = property;
                ValueChanged = value_changed;
            }
        }

        // Models
        public IHookModel HookModel { get; set; }
        public ISaveModel SaveModel { get; set; }
        public IProjectModel ProjectModel { get; set; }

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
            Converter = new TypeToDataTemplateConverter();
            PropertiesPanel.Resources["TypeToDataTemplateConverter"] = Converter;

            RegisterTypeTemplates();

            HookModel = ResourceModel.Get<IHookModel>();
            SaveModel = ResourceModel.Get<ISaveModel>();
            ProjectModel = ResourceModel.Get<IProjectModel>();

            PropertyNodes = new ObservableCollection<PropertyNode>();

            HookModel.AddHook<ComponentFocusHook>((hook) =>
            {
                Component = hook.Component;
                PropertyNodes.Clear();
                foreach(var property in Component.Properties)
                {
                    PropertyNodes.Add(new PropertyNode(Component, property, (node) =>
                    {
                        var context = SaveModel.GetSaveContext<ComponentSaveContext>(Project);
                        context.Components.Add(Component);
                        SaveModel.AddSave<ComponentSaveContext>();

                        ProjectModel.SaveCode(Component.Template.Code);
                    }));
                }
            });

            HookModel.AddHook<CloseComponentHook>((hook) =>
            {
                if (Component.FullName == hook.Component.FullName)
                {
                    Component = null;
                    PropertyNodes.Clear();
                }
            });
        }

        public void RegisterTypeTemplates()
        {
            //TODO: this should be handled in a way so that plugins can also register templates.

            // Load templates from URIs
            RegisterTemplateFromUri("avares://TypeDCore/View/Panels/PropertyTemplates/BooleanPropertyTemplate.axaml", "BooleanPropertyTemplate", typeof(bool));
            var stringTemplate = RegisterTemplateFromUri("avares://TypeDCore/View/Panels/PropertyTemplates/StringPropertyTemplate.axaml", "StringPropertyTemplate", typeof(string));
            RegisterTemplateFromUri("avares://TypeDCore/View/Panels/PropertyTemplates/Int32PropertyTemplate.axaml", "Int32PropertyTemplate", typeof(int));
            RegisterTemplateFromUri("avares://TypeDCore/View/Panels/PropertyTemplates/DoublePropertyTemplate.axaml", "DoublePropertyTemplate", typeof(double));

            // Fallback: use StringPropertyTemplate as default if available.
            if (stringTemplate != null)
                Converter.DefaultTemplate = stringTemplate;
        }

        private IDataTemplate RegisterTemplateFromUri(string uriString, string key, Type type)
        {
            var uri = new Uri(uriString);
            var resourceDict = (IResourceDictionary)AvaloniaXamlLoader.Load(uri);
            if (resourceDict.TryGetResource(key, null, out var template) && template is IDataTemplate dataTemplate)
            {
                Converter.Register(type, dataTemplate);
                return dataTemplate;
            }
            return null;
        }
    }
}
