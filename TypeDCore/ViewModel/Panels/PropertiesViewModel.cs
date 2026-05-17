using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Markup.Xaml;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeDCore.View.Panels;
using TypeD.Models.Data.SaveContexts;
using TypeOEngine.Typedeaf.Core.Common;

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
            public virtual object Value
            {
                get
                {
                    return Property.Value;
                }
                set
                {
                    if(!Property.Value.Equals(value))
                    {
                        Property.Value = value;
                        ValueChanged(this);
                    }
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

        public class BooleanPropertyNode : PropertyNode
        {
            public BooleanPropertyNode(Component component, Property property, Action<PropertyNode> value_changed) : base(component, property, value_changed) { }

            public override object Value {
                get
                {
                    if(base.Value is bool boolValue)
                    {
                        return boolValue;
                    }
                    return false;
                }
                set
                {
                    if(value is bool boolValue)
                    {
                        base.Value = boolValue;
                    }
                    else if(bool.TryParse(value?.ToString(), out bool parsedValue))
                    {
                        base.Value = parsedValue;
                    }
                }
            }
        }

        public class StringPropertyNode : PropertyNode
        {
            public StringPropertyNode(Component component, Property property, Action<PropertyNode> value_changed) : base(component, property, value_changed) { }

            public override object Value
            {
                get
                {
                    if (base.Value is string stringValue)
                    {
                        return stringValue;
                    }
                    return "";
                }
                set
                {
                    if (value is string stringValue)
                    {
                        base.Value = stringValue;
                    }
                    else
                    {
                        base.Value = value?.ToString() ?? "";
                    }
                }
            }
        }

        public class Int32PropertyNode : PropertyNode
        {
            public Int32PropertyNode(Component component, Property property, Action<PropertyNode> value_changed) : base(component, property, value_changed) { }

            public override object Value
            {
                get
                {
                    if (base.Value is int intValue)
                    {
                        return intValue;
                    }
                    return 0;
                }
                set
                {
                    if (value is int intValue)
                    {
                        base.Value = intValue;
                    }
                    else if (int.TryParse(value?.ToString(), out int parsedValue))
                    {
                        base.Value = parsedValue;
                    }
                }
            }
        }

        public class DoublePropertyNode : PropertyNode
        {
            public DoublePropertyNode(Component component, Property property, Action<PropertyNode> value_changed) : base(component, property, value_changed) { }

            public override object Value
            {
                get
                {
                    if (base.Value is double doubleValue)
                    {
                        return doubleValue;
                    }
                    return 0.0;
                }
                set
                {
                    if (value is double doubleValue)
                    {
                        base.Value = doubleValue;
                    }
                    else if (double.TryParse(value?.ToString(), out double parsedValue))
                    {
                        base.Value = parsedValue;
                    }
                }
            }
        }

        public class Vec2PropertyNode : PropertyNode
        {
            public Vec2PropertyNode(Component component, Property property, Action<PropertyNode> value_changed) : base(component, property, value_changed) { }

            public override object Value
            {
                get
                {
                    if (base.Value is Vec2 vec2Value)
                    {
                        return vec2Value;
                    }
                    return new Vec2(0, 0);
                }
                set
                {
                    if (value is Vec2 vec2Value)
                    {
                        base.Value = vec2Value;
                    }
                    else if (value != null)
                    {
                        var parts = value.ToString().Split(',');
                        if (parts.Length == 2 &&
                            double.TryParse(parts[0], out double x) &&
                            double.TryParse(parts[1], out double y))
                        {
                            base.Value = new Vec2(x, y);
                        }
                    }
                }
            }

            public double ValueX
            {
                get
                {
                    if (Value is Vec2 vec2)
                    {
                        return vec2.X;
                    }

                    return 0;
                }
                set
                {
                    Vec2 val = new Vec2(value, 0);
                    val.Y = Value is Vec2 vec2 ? vec2.Y : 0;
                    Value = val;
                }
            }

            public double ValueY
            {
                get
                {
                    if (Value is Vec2 vec2)
                    {
                        return vec2.Y;
                    }

                    return 0;
                }
                set
                {
                    Vec2 val = new Vec2(0, value);
                    val.X = Value is Vec2 vec2 ? vec2.X : 0;
                    Value = val;
                }
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
        public Dictionary<Type, Type> TypeToPropertyNode { get; set; }

        private PropertiesPanel PropertiesPanel { get; set; }

        // Constructors
        public PropertiesViewModel(PropertiesPanel element, Project project) : base(element)
        {
            PropertiesPanel = element;
            Project = project;
            Converter = new TypeToDataTemplateConverter();
            TypeToPropertyNode = new Dictionary<Type, Type>();
            PropertiesPanel.Resources["TypeToDataTemplateConverter"] = Converter;

            RegisterTypeTemplates();

            HookModel = ResourceModel.Get<IHookModel>();
            SaveModel = ResourceModel.Get<ISaveModel>();
            ProjectModel = ResourceModel.Get<IProjectModel>();

            PropertyNodes = new ObservableCollection<PropertyNode>();

            HookModel.AddHook<ComponentFocusHook>((hook) =>
            {
                if(Component == hook.Component)
                    return;
                Component = hook.Component;
                PropertyNodes.Clear();
                foreach(var property in Component.Properties)
                {
                    var propertyNodeType = TypeToPropertyNode.ContainsKey(property.Type) ? TypeToPropertyNode[property.Type] : typeof(PropertyNode);
                    PropertyNodes.Add((PropertyNode)Activator.CreateInstance(propertyNodeType, Component, property, (Action<PropertyNode>)(node =>
                    {
                        Component componentToSave = node.Component.ParentComponent ?? node.Component;
                        var context = SaveModel.GetSaveContext<ComponentSaveContext>(Project);
                        if(!context.Components.Contains(componentToSave))
                        {
                            context.Components.Add(componentToSave);
                            SaveModel.AddSave<ComponentSaveContext>();
                            ProjectModel.SaveCode(componentToSave.Template.Code);
                        }
                    })));
                }
            });

            HookModel.AddHook<CloseComponentHook>((hook) =>
            {
                Component = null;
                PropertyNodes.Clear();
            });
        }

        public void RegisterTypeTemplates()
        {
            //TODO: this should be handled in a way so that plugins can also register templates.

            // Load templates from URIs
            RegisterPropertyTemplate<bool, BooleanPropertyNode>("BooleanPropertyTemplate", "avares://TypeDCore/View/Panels/PropertyTemplates/BooleanPropertyTemplate.axaml");
            var stringTemplate = RegisterPropertyTemplate<string, StringPropertyNode>("StringPropertyTemplate", "avares://TypeDCore/View/Panels/PropertyTemplates/StringPropertyTemplate.axaml");
            RegisterPropertyTemplate<int, Int32PropertyNode>("Int32PropertyTemplate", "avares://TypeDCore/View/Panels/PropertyTemplates/Int32PropertyTemplate.axaml");
            RegisterPropertyTemplate<double, DoublePropertyNode>("DoublePropertyTemplate", "avares://TypeDCore/View/Panels/PropertyTemplates/DoublePropertyTemplate.axaml");
            RegisterPropertyTemplate<Vec2, Vec2PropertyNode>("Vec2PropertyTemplate", "avares://TypeDCore/View/Panels/PropertyTemplates/Vec2PropertyTemplate.axaml");

            // Fallback: use StringPropertyTemplate as default if available.
            if (stringTemplate != null)
                Converter.DefaultTemplate = stringTemplate;
        }

        private IDataTemplate RegisterPropertyTemplate<T, P>(string key, string uriString) where P : PropertyNode
        {
            var uri = new Uri(uriString);
            var resourceDict = (IResourceDictionary)AvaloniaXamlLoader.Load(uri);
            if (resourceDict.TryGetResource(key, null, out var template) && template is IDataTemplate dataTemplate)
            {
                Converter.Register(typeof(T), dataTemplate);
                TypeToPropertyNode[typeof(T)] = typeof(P);
                return dataTemplate;
            }
            return null;
        }
    }
}
