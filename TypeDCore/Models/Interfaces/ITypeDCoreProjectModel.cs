using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    /// <summary>
    /// Extended project model for TypeDCore
    /// </summary>
    public interface ITypeDCoreProjectModel : IModel
    {
        public void CreateEntity(Project project, string className, string @namespace, Component parentComponent, bool updatable, bool drawable);
        public void CreateScene(Project project, string className, string @namespace, Component parentComponent);
        public void CreateDrawable(Project project, string className, string @namespace, Component parentComponent);
        public void SetStartScene(Project project, Component scene);
    }
}
