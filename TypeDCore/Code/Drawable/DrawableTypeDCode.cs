using TypeD.Code;
using TypeD.Models.Data;

namespace TypeDCore.Code.Drawable
{
    public partial class DrawableCode : ComponentTypeCode
    {
        protected override void InitTypeDClass()
        {
            AddFunction(new Function("protected override void Initialize()", () => {
                foreach (var property in Component.Properties)
                {
                    if(property.Value == null || string.IsNullOrEmpty(property.Name))
                        continue;
                    Writer.AddLine($"{property.Name} = {property.Value.ToString()};");
                }
                if (IsBaseComponentType)
                {
                    Writer.AddLine("InternalInitialize();");
                }
            }));
        }
    }
}