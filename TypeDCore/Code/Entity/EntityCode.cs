using System;
using TypeD.Code;
using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeDCore.Code.Entity
{
    /// <summary>
    /// Represents a specialized <see cref="Component"/> type code for entities, providing functionality for determining and managing
    /// updatable and drawable behaviors.
    /// </summary>
    /// <remarks>This class extends <see cref="ComponentTypeCode"/> and is designed to handle entity-specific 
    /// behaviors, such as updating and drawing, based on the interfaces implemented by the associated <see cref="Component"/>. It
    /// dynamically adds functions for initialization, cleanup, updating, and drawing depending on the <see cref="Component"/>'s
    /// capabilities.</remarks>
    public partial class EntityCode : ComponentTypeCode
    {
        // Properties
        /// <summary>
        /// Gets the base type of the <see cref="Entity"/> represented by this type.
        /// </summary>
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Entities.Entity); } }
        /// <summary>
        /// Gets a value indicating whether the current object can be updated.
        /// </summary>
        public bool Updatable { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the object is drawable.
        /// </summary>
        public bool Drawable { get; private set; }

        // Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCode"/> class with the specified <see cref="Component"/>.
        /// </summary>
        /// <remarks>The constructor determines whether the provided <see cref="Component"/> supports the
        /// <see cref="IUpdatable"/> and <see cref="IDrawable"/> interfaces. These properties are set based on the presence
        /// of these interfaces in the <see cref="Component"/>.</remarks>
        /// <param name="component">The <see cref="Component"/> used to initialize the entity. Must implement the required interfaces for specific
        /// functionality.</param>
        public EntityCode(Component component) : base(component)
        {
            Updatable = component.Interfaces.Contains(typeof(IUpdatable));
            Drawable = component.Interfaces.Contains(typeof(IDrawable)); ;
        }

        /// <summary>
        /// Initializes the class by dynamically adding functions and behaviors based on the <see cref="Component"/>'s type and
        /// capabilities.
        /// </summary>
        /// <remarks>This method configures the class by adding functions for initialization, cleanup,
        /// updating, and drawing, depending on the <see cref="Component"/>'s properties such as <see cref="ComponentTypeCode.IsBaseComponentType"/>,
        /// <see cref="Updatable"/>, and <see cref="Drawable"/>. It also considers the presence of parent <see cref="Component"/>
        /// and is implemented interfaces.</remarks>
        protected override void InitClass()
        {
            if(IsBaseComponentType)
            {
                AddFunction(new Function("protected virtual void InternalInitialize()", () => { }));
                AddFunction(new Function("protected override void Cleanup()", () => { }));
            }
            else
            {
                AddFunction(new Function("protected override void InternalInitialize()", () => {
                    Writer.AddLine("base.InternalInitialize();");
                }));
                AddFunction(new Function("protected override void Cleanup()", () => {
                    Writer.AddLine("base.Cleanup();");
                }));
            }

            if (Updatable && (ParentComponent == null || !ParentComponent.Interfaces.Contains(typeof(IUpdatable))))
            {
                AddFunction(new Function("public virtual void Update(double dt)", () => { }));
            }
            else if(Updatable)
            {
                AddFunction(new Function("public override void Update(double dt)", () => {
                    Writer.AddLine("base.Update(dt);");
                }));
            }

            if (Drawable && (ParentComponent == null || !ParentComponent.Interfaces.Contains(typeof(IDrawable))))
            {
                AddUsing("TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces");
                AddFunction(new Function("public virtual void Draw(ICanvas canvas)", () => { }));
            }
            else if(Drawable)
            {
                AddUsing("TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces");
                AddFunction(new Function("public override void Draw(ICanvas canvas)", () => {
                    Writer.AddLine("base.Draw(canvas);");
                }));
            }
        }
    }
}
