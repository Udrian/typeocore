using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.SettingContexts
{
    /// <summary>Persistent settings for the component viewer.</summary>
    [Name("Viewer")]
    public class ViewerSettingContext : SettingContext
    {
        /// <summary>Gets or sets the background color of the editor scene.</summary>
        public Setting<Color> ClearColor { get; set; } = new Setting<Color>(new Color(28, 3, 51));

        /// <summary>Initializes viewer settings, saved when the editor exits.</summary>
        public ViewerSettingContext()
        {
            SaveOnExit = true;
        }
    }
}
