namespace TypeDCore.Commands.Data
{
    internal class OpenInExternalCommandData
    {
        public enum CommandAction
        {
            OpenInFolder,
            OpenInEditor
        }

        public string Path { get; set; }
        public CommandAction Action { get; set; }

        public OpenInExternalCommandData(string path, CommandAction action)
        {
            Path = path;
            Action = action;
        }
    }
}
