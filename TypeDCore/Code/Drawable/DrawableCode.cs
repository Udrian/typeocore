using System;
using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Drawable
{
    public class DrawableCode : ComponentTypeCode
    {
        // Properties
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Entities.Drawables.Drawable); } }

        // Constructors
        public DrawableCode(Component component) : base(component)
        {
            PartialClass = false;
        }

        // Functions
        protected override void InitClass()
        {
            PartialClass = false;

            if (IsBaseComponentType)
            {
                AddUsings(new List<string>()
                {
                    "TypeOEngine.Typedeaf.Core.Common",
                    "TypeOEngine.Typedeaf.Core.Engine.Graphics"
                });

                AddProperty(new Property("public override Vec2 Size", () => { Writer.AddLine("get; protected set;"); }));
                AddFunction(new Function("public override void Initialize()", () => { }));
                AddFunction(new Function("public override void Draw(Canvas canvas)", () => { }));
                AddFunction(new Function("public override void Cleanup()", () => { }));
            }
            else
            {
                AddUsings(new List<string>()
                {
                    "TypeOEngine.Typedeaf.Core.Engine.Graphics"
                });

                AddFunction(new Function("public override void Initialize()", () => {
                    Writer.AddLine("base.Initialize();");
                }));
                AddFunction(new Function("public override void Draw(Canvas canvas)", () => {
                    Writer.AddLine("base.Draw(canvas);");
                }));
                AddFunction(new Function("public override void Cleanup()", () => {
                    Writer.AddLine("base.Cleanup();");
                }));
            }
        }

        protected override void InitTypeDClass()
        {
        }

        public override void Generate()
        {
            Writer.TargetFile = BaseFile;
            Writer.Clear();
            WriteFile();
        }
    }
}