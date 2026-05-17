using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Drawable
{
    public partial class DrawableCode : ComponentTypeCode
    {
        protected override void InitTypeDClass()
        {
            AddFunction(new Function("protected override void Initialize()", () => {
                TypeDInitializeCode();
                if (IsBaseComponentType)
                {
                    Writer.AddLine("InternalInitialize();");
                }
            }));
        }
    }
}