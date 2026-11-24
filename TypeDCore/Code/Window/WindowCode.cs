using System;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Window
{
    /// <summary>
    /// Code generation for the window <see cref="Component"/>. A window has no class of its own; the game creates it
    /// from the properties set on the window node.
    /// </summary>
    public partial class WindowCode : ComponentTypeCode
    {
        // Properties
        /// <summary>
        /// Gets the engine type that describes a window.
        /// </summary>
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Engine.Graphics.Window); } }

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowCode"/> class.
        /// </summary>
        /// <param name="component">The window component.</param>
        public WindowCode(Component component) : base(component)
        {
        }

        /// <inheritdoc/>
        protected override void InitClass()
        {
        }

        /// <inheritdoc/>
        protected override void InitTypeDClass()
        {
        }
    }
}
