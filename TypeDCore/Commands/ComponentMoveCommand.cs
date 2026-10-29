using System;
using System.Collections.Generic;
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
        private sealed class MoveStep
        {
            public Action Undo { get; init; }
            public Action Redo { get; init; }
            public Component UndoResult { get; init; }
            public Component RedoResult { get; init; }
        }

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
                "Move the selected nodes to another node.",
                "global",
                string.Empty,
                this,
                () => new ComponentMoveCommandData
                {
                    Project = _nodeSelectionModel.Project,
                    Node = _nodeSelectionModel.SelectedNode,
                    Nodes = _nodeSelectionModel.SelectedNodes
                });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(ComponentMoveCommandData parameter)
        {
            if (parameter?.Project == null)
                return false;

            if (parameter.Moves != null)
                return parameter.Moves.Any(move => CanMove(move.Node) && TryResolve(move.Node, move.Target, move.Position, out _, out _));

            var nodes = parameter.Nodes is { Count: > 0 } ? parameter.Nodes : new[] { parameter.Node };
            if (parameter.Target == null)
                return nodes.Any(CanMove);

            return nodes.Any(node => CanMove(node) && TryResolve(node, parameter.Target, parameter.Position, out _, out _));
        }

        public override async void Execute(ComponentMoveCommandData parameter)
        {
            var moves = parameter.Moves?.ToList();
            if (moves == null)
            {
                var target = parameter.Target;
                var position = parameter.Position;
                var primary = parameter.Node ?? parameter.Nodes?.FirstOrDefault();
                if (primary == null)
                    return;

                if (target == null)
                {
                    var dialog = new ComponentMoveTargetDialog(GetRoot(primary), primary);
                    if (await dialog.ShowDialog<bool>(ViewModelBase.MainWindow) != true || dialog.SelectedComponent == null)
                        return;
                    target = dialog.SelectedComponent;
                    position = MovePosition.Child;
                }

                var nodes = (parameter.Nodes is { Count: > 0 } ? parameter.Nodes : new[] { primary }).Distinct().ToList();
                nodes = nodes.Where(node => !nodes.Any(other => !ReferenceEquals(other, node) && IsWithin(node, other))).ToList();
                if (position is MovePosition.After or MovePosition.FirstChild)
                    nodes.Reverse();
                moves = nodes.Select(node => new ComponentMove { Node = node, Target = target, Position = position }).ToList();
            }

            var steps = new List<MoveStep>();
            foreach (var move in moves)
            {
                if (!CanMove(move.Node) || !TryResolve(move.Node, move.Target, move.Position, out var newParent, out var index))
                    continue;
                steps.Add(Move(parameter.Project, move.Node, newParent, index));
            }

            if (steps.Count == 0)
                return;

            var project = parameter.Project;
            var focus = parameter.Node ?? parameter.Nodes?.FirstOrDefault();
            Focus(project, steps.Select(step => step.RedoResult).ToList(), focus);

            var undoSteps = steps.AsEnumerable().Reverse().ToList();
            var first = steps[0].RedoResult;
            _commandHistoryModel.Record(
                "Move node",
                steps.Count == 1
                    ? $"Moved {first.FullName} to {first.ParentComponent?.FullName}."
                    : $"Moved {steps.Count} nodes.",
                () =>
                {
                    foreach (var step in undoSteps)
                        step.Undo();
                    Focus(project, steps.Select(step => step.UndoResult).ToList(), focus);
                },
                () =>
                {
                    foreach (var step in steps)
                        step.Redo();
                    Focus(project, steps.Select(step => step.RedoResult).ToList(), focus);
                });
        }

        private MoveStep Move(Project project, Component node, Component newParent, int index)
        {
            var oldParent = node.ParentComponent;
            var oldIndex = oldParent.Children.IndexOf(node);

            if (ReferenceEquals(oldParent, newParent))
            {
                _componentModel.Reorder(project, node, index);
                return new MoveStep
                {
                    Undo = () => _componentModel.Reorder(project, node, oldIndex),
                    Redo = () => _componentModel.Reorder(project, node, index),
                    UndoResult = node,
                    RedoResult = node
                };
            }

            if (node.IsInherited)
            {
                var copy = CreateCopy(project, node);
                Action redo = () =>
                {
                    _componentModel.SetRemoved(project, node, true);
                    _componentModel.AddExisting(project, newParent, copy, index);
                };
                redo();
                return new MoveStep
                {
                    Undo = () =>
                    {
                        _componentModel.Remove(project, copy);
                        _componentModel.SetRemoved(project, node, false);
                    },
                    Redo = redo,
                    UndoResult = node,
                    RedoResult = copy
                };
            }

            _componentModel.Remove(project, node);
            _componentModel.AddExisting(project, newParent, node, index);
            return new MoveStep
            {
                Undo = () =>
                {
                    _componentModel.Remove(project, node);
                    _componentModel.AddExisting(project, oldParent, node, oldIndex);
                },
                Redo = () =>
                {
                    _componentModel.Remove(project, node);
                    _componentModel.AddExisting(project, newParent, node, index);
                },
                UndoResult = node,
                RedoResult = node
            };
        }

        private void Focus(Project project, List<Component> results, Component preferred)
        {
            var root = GetRoot(results[0]);
            var ordered = new[] { root }.Flatten(c => c.Children).Where(results.Contains).ToList();
            var primary = ordered.FirstOrDefault(c => ReferenceEquals(c, preferred)) ?? ordered.LastOrDefault();
            if (primary == null)
                return;

            _hookModel.Shoot(new ComponentFocusHook() { Project = project, Component = primary, Root = false, Selection = ordered });
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

        private static bool IsWithin(Component component, Component ancestor)
        {
            for (; component != null; component = component.ParentComponent)
            {
                if (ReferenceEquals(component, ancestor))
                    return true;
            }
            return false;
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
            if (!(filter.Filters ?? string.Empty).Contains($"{node.RootBaseComponent.FullName};"))
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
