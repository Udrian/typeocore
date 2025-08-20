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
    public class DrawableCode : ComponentTypeCode
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
            PartialClass = false;
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
            PartialClass = false;

            if (IsBaseComponentType)
            {
                AddUsings(new List<string>()
                {
                    "TypeOEngine.Typedeaf.Core.Common",
                    "TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces"
                });

                AddFunction(new Function("public override void Initialize()", () => { }));
                AddFunction(new Function("public override void Draw(ICanvas canvas)", () => { }));
                AddFunction(new Function("public override void Cleanup()", () => { }));
            }
            else
            {
                AddUsings(new List<string>()
                {
                    "TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces"
                });

                AddFunction(new Function("public override void Initialize()", () => {
                    Writer.AddLine("base.Initialize();");
                }));
                AddFunction(new Function("public override void Draw(ICanvas canvas)", () => {
                    Writer.AddLine("base.Draw(canvas);");
                }));
                AddFunction(new Function("public override void Cleanup()", () => {
                    Writer.AddLine("base.Cleanup();");
                }));
            }
        }

        /// <summary>
        /// Initializes the Type D class with any required setup or configuration.
        /// </summary>
        /// <remarks>This method is called as part of the initialization process for derived classes.
        /// Override this method in a subclass to provide specific initialization logic for TypeD.</remarks>
        protected override void InitTypeDClass()
        {
        }

        /// <summary>
        /// Generates the output file by writing the necessary content to the target file.
        /// </summary>
        /// <remarks>This method clears the current content of the target file and writes new content to
        /// it. The target file is specified by the <see cref="Codalyzer.BaseFile"/> property.</remarks>
        public override void Generate()
        {
            Writer.TargetFile = BaseFile;
            Writer.Clear();
            WriteFile();
        }
    }
}