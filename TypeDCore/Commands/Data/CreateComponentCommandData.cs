using TypeD.Models.Data;

namespace TypeDCore.Commands.Data
{
    internal class CreateComponentCommandData
    {
        public Project Project { get; private set; }
        public string Namespace { get; private set; }
        public Component BaseComponent { get; private set; }

        public CreateComponentCommandData(Project project, string @namespace) : this(project, @namespace, null) { }

        public CreateComponentCommandData(Project project, string @namespace, Component baseComponent)
        {
            Project = project;
            Namespace = @namespace;
            BaseComponent = baseComponent;
        }
    }
}
