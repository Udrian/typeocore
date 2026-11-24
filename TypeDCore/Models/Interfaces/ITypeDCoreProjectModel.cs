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
        /// Sets the game's StartingScene property to the scene, adding the scene to the game when it is not already a child.
        /// </summary>
        public void SetStartScene(Project project, Component scene);
        /// <summary>
        /// Sets the game's StartingScene property to a scene's fully qualified name, or clears it when null.
        /// </summary>
        public void SetStartScene(Project project, string sceneFullName);
        /// <summary>
        /// Gets the fully qualified name stored in the game's StartingScene property, or null.
        /// </summary>
        public string GetStartScene(Project project);
    }
}
