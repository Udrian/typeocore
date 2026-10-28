using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    /// <summary>
    /// Extended project model for TypeDCore
    /// </summary>
    public interface ITypeDCoreProjectModel : IModel
    {
        public Component CreateEntity(Project project, string className, string @namespace, Component parentComponent, bool updatable, bool drawable);
        public Component CreateScene(Project project, string className, string @namespace, Component parentComponent);
        public Component CreateDrawable(Project project, string className, string @namespace, Component parentComponent);
        public void SetStartScene(Project project, Component scene);
        public void SetStartScene(Project project, string sceneFullName);
    }
}
