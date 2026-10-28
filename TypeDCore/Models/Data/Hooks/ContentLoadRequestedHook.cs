using System;
using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Fired when a content property was assigned a content path and the content should be loaded into the live object.
    /// Loading is left to the handler, since it needs to happen where the engine's graphics context is bound.
    /// </summary>
    public class ContentLoadRequestedHook : Hook
    {
        public Component Component { get; set; }
        public Property Property { get; set; }

        /// <summary>
        /// Path to the content, relative to the project's content folder. Null or empty clears the content.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// How the content is loaded, or null if the path has no association.
        /// </summary>
        public ContentTypeAssociation Association { get; set; }

        /// <summary>
        /// Called by the handler that loads the content, with the loaded content, once it has been assigned to the live object.
        /// </summary>
        public Action<object> Loaded { get; set; }

        public ContentLoadRequestedHook() { }
    }
}
