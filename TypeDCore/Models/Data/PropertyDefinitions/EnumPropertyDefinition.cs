using System;
using System.Linq;
using TypeD.Models.Data;
using TypeD.ViewModel;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    public class EnumPropertyNode : PropertyNode
    {
        public EnumPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        public Array Options => Type != null && Type.IsEnum ? Enum.GetValues(Type) : Array.Empty<object>();

        public override object Value
        {
            get => base.Value;
            set
            {
                // The ListBox sends null when the selection is cleared.
                if (value != null && Type.IsInstanceOfType(value))
                    base.Value = value;
            }
        }
    }

    /// <summary>
    /// Handles every enum type, registered under <see cref="Enum"/>.
    /// </summary>
    public class EnumPropertyDefinition : PropertyDefinition<Enum, EnumPropertyNode>
    {
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/EnumPropertyTemplate.axaml";
        protected override string TemplateKey => "EnumPropertyTemplate";

        public override string ToCode(Enum value)
        {
            var typeName = value.GetType().FullName.Replace('+', '.');
            var names = value.ToString().Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" | ", names.Select(name =>
                char.IsDigit(name[0]) || name[0] == '-'
                    ? $"({typeName}){name}"
                    : $"{typeName}.{name}"));
        }
    }
}
