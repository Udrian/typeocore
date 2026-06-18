using Avalonia.Media.Imaging;
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
    }
}
