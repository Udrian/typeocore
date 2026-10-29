using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;
using TypeD.Models.Data;

namespace TypeDCore.Models.Data
{
    public class Texture2dContent : Content
    {
        public override void Initialize()
        {
            Thumbnail = new Bitmap(new MemoryStream(Data));
        }

        public override Control CreateToolTip()
        {
            var panel = CreateToolTipPanel();

            if (Thumbnail != null)
            {
                panel.Children.Add(new Border
                {
                    BorderBrush = new SolidColorBrush(Color.Parse("#FFCCCCCC")),
                    BorderThickness = new Thickness(1),
                    Background = new SolidColorBrush(Color.Parse("#FF404040")),
                    Child = new Image
                    {
                        Source = Thumbnail,
                        Stretch = Stretch.Uniform,
                        StretchDirection = StretchDirection.DownOnly,
                        MaxWidth = 400,
                        MaxHeight = 300
                    }
                });
            }

            AddToolTipRows(panel, GetToolTipProperties());
            return panel;
        }

        protected override List<KeyValuePair<string, string>> GetToolTipProperties()
        {
            var properties = new List<KeyValuePair<string, string>>
            {
                new("File", System.IO.Path.GetFileName(Path)),
                new("Folder", System.IO.Path.GetDirectoryName(Path))
            };

            if (Thumbnail != null)
            {
                var size = Thumbnail.PixelSize;
                var divisor = Gcd(size.Width, size.Height);
                properties.Add(new("Dimensions", $"{size.Width} x {size.Height} px"));
                if (divisor > 0)
                    properties.Add(new("Aspect ratio", $"{size.Width / divisor}:{size.Height / divisor}"));
                properties.Add(new("Power of two", IsPowerOfTwo(size.Width) && IsPowerOfTwo(size.Height) ? "Yes" : "No"));
                properties.Add(new("Pixels", $"{(long)size.Width * size.Height:N0}"));
                properties.Add(new("DPI", $"{Thumbnail.Dpi.X:0.##} x {Thumbnail.Dpi.Y:0.##}"));
                properties.Add(new("Uncompressed memory", FormatSize((long)size.Width * size.Height * 4)));
            }

            properties.Add(new("File size", FormatSize(Data?.LongLength ?? 0)));
            if (!string.IsNullOrEmpty(Path) && System.IO.File.Exists(Path))
                properties.Add(new("Modified", System.IO.File.GetLastWriteTime(Path).ToString("g")));

            return properties;
        }

        private static bool IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;

        private static int Gcd(int a, int b)
        {
            while (b != 0)
                (a, b) = (b, a % b);
            return a;
        }
    }
}
