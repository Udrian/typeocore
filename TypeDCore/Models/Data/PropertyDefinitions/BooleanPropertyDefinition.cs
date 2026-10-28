using System;
using TypeD.Models.Data;
using TypeD.ViewModel;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class BooleanPropertyNode : PropertyNode
    {
        public BooleanPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public override object Value
        {
            get
            {
                if (base.Value is bool boolValue)
                    return boolValue;
                return false;
            }
            set
            {
                if (value is bool boolValue)
                    base.Value = boolValue;
                else if (bool.TryParse(value?.ToString(), out bool parsedValue))
                    base.Value = parsedValue;
            }
        }
    }

    public class BooleanPropertyDefinition : PropertyDefinition<bool, BooleanPropertyNode>
    {
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/BooleanPropertyTemplate.axaml";
        protected override string TemplateKey => "BooleanPropertyTemplate";

        public override string ToCode(bool value) => value ? "true" : "false";
    }
}
