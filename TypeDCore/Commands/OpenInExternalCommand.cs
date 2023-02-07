using System.IO;
using TypeD.Commands;
using TypeD.Helpers;
using TypeD.Models.Data.SettingContexts;
using TypeD.Models.Interfaces;
using TypeDCore.Commands.Data;

namespace TypeDCore.Commands
{
    internal class OpenInExternalCommand : CustomCommand<OpenInExternalCommandData>
    {
        // Models
        ISettingModel SettingModel { get; set; }

        // Constructors
        public OpenInExternalCommand(IResourceModel resourceModel)
        {
            SettingModel = resourceModel.Get<ISettingModel>();
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
