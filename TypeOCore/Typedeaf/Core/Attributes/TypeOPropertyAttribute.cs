namespace TypeOEngine.Typedeaf.Core.Attributes
{
    /// <summary>
    /// Attribute to mark properties that should be picked up by the engine editor.
    /// </summary>
    /// <remarks>
    /// Use <c>[TypeOProperty("Description", Group = "Transform")]</c> to place a property
    /// under a named heading in the editor. Properties without a group use General.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class TypeOPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        public string Description { get; }

        /// <summary>Gets or sets the editor property group. Defaults to General.</summary>
        public string Group { get; set; }

        /// <summary>Gets or sets the default value associated with the property.</summary>
        public object DefaultValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeOPropertyAttribute"/> class.
        /// </summary>
        /// <param name="description">The description of the property.</param>
        /// <param name="defaultValue">The default value associated with the property.</param>
        public TypeOPropertyAttribute(string description = null, object defaultValue = null)
            : this(description, defaultValue, "General")
        {
        }

        /// <summary>Marks an editable property and assigns it to an editor group.</summary>
        /// <param name="description">The description of the property.</param>
        /// <param name="defaultValue">The default value associated with the property.</param>
        /// <param name="group">The editor property group.</param>
        public TypeOPropertyAttribute(string description, object defaultValue, string group)
        {
            Description = description;
            Group = group;
            if(defaultValue != null)
            {
                DefaultValue = defaultValue;
            }
        }
    }
}