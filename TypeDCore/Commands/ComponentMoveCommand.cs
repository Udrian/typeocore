using System;
using System.Linq;
using TypeD.Commands;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.ViewModel;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Dialogs.Project;

namespace TypeDCore.Commands
{
    internal class ComponentMoveCommand : CustomCommand<ComponentMoveCommandData>, IDisposable
    {
        private readonly IComponentModel _componentModel;
        private readonly IComponentProvider _componentProvider;
        private readonly ICommandHistoryModel _commandHistoryModel;
        private readonly INodeSelectionModel _nodeSelectionModel;
        private readonly IHookModel _hookModel;
        private readonly IDisposable _registration;

        public ComponentMoveCommand(IResourceModel resourceModel) : base(resourceModel)
        {
            _componentModel = resourceModel.Get<IComponentModel>();
            _componentProvider = resourceModel.Get<IComponentProvider>();
            _commandHistoryModel = resourceModel.Get<ICommandHistoryModel>();
            _nodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            _hookModel = resourceModel.Get<IHookModel>();
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                "node.move",
                "Move node",
                "Move the selected node to another node.",
                "global",
                string.Empty,
                this,
                () => new ComponentMoveCommandData
                {
                    Project = _nodeSelectionModel.Project,
                    Node = _nodeSelectionModel.SelectedNode
                });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(ComponentMoveCommandData parameter)
        {
            if (parameter?.Project == null || !CanMove(parameter.Node))
                return false;

            return parameter.Target == null
                || TryResolve(parameter.Node, parameter.Target, parameter.Position, out _, out _);
        }

        public override async void Execute(ComponentMoveCommandData parameter)
        {
            var node = parameter.Node;
            var target = parameter.Target;
            var position = parameter.Position;

            if (target == null)
            {
                var dialog = new ComponentMoveTargetDialog(GetRoot(node), node);
                if (await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) != true || dialog.SelectedComponent == null)
                    return;
                target = dialog.SelectedComponent;
                position = MovePosition.Child;
            }

            if (!TryResolve(node, target, position, out var newParent, out var index))
                return;

            Move(parameter.Project, node, newParent, index);
        }

        private void Move(Project project, Component node, Component newParent, int index)
        {
            var oldParent = node.ParentComponent;
            var oldIndex = oldParent.Children.IndexOf(node);
            var title = "Move node";
            var description = $"Moved {node.FullName} to {newParent.FullName}.";

            if (ReferenceEquals(oldParent, newParent))
            {
                Apply(node, () => _componentModel.Reorder(project, node, index));
                _commandHistoryModel.Record(title, description,
                    () => Apply(node, () => _componentModel.Reorder(project, node, oldIndex)),
                    () => Apply(node, () => _componentModel.Reorder(project, node, index)));
                return;
            }

            if (node.IsInherited)
            {
                var copy = CreateCopy(project, node);
                Apply(copy, () =>
                {
                    _componentModel.SetRemoved(project, node, true);
                    _componentModel.AddExisting(project, newParent, copy, index);
                });
                _commandHistoryModel.Record(title, description,
                    () => Apply(node, () =>
                    {
                        _componentModel.Remove(project, copy);
                        _componentModel.SetRemoved(project, node, false);
                    }),
                    () => Apply(copy, () =>
                    {
                        _componentModel.SetRemoved(project, node, true);
                        _componentModel.AddExisting(project, newParent, copy, index);
                    }));
                return;
            }

            Apply(node, () =>
            {
                _componentModel.Remove(project, node);
                _componentModel.AddExisting(project, newParent, node, index);
            });
            _commandHistoryModel.Record(title, description,
                () => Apply(node, () =>
                {
                    _componentModel.Remove(project, node);
                    _componentModel.AddExisting(project, oldParent, node, oldIndex);
                }),
                () => Apply(node, () =>
                {
                    _componentModel.Remove(project, node);
                    _componentModel.AddExisting(project, newParent, node, index);
                }));
        }

        private void Apply(Component focus, Action action)
        {
            action();
            _hookModel.Shoot(new ComponentFocusHook() { Project = _nodeSelectionModel.Project, Component = focus, Root = false });
        }

        private Component CreateCopy(Project project, Component node)
        {
            var definition = _componentProvider.Load(project, node.FullName);
            var copy = _componentProvider.CreateInstance(project, definition);
            foreach (var property in node.Properties.Where(p => p.Name != "ID"))
            {
                var copyProperty = copy.Properties.FirstOrDefault(p => p.Name == property.Name);
                if (copyProperty != null && !copyProperty.ReadOnly)
                    copyProperty.Value = property.Value;
            }
            return copy;
        }

        private bool CanMove(Component node)
        {
            return node != null
                && !node.Removed
                && node.ParentComponent != null
                && !node.ParentComponent.ReadOnly
                && !ReferenceEquals(node, _nodeSelectionModel.OpenedComponent);
        }

        /// <summary>
        /// Resolves the parent and the index among that parent's children without the moved node, or false when the move isn't allowed or changes nothing.
        /// </summary>
        internal static bool TryResolve(Component node, Component target, MovePosition position, out Component newParent, out int index)
        {
            newParent = null;
            index = -1;

            if (node == null || target == null || node.Removed || node.ParentComponent == null || node.ParentComponent.ReadOnly)
                return false;

            for (var ancestor = target; ancestor != null; ancestor = ancestor.ParentComponent)
            {
                if (ReferenceEquals(ancestor, node))
                    return false;
            }

            newParent = position is MovePosition.Child or MovePosition.FirstChild ? target : target.ParentComponent;
            if (newParent == null || newParent.ReadOnly)
                return false;

            for (var ancestor = newParent; ancestor != null; ancestor = ancestor.ParentComponent)
            {
                if (ancestor.Removed)
                    return false;
            }

            var filter = new FilterHelper();
            newParent.Template?.ChildrenFilter(filter);
            if (node.TypeOBaseType == null || !(filter.Filters ?? string.Empty).Contains($"{node.TypeOBaseType.FullName};"))
                return false;

            var siblings = newParent.Children.Where(child => !ReferenceEquals(child, node)).ToList();
            switch (position)
            {
                case MovePosition.Child:
                    index = siblings.Count;
                    break;
                case MovePosition.FirstChild:
                    index = 0;
                    break;
                case MovePosition.Before:
                    index = siblings.IndexOf(target);
                    break;
                default:
                    index = siblings.IndexOf(target) + 1;
                    break;
            }
            if (index < 0)
                return false;

            return !(ReferenceEquals(node.ParentComponent, newParent) && node.ParentComponent.Children.IndexOf(node) == index);
        }

        private static Component GetRoot(Component component)
        {
            while (component?.ParentComponent != null)
                component = component.ParentComponent;
            return component;
        }
    }
}
