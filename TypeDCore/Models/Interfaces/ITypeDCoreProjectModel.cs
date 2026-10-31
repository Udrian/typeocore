using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    /// <summary>
    /// Extended project model for TypeDCore
    /// </summary>
    public interface ITypeDCoreProjectModel : IModel
    {
        /// <summary>
        /// Creates an entity component type in a project.
        /// </summary>
        public Component CreateEntity(Project project, string className, string @namespace, Component parentComponent,         bool updatable, bool drawable, string icon = null);
        /// <summary>
        /// Creates a scene component type in a project.
        /// </summary>
                public Component CreateScene(Project project, string className, string @namespace, Component parentComponent, string icon = null);
        /// <summary>
        /// Creates a drawable component type in a project.
        /// </summary>
                public Component CreateDrawable(Project project, string className, string @namespace, Component parentComponent, string icon = null);
        /// <summary>
        /// Sets the project's start scene using its component.
        /// </summary>
        public void SetStartScene(Project project, Component scene);
        /// <summary>
        /// Sets the project's start scene using its fully qualified name.
        /// </summary>
        public void SetStartScene(Project project, string sceneFullName);
    }
}
