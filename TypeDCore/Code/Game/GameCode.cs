using System;
using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Game
{
    public partial class GameCode : ComponentTypeCode
    {
        // Properties
        public override Type TypeOBaseType { get { return typeof(TypeOEngine.Typedeaf.Core.Game); } }

        // Constructors
        public GameCode(Component component) : base(component)
        {
        }

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
