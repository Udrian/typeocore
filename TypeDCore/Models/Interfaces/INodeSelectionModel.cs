using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    public interface INodeSelectionModel : IModel
    {
        Project Project { get; }
        Component OpenedComponent { get; }
        Component SelectedNode { get; }
        /// <summary>
        /// All selected nodes in tree order. Contains <see cref="SelectedNode"/> when it isn't null.
        /// </summary>
        System.Collections.Generic.IReadOnlyList<Component> SelectedNodes { get; }
        Component SelectedComponentType { get; }
        bool IsComponentExplorerActive { get; }
        bool IsComponentBrowserActive { get; }

        void SetComponentExplorerActive();
        void SetNoTreeActive();
        void SetComponentBrowserActive(Project project, Component component);
    }
}
