using System;
using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Game
{
    /// <summary>
    /// Represents the game-specific <see cref="Component"/> type code within the TypeOEngine framework.
    /// </summary>
    /// <remarks>This class is a specialized implementation of <see cref="ComponentTypeCode"/> for handling
    /// game-related components. It provides functionality to initialize and manage game-specific behaviors and
    /// dependencies.</remarks>
    public partial class GameCode : ComponentTypeCode
    {
        // Properties
        /// <summary>
        /// Gets the base type of the object represented by this type.
        /// </summary>
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Game); } }

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="GameCode"/> class with the specified component.
        /// </summary>
        /// <param name="component">The <see cref="Component"/> associated with this instance of <see cref="GameCode"/>.</param>
        public GameCode(Component component) : base(component)
        {
        }

        /// <summary>
        /// Initializes the class by adding required namespaces and functions.
        /// </summary>
        /// <remarks>This method sets up the class by registering necessary using directives and
        /// predefined functions. It is intended to be called during the initialization phase of the derived
        /// class.</remarks>
        protected override void InitClass()
        {
            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core"
            });

            AddFunction(new Function("protected void InternalInitialize()", () => { }));
            AddFunction(new Function("protected void InternalCleanup()", () => { }));
        }
    }
}
