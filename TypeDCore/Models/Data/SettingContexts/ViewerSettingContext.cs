using TypeD.Models.Data;
using TypeDCore.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.SettingContexts
{
    /// <summary>Persistent settings for the component viewer.</summary>
    [Name("Viewer")]
    public class ViewerSettingContext : SettingContext
    {
        /// <summary>Gets or sets the background color of the editor scene.</summary>
        public Setting<Color> ClearColor { get; set; } = new Setting<Color>(new Color(28, 3, 51));

        /// <summary>Gets or sets whether bounding boxes are drawn in the viewer.</summary>
        public Setting<bool> ShowBoundingBoxes { get; set; } = new Setting<bool>(true);

        /// <summary>Gets or sets which bounding boxes are drawn in the viewer.</summary>
        public Setting<BoundingBoxDisplayMode> BoundingBoxDisplay { get; set; } = new Setting<BoundingBoxDisplayMode>(BoundingBoxDisplayMode.All);

        /// <summary>Gets or sets the color of selected bounding boxes.</summary>
        public Setting<Color> SelectedBoundingBoxColor { get; set; } = new Setting<Color>(Color.Green);

        /// <summary>Gets or sets the color of unselected bounding boxes.</summary>
        public Setting<Color> UnselectedBoundingBoxColor { get; set; } = new Setting<Color>(Color.Red);

        /// <summary>Gets or sets the color of the bounding boxes of parents of selected nodes.</summary>
        public Setting<Color> ParentBoundingBoxColor { get; set; } = new Setting<Color>(Color.Yellow);

        /// <summary>Gets or sets the color of the bounding boxes of children of selected nodes.</summary>
        public Setting<Color> ChildBoundingBoxColor { get; set; } = new Setting<Color>(Color.Cyan);

        /// <summary>Initializes viewer settings, saved when the editor exits.</summary>
        public ViewerSettingContext()
        {
            SaveOnExit = true;
        }
    }
}
