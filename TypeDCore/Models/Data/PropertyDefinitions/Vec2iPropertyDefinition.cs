using System;
using TypeD.Models.Data;
using TypeD.Models.Data.PropertyDefinitions;
using TypeD.ViewModel;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    /// <summary>
    /// Exposes a two-dimensional integer vector property value and its coordinates to the property editor.
    /// </summary>
    public class Vec2iPropertyNode : PropertyNode
    {
        /// <summary>
        /// Creates a node for an integer vector component property.
        /// </summary>
        public Vec2iPropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

        /// <summary>
        /// Refreshes the value and notifies bindings of both coordinates.
        /// </summary>
        public override void RefreshValue()
        {
            base.RefreshValue();
            ValidationError = null;
            OnPropertyChanged(nameof(ValueX));
            OnPropertyChanged(nameof(ValueY));
        }

        /// <summary>
        /// Gets or sets the property value as an integer vector.
        /// </summary>
        public override object Value
        {
            get => base.Value is Vec2i vec ? vec : new Vec2i(0, 0);
            set
            {
                if (value is Vec2i vec)
                {
                    base.Value = vec;
                    ValidationError = null;
                }
                else if (value != null)
                {
                    var parts = value.ToString().Split(',');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                    {
                        base.Value = new Vec2i(x, y);
                        ValidationError = null;
                    }
                    else
                    {
                        ValidationError = "Invalid vector format. Expected 'X, Y' (e.g., '1024, 768')";
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal coordinate.
        /// </summary>
        public int ValueX
        {
            get => Value is Vec2i vec ? vec.X : 0;
            set => Value = new Vec2i(value, ValueY);
        }

        /// <summary>
        /// Gets or sets the vertical coordinate.
        /// </summary>
        public int ValueY
        {
            get => Value is Vec2i vec ? vec.Y : 0;
            set => Value = new Vec2i(ValueX, value);
        }
    }

    /// <summary>
    /// Defines the editor and code representation for two-dimensional integer vector properties.
    /// </summary>
    public class Vec2iPropertyDefinition : PropertyDefinition<Vec2i, Vec2iPropertyNode>
    {
        /// <summary>
        /// Gets the vector property editor template URI.
        /// </summary>
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/Vec2iPropertyTemplate.axaml";
        /// <summary>
        /// Gets the vector property editor template key.
        /// </summary>
        protected override string TemplateKey => "Vec2iPropertyTemplate";

        /// <summary>
        /// Converts a vector to a C# expression containing its coordinates.
        /// </summary>
        public override string ToCode(Vec2i value)
        {
            return $"new {typeof(Vec2i).FullName}({value.X}, {value.Y})";
        }
    }
}
