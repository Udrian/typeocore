using TypeD.Models.Data;
using TypeD.Models.Interfaces;

namespace TypeDCore.Models.Interfaces
{
    public interface INodeSelectionModel : IModel
    {
        Project Project { get; }
        Component OpenedComponent { get; }
        Component SelectedNode { get; }
        Component SelectedComponentType { get; }
        bool IsComponentExplorerActive { get; }
        bool IsComponentBrowserActive { get; }

        void SetComponentExplorerActive();
        void SetNoTreeActive();
        void SetComponentBrowserActive(Project project, Component component);
    }
}
