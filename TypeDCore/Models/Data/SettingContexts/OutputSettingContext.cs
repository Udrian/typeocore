using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.SettingContexts
{
    /// <summary>Persistent settings for the Output panel's game log.</summary>
    [Name("Output")]
    public class OutputSettingContext : SettingContext
    {
        /// <summary>Gets or sets the color of Ludacris lines in the game log.</summary>
        public Setting<Color> LudacrisColor { get; set; } = new Setting<Color>(new Color(158, 158, 158));
        /// <summary>Gets or sets the color of Debug lines in the game log.</summary>
        public Setting<Color> DebugColor { get; set; } = new Setting<Color>(new Color(78, 201, 176));
        /// <summary>Gets or sets the color of Info lines in the game log.</summary>
        public Setting<Color> InfoColor { get; set; } = new Setting<Color>(new Color(212, 212, 212));
        /// <summary>Gets or sets the color of Warning lines in the game log.</summary>
        public Setting<Color> WarningColor { get; set; } = new Setting<Color>(new Color(255, 192, 46));
        /// <summary>Gets or sets the color of Error lines in the game log.</summary>
        public Setting<Color> ErrorColor { get; set; } = new Setting<Color>(new Color(248, 81, 73));
        /// <summary>Gets or sets the color of Fatal lines in the game log.</summary>
        public Setting<Color> FatalColor { get; set; } = new Setting<Color>(new Color(255, 0, 255));

        /// <summary>Initializes output settings, saved when the editor exits.</summary>
        public OutputSettingContext()
        {
            SaveOnExit = true;
        }
    }
}
