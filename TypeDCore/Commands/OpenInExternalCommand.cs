using System;
using System.IO;
using TypeD.Commands;
using TypeD.Helpers;
using TypeD.Models.Data;
using TypeD.Models.Data.SettingContexts;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class OpenInExternalCommand : CustomCommand<OpenInExternalCommandData>, IDisposable
    {
        // Models
        ISettingModel SettingModel { get; set; }
        private readonly IDisposable[] _registrations;

        // Constructors
        public OpenInExternalCommand(Project project, IResourceModel resourceModel)
        {
            SettingModel = resourceModel.Get<ISettingModel>();
            var commandRegistry = resourceModel.Get<IEditorCommandRegistry>();
            _registrations = new[]
            {
                commandRegistry.Register(
                    "project.open-in-folder",
                    "Open in file explorer",
                    "Open a file or folder in the file explorer.",
                    "global",
                    string.Empty,
                    this,
                    () => new OpenInExternalCommandData(
                        project.Location,
                        OpenInExternalCommandData.CommandAction.OpenInFolder)),
                commandRegistry.Register(
                    "project.open-in-editor",
                    "Open in external editor",
                    "Open a file in the configured external editor.",
                    "global",
                    string.Empty,
                    this,
                    () => new OpenInExternalCommandData(
                        project.Location,
                        OpenInExternalCommandData.CommandAction.OpenInEditor))
            };
        }

        public void Dispose()
        {
            foreach (var registration in _registrations)
                registration.Dispose();
        }

        // Functions
        public override async void Execute(OpenInExternalCommandData parameter)
        {
            if (parameter.Action == OpenInExternalCommandData.CommandAction.OpenInFolder)
            {
                if (File.Exists(parameter.Path))
                {
                    await CMD.Run($"Invoke-Expression \"explorer '/select,{parameter.Path}'\"");
                }
                else if(Directory.Exists(parameter.Path))
                {
                    await CMD.Run($"explorer \"{parameter.Path}\"");
                }
            } else if (parameter.Action == OpenInExternalCommandData.CommandAction.OpenInEditor)
            {
                var settings = SettingModel.GetContext<MainWindowSettingContext>();

                var cmd = settings.ExternalEditor.Value;
                await CMD.Run(cmd.Replace("{path}", $"\"{parameter.Path}\""));
            }
        }
    }
}
