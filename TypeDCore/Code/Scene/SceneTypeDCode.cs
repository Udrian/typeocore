using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Scene
{
    /// <summary>
    /// Represents a specialized <see cref="Component"/> type for managing scene-related functionality in the application. This class
    /// provides initialization, update, and rendering logic for scenes, supporting both base and derived <see cref="Component"/>
    /// types.
    /// </summary>
    /// <remarks>The <see cref="SceneCode"/> class is a partial class that extends <see cref="ComponentTypeCode"/>
    /// and is designed to handle scene-specific operations such as initialization, updates,
    /// and rendering. It supports both base <see cref="Component"/> types and derived types, with behavior  tailored accordingly.
    /// Derived classes can override the provided methods to customize behavior for specific scene types. The
    /// class also integrates with the engine's entity and drawing systems to manage scene updates and
    /// rendering.</remarks>
    public partial class SceneCode : ComponentTypeCode
    {
        // Constructors
        /// <summary>
        /// Initializes the TypeD class by configuring its behavior and adding necessary functions.
        /// </summary>
        /// <remarks>This method sets up the TypeD class by adding required using directives and defining 
        /// core functions such as <c>Initialize</c>, <c>Update</c>, and <c>Draw</c>. These functions are tailored to
        /// handle the specific behavior of the TypeD class, including interactions with child components and base
        /// component types.</remarks>
        protected override void InitTypeDClass()
        {
            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core.Common"
            });

            AddFunction(new Function("protected override void Initialize()", () => {
                Writer.AddLine("base.Initialize();");
                foreach (var child in Component.Children)
                {
                    if(child.TypeOBaseType == typeof(TypeOEngine.Typedeaf.Core.Entities.Entity))
                        Writer.AddLine($"Entities.Create<{child.FullName}>();");
                }
                if (IsBaseComponentType)
                {
                    Writer.AddLine("InternalInitialize();");
                }
            }));

            AddFunction(new Function("public override void Update(double dt)", () => {
                if (IsBaseComponentType)
                {
                    Writer.AddLine("Entities.Update(dt);");
                    Writer.AddLine("UpdateLoop.Update(dt);");
                }
                else
                {
                    Writer.AddLine("base.Update(dt);");
                }
            }));
            AddFunction(new Function("public override void Draw()", () => {
                if (IsBaseComponentType)
                {
                    Writer.AddLine("Canvas?.Clear(Color.Black);");
                    Writer.AddLine("DrawStack.Draw(Canvas);");
                    Writer.AddLine("Canvas?.Present();");
                }
                else
                {
                    Writer.AddLine("base.Draw();");
                }
            }));
        }
    }
}
