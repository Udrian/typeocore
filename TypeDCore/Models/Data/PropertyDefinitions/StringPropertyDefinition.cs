using System;
using TypeD.Models.Data;
using TypeD.ViewModel;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class StringPropertyNode : PropertyNode
    {
        public StringPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public override object Value
        {
            get
            {
                if (base.Value is string stringValue)
                    return stringValue;
                return "";
            }
            set
            {
                if (value is string stringValue)
                    base.Value = stringValue;
                else
                    base.Value = value?.ToString() ?? "";
            }
        }
    }

    public class StringPropertyDefinition : PropertyDefinition<string, StringPropertyNode>
    {
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/StringPropertyTemplate.axaml";
        protected override string TemplateKey => "StringPropertyTemplate";

        public override string ToCode(string value)
        {
            var escaped = value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
            return $"\"{escaped}\"";
        }
    }
}
