using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Notifies listeners that a TypeO object was removed from a viewer.
    /// </summary>
    public class TypeOObjectRemovedFromViewHook : Hook
    {
        /// <summary>
        /// The engine context containing the object.
        /// </summary>
        public Context Context { get; set; }
        /// <summary>
        /// The object removed from the viewer.
        /// </summary>
        public TypeOObject TypeOObject { get; set; }
        /// <summary>
        /// The component represented by the object.
        /// </summary>
        public Component Component { get; set; }

        /// <summary>
        /// Creates an object-removed notification.
        /// </summary>
        public TypeOObjectRemovedFromViewHook() { }
    }
}
