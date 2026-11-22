using Avalonia.Controls;
using TypeD.Models.Data;

namespace TypeDCore.Models.Data
{
    /// <summary>
    /// Represents texture content with a bitmap thumbnail and image metadata.
    /// </summary>
    public class FontContent : Content
    {
        /// <summary>
        /// Creates a thumbnail bitmap from the content data.
        /// </summary>
        public override void Initialize()
        {
            //Thumbnail = new Bitmap(new MemoryStream(Data));
        }

        /// <summary>
        /// Creates a tooltip containing the texture thumbnail and metadata.
        /// </summary>
        public override Control CreateToolTip()
        {
            var panel = CreateToolTipPanel();

            AddToolTipRows(panel, GetToolTipProperties());
            return panel;
        }

        /// <summary>
        /// Gets the file and image metadata displayed in the tooltip.
        /// </summary>
        protected override List<KeyValuePair<string, string>> GetToolTipProperties()
        {
            var properties = new List<KeyValuePair<string, string>>
            {
                new("File", System.IO.Path.GetFileName(Path)),
                new("Folder", System.IO.Path.GetDirectoryName(Path))
            };

            properties.Add(new("File size", FormatSize(Data?.LongLength ?? 0)));
            if (!string.IsNullOrEmpty(Path) && System.IO.File.Exists(Path))
                properties.Add(new("Modified", System.IO.File.GetLastWriteTime(Path).ToString("g")));

            return properties;
        }
    }
}
