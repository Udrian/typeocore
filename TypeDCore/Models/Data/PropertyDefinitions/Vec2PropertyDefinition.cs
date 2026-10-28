using System;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class Vec2PropertyNode : PropertyNode
    {
        public Vec2PropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public override void RefreshValue()
        {
            base.RefreshValue();
            OnPropertyChanged(nameof(ValueX));
            OnPropertyChanged(nameof(ValueY));
        }

        public override object Value
        {
            get
            {
                if (base.Value is Vec2 vec2Value)
                    return vec2Value;
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
            get => Value is Vec2 vec2 ? vec2.X : 0;
            set
            {
                Vec2 val = new Vec2(value, 0);
                val.Y = Value is Vec2 vec2 ? vec2.Y : 0;
                Value = val;
            }
        }

        public double ValueY
        {
            get => Value is Vec2 vec2 ? vec2.Y : 0;
            set
            {
                Vec2 val = new Vec2(0, value);
                val.X = Value is Vec2 vec2 ? vec2.X : 0;
                Value = val;
            }
        }
    }

    public class Vec2PropertyDefinition : PropertyDefinition<Vec2, Vec2PropertyNode>
    {
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/Vec2PropertyTemplate.axaml";
        protected override string TemplateKey => "Vec2PropertyTemplate";

        public override string ToCode(Vec2 value)
        {
            return $"new {typeof(Vec2).FullName}({DoublePropertyDefinition.DoubleToCode(value.X)}, {DoublePropertyDefinition.DoubleToCode(value.Y)})";
        }
    }
}
