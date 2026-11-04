using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Asks the loaded viewer module which component is displayed at a position, in world coordinates of the viewer (viewer coordinates converted with its camera).
    /// </summary>
    public class ViewerHitTestHook : Hook
    {
        /// <summary>
        /// The project displayed in the viewer.
        /// </summary>
        public Project Project { get; set; }
        /// <summary>
        /// The horizontal world coordinate to test.
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// The vertical world coordinate to test.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Set by the handler to the topmost component at the position, or left null if there is none.
        /// </summary>
        public Component Result { get; set; }

        /// <summary>
        /// When set, the handler only reports the world position and bounds of this component.
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
        /// Set by the handler when the target component is displayed, together with its bounds.
        /// </summary>
        public bool TargetFound { get; set; }
        /// <summary>
        /// The left world coordinate of the displayed area of the target component.
        /// </summary>
        public double BoundsX { get; set; }
        /// <summary>
        /// The top world coordinate of the displayed area of the target component.
        /// </summary>
        public double BoundsY { get; set; }
        /// <summary>
        /// The width of the displayed area of the target component.
        /// </summary>
        public double BoundsWidth { get; set; }
        /// <summary>
        /// The height of the displayed area of the target component.
        /// </summary>
        public double BoundsHeight { get; set; }

        /// <summary>
        /// Creates a viewer hit-test request.
        /// </summary>
        public ViewerHitTestHook() { }
    }
}
