using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Fired when the user double clicks a component in the viewer. A module that handles the double click sets
    /// <see cref="Handled"/>, which stops the other handlers from acting on it and the viewer from starting a drag.
    /// Context menu items are added through <see cref="TypeD.Models.Data.Hooks.ComponentContextMenuHook"/>.
    /// </summary>
    public class ViewerDoubleClickHook : Hook
    {
        /// <summary>
        /// The project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }
        /// <summary>
        /// The component that was double clicked.
        /// </summary>
        public Component Component { get; set; }
        /// <summary>
        /// Gets or sets whether a handler acted on the double click.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Creates a viewer double click notification.
        /// </summary>
        public ViewerDoubleClickHook() { }
    }
}
