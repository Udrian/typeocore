using Avalonia.Controls;
using Avalonia.VisualTree;
using System;
using System.Linq;
using System.Threading.Tasks;
using TypeD.Commands;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Interfaces;
using TypeD.ViewModel;
using TypeD.Models.Data;
using TypeDCore.Models.Data.Hooks;

namespace TypeDCore.Commands
{
    internal abstract class NodeClipboardCommand : CustomCommand<NodeClipboardCommandData>, IDisposable
    {
        protected INodeClipboardModel Clipboard { get; }
        private readonly IDisposable Registration;
        private readonly IHookModel Hooks;
        private readonly Action<CommandPaletteContextHook> PaletteContextRequested;

        protected NodeClipboardCommand(IResourceModel resources, string id, string name, string shortcut) : base(resources)
        {
            Clipboard = resources.Get<INodeClipboardModel>();
            Hooks = resources.Get<IHookModel>();
            PaletteContextRequested = hook => hook.Parameters[id] = GetContext(resources, id == "node.paste", requireFocus: false);
            Hooks.AddHook(PaletteContextRequested);
            Registration = resources.Get<IEditorCommandRegistry>().Register(
                id, name, name + " with its properties and children.", "global", shortcut, this,
                () => GetContext(resources, id == "node.paste"));
        }

        private static NodeClipboardCommandData GetContext(IResourceModel resources, bool paste, bool requireFocus = true)
        {
            var selection = resources.Get<INodeSelectionModel>();
            var viewer = new ViewerNodeContextHook { Project = selection.Project };
            resources.Get<IHookModel>().Shoot(viewer);
            var lifetime = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
            var focused = lifetime?.MainWindow?.FocusManager?.GetFocusedElement();
            var explorer = (focused as Avalonia.Visual)?.GetSelfAndVisualAncestors().OfType<IComponentExplorerView>().Any() == true;
            var target = selection.SelectedNode ?? selection.OpenedComponent;
            if (paste && viewer.Focused)
            {
                var hit = new ViewerHitTestHook { Project = selection.Project, X = viewer.Position.X, Y = viewer.Position.Y };
                resources.Get<IHookModel>().Shoot(hit);
                target = hit.Result ?? selection.OpenedComponent;
            }
            return new NodeClipboardCommandData
            {
                Project = selection.Project,
                Node = !requireFocus || ((viewer.Focused || explorer) && focused is not TextBox)
                    ? target : null,
                WorldPosition = viewer.Focused ? (viewer.Position.X, viewer.Position.Y) : null
            };
        }

        protected async void Run(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception exception) when (exception is System.IO.IOException or System.Text.Json.JsonException or InvalidOperationException or NotSupportedException)
            {
                ResourceModel.Get<ILogModel>().Log($"Node clipboard operation failed: {exception}");
                var box = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
                    "Node clipboard", exception.Message, MsBox.Avalonia.Enums.ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                await box.ShowWindowDialogAsync(ViewModelBase.MainWindow);
            }
        }

        public void Dispose()
        {
            Hooks.RemoveHook(PaletteContextRequested);
            Registration.Dispose();
        }
    }
}
