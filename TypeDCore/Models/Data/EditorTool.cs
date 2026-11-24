namespace TypeDCore.Models.Data
{
    /// <summary>
    /// The tool currently used to interact with the viewer.
    /// </summary>
    public enum EditorTool
    {
        /// <summary>
        /// Selects components.
        /// </summary>
        Selection,
        /// <summary>
        /// Moves the selected component.
        /// </summary>
        Move,
        /// <summary>
        /// Resizes the selected component.
        /// </summary>
        Resize,
        /// <summary>
        /// Changes the origin point of the selected component.
        /// </summary>
        Origin,
        /// <summary>
        /// Rotates the selected component around its origin.
        /// </summary>
        Rotate
    }
}

