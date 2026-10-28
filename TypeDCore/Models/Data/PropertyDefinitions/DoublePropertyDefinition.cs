using System;
using System.Globalization;
using TypeD.Models.Data;
using TypeD.ViewModel;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class DoublePropertyNode : PropertyNode
    {
        public DoublePropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public override object Value
        {
            get
            {
                if (base.Value is double doubleValue)
                    return doubleValue;
                return 0.0;
            }
            set
            {
                if (value is double doubleValue)
                    base.Value = doubleValue;
                else if (double.TryParse(value?.ToString(), out double parsedValue))
                    base.Value = parsedValue;
            }
        }
    }

    public class DoublePropertyDefinition : PropertyDefinition<double, DoublePropertyNode>
    {
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/DoublePropertyTemplate.axaml";
        protected override string TemplateKey => "DoublePropertyTemplate";

        public override string ToCode(double value) => DoubleToCode(value);

        /// <summary>
        /// Creates a culture independent C# expression for a double.
        /// </summary>
        public static string DoubleToCode(double value)
        {
            if (double.IsNaN(value)) return "double.NaN";
            if (double.IsPositiveInfinity(value)) return "double.PositiveInfinity";
            if (double.IsNegativeInfinity(value)) return "double.NegativeInfinity";
            return value.ToString("R", CultureInfo.InvariantCulture);
        }
    }
}
