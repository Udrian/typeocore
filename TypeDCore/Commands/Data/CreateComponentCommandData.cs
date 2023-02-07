using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal class CreateComponentCommandData
    {
        public Project Project { get; private set; }
        public string Namespace { get; private set; }

        public CreateComponentCommandData(Project project, string @namespace)
        {
            Project = project;
            Namespace = @namespace;
        }
    }
}
