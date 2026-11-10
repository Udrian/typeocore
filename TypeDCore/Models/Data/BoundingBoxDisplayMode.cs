namespace TypeDCore.Models.Data
{
    /// <summary>
    /// Which bounding boxes are drawn in the viewer.
    /// </summary>
    public enum BoundingBoxDisplayMode
    {
        /// <summary>
        /// All bounding boxes.
        /// </summary>
        All,
        /// <summary>
        /// Only the bounding boxes of the selected nodes.
        /// </summary>
        Selected,
        /// <summary>
        /// The bounding boxes of the selected nodes and their parents.
        /// </summary>
        SelectedWithParents,
        /// <summary>
        /// The bounding boxes of the selected nodes, their parents and their children.
        /// </summary>
        SelectedWithParentsAndChildren
    }
}
