using TypeD.Code;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;
using TypeD.Models.Data;

namespace TypeDCore.Code.Entity
{
    /// <summary>
    /// Represents a specialized component type code for managing entity-related functionality within the TypeOEngine
    /// framework.
    /// </summary>
    /// <remarks>The <see cref="EntityCode"/> class extends <see cref="ComponentTypeCode"/> to provide 
    /// additional functionality for handling entities and their associated behaviors, such as  initialization,
    /// updatability, and drawability. This class is designed to be used as part of the TypeOEngine framework and
    /// supports dynamic creation of entities and drawables based on the <see cref="Component"/>'s children.</remarks>
    public partial class EntityCode : ComponentTypeCode
    {
        // Constructors
        /// <summary>
        /// Initializes the type-specific configuration for the derived class.
        /// </summary>
        /// <remarks>This method sets up the necessary functions, interfaces, and properties for the
        /// derived class based on its characteristics, such as whether it is updatable or drawable. It also generates
        /// initialization logic for child components, ensuring proper creation of entities and drawables as needed. If
        /// the class is a base component type, additional internal initialization logic is added.</remarks>
        protected override void InitTypeDClass()
        {
            AddFunction(new Function("protected override void Initialize()", () => {
                Writer.AddLine("base.Initialize();");
                foreach (var child in Component.Children)
                {
                    if (child.TypeOBaseType == typeof(TypeOEngine.Typedeaf.Core.Entities.Entity))
                        Writer.AddLine($"Entities.Create<{child.FullName}>();");
                    else if (child.TypeOBaseType == typeof(TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable))
                        Writer.AddLine($"Drawables.Create<{child.FullName}>();");
                }
                if (IsBaseComponentType)
                {
                    Writer.AddLine("InternalInitialize();");
                }
            }));

            if (Updatable && (ParentComponent == null || !ParentComponent.Interfaces.Contains(typeof(IUpdatable))))
            {
                AddUsing("TypeOEngine.Typedeaf.Core.Interfaces");
                AddInterface(typeof(IUpdatable));
                AddProperty(new Property("public bool Pause"));
            }

            if (Drawable && (ParentComponent == null || !ParentComponent.Interfaces.Contains(typeof(IDrawable))))
            {
                AddUsing("TypeOEngine.Typedeaf.Core.Entities.Interfaces");
                AddInterface(typeof(IDrawable));
                AddProperty(new Property("public bool Hidden"));
                AddProperty(new Property("public int DrawOrder"));
            }
        }
    }
}
