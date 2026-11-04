using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>Notifies open viewers that their editor background color changed.</summary>
    public class ViewerClearColorChangedHook : Hook
    {
        /// <summary>Gets or sets the new background color.</summary>
        public Color Color { get; set; }
    }
}
