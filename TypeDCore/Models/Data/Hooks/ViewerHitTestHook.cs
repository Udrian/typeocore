using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Asks the loaded viewer module which component is displayed at a position, in viewer coordinates.
    /// </summary>
    public class ViewerHitTestHook : Hook
    {
        /// <summary>
        /// The project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }
        /// <summary>
        /// The horizontal viewer coordinate to test.
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// The vertical viewer coordinate to test.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Set by the handler to the topmost component at the position, or left null if there is none.
        /// </summary>
        public Component Result { get; set; }

        /// <summary>
        /// When set, the handler only reports the viewer position of this component in OriginX and OriginY.
        /// </summary>
        public Component Target { get; set; }
        /// <summary>
        /// The target component's horizontal origin, when a target is specified.
        /// </summary>
        public double OriginX { get; set; }
        /// <summary>
        /// The target component's vertical origin, when a target is specified.
        /// </summary>
        public double OriginY { get; set; }

        /// <summary>
        /// Creates a viewer hit-test request.
        /// </summary>
        public ViewerHitTestHook() { }
    }
}
