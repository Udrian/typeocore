using System;
using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Scene
{
    public partial class SceneCode : ComponentTypeCode
    {
        // Properties
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Scene); } }

        // Constructors
        public SceneCode(Component component) : base(component)
        {
        }

        protected override void InitClass()
        {
            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core"
            });

            if(IsBaseComponentType)
            {
                AddFunction(new Function("protected virtual void InternalInitialize()", () => { }));
                AddFunction(new Function("public override void OnEnter(Scene from)", () => { }));
                AddFunction(new Function("public override void OnExit(Scene to)", () => { }));
            }
            else
            {
                AddFunction(new Function("protected override void InternalInitialize()", () => {
                    Writer.AddLine("base.InternalInitialize();");
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
