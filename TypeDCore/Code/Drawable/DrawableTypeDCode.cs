using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Drawable
{
    /// <summary>
    /// Generates TypeD initialization code for drawable component types.
    /// </summary>
    public partial class DrawableCode : ComponentTypeCode
    {
        /// <summary>
        /// Adds the generated initialization override for a drawable type.
        /// </summary>
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