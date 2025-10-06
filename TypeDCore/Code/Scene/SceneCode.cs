using System;
using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Scene
{
    /// <summary>
    /// Represents the code generation logic for a scene <see cref="Component"/> in the TypeOEngine framework.
    /// </summary>
    /// <remarks>This class is responsible for generating code related to scene components, including
    /// initialization and lifecycle methods such as entering and exiting scenes. It extends the
    /// <see cref="ComponentTypeCode"/> base class and provides specialized behavior for scene-related <see cref="Component"/>s.</remarks>
    public partial class SceneCode : ComponentTypeCode
    {
        // Properties
        /// <summary>
        /// Gets the base type of the object represented by this type.
        /// </summary>
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Scene); } }

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCode"/> class with the specified <see cref="Component"/>.
        /// </summary>
        /// <param name="component">The <see cref="Component"/> associated with this scene code. Cannot be null.</param>
        public SceneCode(Component component) : base(component)
        {
        }

        /// <summary>
        /// Initializes the class by adding necessary using directives and functions based on the <see cref="Component"/> type.
        /// </summary>
        /// <remarks>This method configures the class by adding required using directives and defining
        /// functions depending on whether the class represents a base component type or not. For base component types,
        /// it adds virtual and override functions without additional logic. For non-base component types, it adds
        /// override functions with calls to the base implementation.</remarks>
        protected override void InitClass()
        {
            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core"
            });

            if(IsBaseComponentType)
            {
                AddFunction(new Function("protected virtual void InternalInitialize()", () => { }));
                AddFunction(new Function("protected override void Cleanup()", () => { }));
                AddFunction(new Function("public override void OnEnter(Scene from)", () => { }));
                AddFunction(new Function("public override void OnExit(Scene to)", () => { }));
            }
            else
            {
                AddFunction(new Function("protected override void InternalInitialize()", () => {
                    Writer.AddLine("base.InternalInitialize();");
                }));
                AddFunction(new Function("protected override void Cleanup()", () => {
                    Writer.AddLine("base.Cleanup();");
                }));
                AddFunction(new Function("public override void OnEnter(Scene from)", () => {
                    Writer.AddLine("base.OnEnter(from);");
                }));
                AddFunction(new Function("public override void OnExit(Scene to)", () => {
                    Writer.AddLine("base.OnExit(to);");
                }));
            }
        }
    }
}
