using TypeD.Models.Data;

namespace TypeDCore.Models
{
    /// <summary>
    /// What is currently being dragged from a panel towards the viewer or a property. The drag and drop runs in-process,
    /// so the payload is shared here instead of being serialized into the drag data.
    /// </summary>
    internal static class ViewerDragPayload
    {
        /// <summary>
        /// A component type dragged from the component browser.
        /// </summary>
        public static Component Component { get; private set; }

        /// <summary>
        /// The full path of a content file dragged from the content panel.
        /// </summary>
        public static string ContentPath { get; private set; }

        public static bool HasPayload => Component != null || ContentPath != null;

        public static void SetComponent(Component component)
        {
            Clear();
            Component = component;
        }

        public static void SetContent(string path)
        {
            Clear();
            ContentPath = path;
        }

        public static void Clear()
        {
            Component = null;
            ContentPath = null;
        }
    }
}
