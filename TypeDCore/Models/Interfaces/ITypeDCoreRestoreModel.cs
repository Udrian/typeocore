using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    public interface ITypeDCoreRestoreModel : IModel
    {
        public void Restore(Project project);
    }
}
