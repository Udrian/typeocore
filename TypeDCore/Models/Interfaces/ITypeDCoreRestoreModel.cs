using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    /// <summary>
    /// Interface for restoring missing components, files, and configurations in a TypeD project.
    /// </summary>
    public interface ITypeDCoreRestoreModel : IModel
    {
        /// <summary>
        /// Restores missing components, files, and configurations for the specified project.
        /// </summary>
        /// <remarks>This method ensures that all required components, such as entities, scenes, and
        /// drawables, are present in the project. It also restores missing `.component` files, `Program.cs`, and
        /// other essential files. If necessary, it creates default components like the game class and the start scene.
        /// Additionally, it updates the project structure and rebuilds the component tree if changes are made. The
        /// method performs the following actions:
        /// <list type="bullet"><item><description>
        /// Restores missing `.component` files for types derived from <see cref="Entity"/>, <see cref="Scene"/>, or <see cref="Drawable"/>.
        /// </description></item><item><description>
        /// Ensures that `Program.cs` exists in the project directory.
        /// </description></item><item><description>
        /// Checks for missing `.cs` files for components and generates them if necessary.
        /// </description></item><item><description>
        /// Creates the game class and start scene if they are missing.
        /// </description></item><item><description>
        /// Rebuilds the component tree if updates are made to the project structure.
        /// </description></item></list></remarks>
        /// <param name="project">The project to restore. Cannot be <see langword="null"/>.</param>
        public void Restore(Project project);
    }
}
