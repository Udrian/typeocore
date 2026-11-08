using System;
using TypeD.Models.Data;
using TypeD.Models.Data.PropertyDefinitions;
using TypeD.ViewModel;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.PropertyDefinitions
{
    /// <summary>
    /// Exposes a two-dimensional vector property value and its coordinates to the property editor.
    /// </summary>
    public class Vec2PropertyNode : PropertyNode
    {
        /// <summary>
        /// Creates a node for a vector component property.
        /// </summary>
        public Vec2PropertyNode(Component component, Property property, Action<PropertyNode, object> value_changed) : base(component, property, value_changed) { }

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
        /// Gets or sets the property value as a two-dimensional vector.
        /// </summary>
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
                    ValidationError = null;
                }
                else if (value != null)
                {
                    var parts = value.ToString().Split(',');
                    if (parts.Length == 2 &&
                        double.TryParse(parts[0], out double x) &&
                        double.TryParse(parts[1], out double y))
                    {
                        base.Value = new Vec2(x, y);
                        ValidationError = null;
                    }
                    else
                    {
                        ValidationError = $"Invalid vector format. Expected 'X, Y' (e.g., '10.5, 20.3')";
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal coordinate.
        /// </summary>
        public double ValueX
        {
            get => Value is Vec2 vec2 ? vec2.X : 0;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    ValidationError = "Invalid X value: must be a valid number";
                    return;
                }

                Vec2 val = new Vec2(value, 0);
                val.Y = Value is Vec2 vec2 ? vec2.Y : 0;
                Value = val;
                ValidationError = null;
            }
        }

        /// <summary>
        /// Gets or sets the vertical coordinate.
        /// </summary>
        public double ValueY
        {
            get => Value is Vec2 vec2 ? vec2.Y : 0;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    ValidationError = "Invalid Y value: must be a valid number";
                    return;
                }

                Vec2 val = new Vec2(0, value);
                val.X = Value is Vec2 vec2 ? vec2.X : 0;
                Value = val;
                ValidationError = null;
            }
        }
    }

    /// <summary>
    /// Defines the editor and code representation for two-dimensional vector properties.
    /// </summary>
    public class Vec2PropertyDefinition : PropertyDefinition<Vec2, Vec2PropertyNode>
    {
        /// <summary>
        /// Gets the vector property editor template URI.
        /// </summary>
        protected override string TemplateUri => "avares://TypeDCore/View/Panels/PropertyTemplates/Vec2PropertyTemplate.axaml";
        /// <summary>
        /// Gets the vector property editor template key.
        /// </summary>
        protected override string TemplateKey => "Vec2PropertyTemplate";

        /// <summary>
        /// Converts a vector to a C# expression containing its coordinates.
        /// </summary>
        public override string ToCode(Vec2 value)
        {
            return $"new {typeof(Vec2).FullName}({DoublePropertyDefinition.DoubleToCode(value.X)}, {DoublePropertyDefinition.DoubleToCode(value.Y)})";
        }
    }
}
