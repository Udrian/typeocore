using TypeD.Models.Data;
using TypeOEngine.Typedeaf.Core.Engine;

namespace TypeDCore.Models.Data.Hooks
{
    public class TypeOObjectRemovedFromViewHook : Hook
    {
        public Context Context { get; set; }
        public TypeOObject TypeOObject { get; set; }
        public Component Component { get; set; }

        public TypeOObjectRemovedFromViewHook() { }
    }
}
