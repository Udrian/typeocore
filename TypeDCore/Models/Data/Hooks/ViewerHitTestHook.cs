using TypeD.Models.Data;

namespace TypeDCore.Models.Data.Hooks
{
    /// <summary>
    /// Asks the loaded viewer module which component is displayed at a position, in viewer coordinates.
    /// </summary>
    public class ViewerHitTestHook : Hook
    {
        public Project Project { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// Set by the handler to the topmost component at the position, or left null if there is none.
        /// </summary>
        public Component Result { get; set; }

        /// <summary>
        /// When set, the handler only reports the viewer position of this component in OriginX and OriginY.
        /// </summary>
        public Component Target { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }

        public ViewerHitTestHook() { }
    }
}
