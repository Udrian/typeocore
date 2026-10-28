using System;
using System.Text.Json.Serialization;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class ColorPropertyNode : PropertyNode
    {
        public ColorPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public override void RefreshValue()
        {
            base.RefreshValue();
            OnPropertyChanged(nameof(AvaloniaColor));
        }

        public override object Value
        {
            get
            {
                if (base.Value is Color colorValue)
                    return colorValue;
                return new Color(255, 0, 0, 0);
            }
            set
            {
                if (value is Color colorValue)
                    base.Value = colorValue;
            }
        }

        public Avalonia.Media.Color AvaloniaColor
        {
            get
            {
                var color = (Color)Value;
                return Avalonia.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            set
            {
                var color = new Color(value.A, value.R, value.G, value.B);
                if (!color.Equals((Color)Value))
                    Value = color;
            }
        }
    }

    public class ColorPropertyDefinition : PropertyDefinition<Color, ColorPropertyNode>
    {
        private readonly JsonConverter _jsonConverter = new ColorJsonConverter();

        public override JsonConverter JsonConverter => _jsonConverter;

        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/ColorPropertyTemplate.axaml";
        protected override string TemplateKey => "ColorPropertyTemplate";

        public override string ToCode(Color value)
        {
            return $"new {typeof(Color).FullName}({value.A}, {value.R}, {value.G}, {value.B})";
        }
    }
}
