using System.Collections.Generic;
using TypeD.Code;

namespace TypeDCore.Code.Scene
{
    public partial class SceneCode : ComponentTypeCode
    {
        // Constructors
        protected override void InitTypeDClass()
        {
            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core.Common"
            });

            AddFunction(new Function("public override void Initialize()", () => {
                if (!IsBaseComponentType)
                {
                    Writer.AddLine("base.Initialize();");
                }
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
                    Writer.AddLine("Canvas.Clear(Color.Black);");
                    Writer.AddLine("DrawStack.Draw(Canvas);");
                    Writer.AddLine("Canvas.Present();");
                }
                else
                {
                    Writer.AddLine("base.Draw();");
                }
            }));
        }
    }
}
