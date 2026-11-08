using System;
using TypeD.Commands;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;
using TypeDCore.Models.Data.Hooks;

namespace TypeDCore.Commands
{
    internal abstract class ViewerCameraCommand : CustomCommand<ViewerCameraCommandData>, IDisposable
    {
        private readonly IHookModel _hookModel;
        private readonly IDisposable _registration;
        private readonly ViewerCameraAction _action;

        protected ViewerCameraCommand(IResourceModel resourceModel, ViewerCameraAction action, string id, string name, string description, string defaultShortcut) : base(resourceModel)
        {
            _action = action;
            _hookModel = resourceModel.Get<IHookModel>();
            var nodeSelectionModel = resourceModel.Get<INodeSelectionModel>();
            _registration = resourceModel.Get<IEditorCommandRegistry>().Register(
                id,
                name,
                description,
                "global",
                defaultShortcut,
                this,
                () => new ViewerCameraCommandData { Project = nodeSelectionModel.Project });
        }

        public void Dispose() => _registration.Dispose();

        public override bool CanExecute(ViewerCameraCommandData parameter) => parameter?.Project != null;

        public override void Execute(ViewerCameraCommandData parameter)
        {
            _hookModel.Shoot(new ViewerCameraHook
            {
                Project = parameter.Project,
                Action = _action,
                Target = parameter.Component
            });
        }
    }

    internal class TeleportToSelectionCommand : ViewerCameraCommand
    {
        public const string Id = "viewer.teleport-to-selection";

        public TeleportToSelectionCommand(IResourceModel resourceModel) : base(
            resourceModel,
            ViewerCameraAction.TeleportToSelection,
            Id,
            "Teleport to selection",
            "Move the viewer camera to the selected node, or the opened component when nothing is selected.",
            "Ctrl+T")
        { }
    }

    internal class ResetViewCommand : ViewerCameraCommand
    {
        public const string Id = "viewer.reset-view";

        public ResetViewCommand(IResourceModel resourceModel) : base(
            resourceModel,
            ViewerCameraAction.ResetView,
            Id,
            "Reset view",
            "Reset the viewer camera to its default position and zoom.",
            "Home")
        { }
    }
}
