using TypeD.Models.Data;

namespace TypeDCore.Models.Data.SettingContexts
{
    /// <summary>Per-user custom property creation preferences for a project.</summary>
    [Name("PropertyCreation")]
    public class PropertyCreationSettingContext : SettingContext
    {
        /// <summary>Whether to restore the last property creation fields.</summary>
        public Setting<bool> RememberFields { get; set; } = new(true);
        /// <summary>Last selected property type.</summary>
        public Setting<string> TypeName { get; set; } = new(null);
        /// <summary>Last default value serialized using its property type.</summary>
        public Setting<string> DefaultValueJson { get; set; } = new(null);
        /// <summary>Last description.</summary>
        public Setting<string> Description { get; set; } = new(null);
        /// <summary>Last property group.</summary>
        public Setting<string> Group { get; set; } = new("General");
        /// <summary>Last access modifier.</summary>
        public Setting<CustomPropertyAccess> Access { get; set; } = new(CustomPropertyAccess.Public);

        /// <summary>Initializes preferences saved when the editor exits.</summary>
        public PropertyCreationSettingContext() { SaveOnExit = true; }
    }
}
