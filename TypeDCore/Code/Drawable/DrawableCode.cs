using System;
using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Drawable
{
    /// <summary>
    /// Represents a code generator for drawable components in TypeOEngine.
    /// </summary>
    /// <remarks>This class is responsible for generating code related to drawable components, including
    /// initialization, drawing, and cleanup methods. It extends the <see cref="ComponentTypeCode"/> base class and
    /// provides functionality specific to drawable entities.</remarks>
    public partial class DrawableCode : ComponentTypeCode
    {
        // Properties
        /// <summary>
        /// Gets the base type of the drawable entity.
        /// </summary>
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable); } }

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DrawableCode"/> class with the specified component.
        /// </summary>
        /// <remarks>The <see cref="DrawableCode"/> class represents a drawable code element and is
        /// initialized with a specific component. By default, the <see cref="Codalyzer.PartialClass"/> property is set to <see langword="false"/>.</remarks>
        /// <param name="component">The <see cref="Component"/> associated with this instance. Cannot be <see langword="null"/>.</param>
        public DrawableCode(Component component) : base(component)
        {
        }

        // Functions
        /// <summary>
        /// Initializes the class by configuring its state, adding necessary using directives, and defining the
        /// required functions for the component.
        /// </summary>
        /// <remarks>This method sets up the class based on whether it is a base component type or not. 
        /// It adds appropriate using directives and defines the `Initialize`, `Draw`, and `Cleanup`  functions with
        /// behavior tailored to the component type. For base component types, the functions are defined without
        /// additional base calls. For non-base component types, the functions include calls to their base
        /// implementations.</remarks>
        protected override void InitClass()
        {
            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces"
            });

            if (IsBaseComponentType)
            {
                AddFunction(new Function("protected virtual void InternalInitialize()", () => { }));
                AddFunction(new Function("public override void Draw(ICanvas canvas)", () => { }));
                AddFunction(new Function("protected override void Cleanup()", () => { }));
            }
            else
            {
                AddFunction(new Function("protected override void InternalInitialize()", () => {
                    Writer.AddLine("base.InternalInitialize();");
                }));
                AddFunction(new Function("public override void Draw(ICanvas canvas)", () => {
                    Writer.AddLine("base.Draw(canvas);");
                }));
                AddFunction(new Function("protected override void Cleanup()", () => {
                    Writer.AddLine("base.Cleanup();");
                }));
            }
        }
    }
}