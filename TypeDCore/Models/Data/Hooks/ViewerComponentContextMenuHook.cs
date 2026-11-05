using TypeD.Models.Data.Hooks;
using TypeOEngine.Typedeaf.Core.Common;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>A component context menu opened at a world position in the viewer.</summary>
    public class ViewerComponentContextMenuHook : ComponentContextMenuHook
    {
        /// <summary>Gets or sets the paste location in viewer world coordinates.</summary>
        public Vec2 Position { get; set; }
    }
}
