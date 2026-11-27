using TypeD.Models.Data;

namespace TypeDCore.Models.Data.SettingContexts
{
    /// <summary>Per-user component creation preferences for a project.</summary>
    [Name("ComponentCreation")]
    public class ComponentCreationSettingContext : SettingContext
    {
        /// <summary>Last successfully used base component name for each root type.</summary>
        public Setting<Dictionary<string, string>> LastBaseTypes { get; set; } = new(new Dictionary<string, string>());
        /// <summary>Whether to restore the last component creation fields.</summary>
        public Setting<bool> RememberFields { get; set; } = new(true);
        /// <summary>Last selected root type.</summary>
        public Setting<string> RootType { get; set; } = new(null);
        /// <summary>Last selected namespace.</summary>
        public Setting<string> Namespace { get; set; } = new(null);
        /// <summary>Last explicitly selected icon, or null to inherit.</summary>
        public Setting<string> Icon { get; set; } = new(null);
        /// <summary>Last update capability selection.</summary>
        public Setting<bool> Updatable { get; set; } = new(true);
        /// <summary>Last draw capability selection.</summary>
        public Setting<bool> Drawable { get; set; } = new(true);

        /// <summary>Initializes creation preferences that are saved when the editor exits.</summary>
        public ComponentCreationSettingContext()
        {
            SaveOnExit = true;
        }
    }
}
