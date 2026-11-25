using System;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.DrawStack
{
    /// <summary>
    /// Code generation for the draw stack <see cref="Component"/>. A draw stack has no class of its own; the scene
    /// creates it from the properties set on the draw stack node.
    /// </summary>
    public partial class DrawStackCode : ComponentTypeCode
    {
        // Properties
        /// <summary>
        /// Gets the engine type that describes a draw stack.
        /// </summary>
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Engine.DrawStack); } }

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawStackCode"/> class.
        /// </summary>
        /// <param name="component">The draw stack component.</param>
        public DrawStackCode(Component component) : base(component)
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
