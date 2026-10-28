using System;
using System.Globalization;
using TypeD.Models.Data;
using TypeD.ViewModel;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class Int32PropertyNode : PropertyNode
    {
        public Int32PropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public override object Value
        {
            get
            {
                if (base.Value is int intValue)
                    return intValue;
                return 0;
            }
            set
            {
                if (value is int intValue)
                    base.Value = intValue;
                else if (int.TryParse(value?.ToString(), out int parsedValue))
                    base.Value = parsedValue;
            }
        }
    }

    public class Int32PropertyDefinition : PropertyDefinition<int, Int32PropertyNode>
    {
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/Int32PropertyTemplate.axaml";
        protected override string TemplateKey => "Int32PropertyTemplate";

        public override string ToCode(int value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
