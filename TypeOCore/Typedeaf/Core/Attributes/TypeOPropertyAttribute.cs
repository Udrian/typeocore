namespace TypeOEngine.Typedeaf.Core.Attributes
{
    /// <summary>
    /// Attribute to mark properties that should be picked up by the engine editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TypeOPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        public string Description { get; }

        public object DefaultValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOPropertyAttribute"/> class.
        /// </summary>
        /// <param name="description">The description of the property.</param>
        public TypeOPropertyAttribute(string description = null, object defaultValue = null)
        {
            Description = description;
            if(defaultValue != null)
            {
                DefaultValue = defaultValue;
            }
        }
    }
}