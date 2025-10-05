using System.Collections.Generic;
using TypeD.Models.Data;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeDCore.Models
{
    internal class TypeDCoreProjectModel : ITypeDCoreProjectModel
    {
        // Models
        IProjectModel ProjectModel { get; set; }
        ISaveModel SaveModel { get; set; }
        IComponentModel ComponentModel { get; set; }

        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Constructors
        public TypeDCoreProjectModel() { }

        public void Init(IResourceModel resourceModel)
        {
            ProjectModel = resourceModel.Get<IProjectModel>();
            SaveModel = resourceModel.Get<ISaveModel>();
            ComponentProvider = resourceModel.Get<IComponentProvider>();
            ComponentModel = resourceModel.Get<IComponentModel>();
        }

        // Functions
        public void CreateEntity(Project project, string className, string @namespace, Component parentComponent, bool updatable, bool drawable)
        {
            @namespace = ProjectModel.TransformNamespaceString(project, @namespace);

            var interfaces = new List<string>();
            if (updatable)
            {
                interfaces.Add(typeof(IUpdatable).FullName);
            }
            if (drawable)
            {
                interfaces.Add(typeof(IDrawable).FullName);
            }

            ComponentProvider.Create(
                project,
                className,
                @namespace,
                parentComponent,
                interfaces
            );
        }

        public void CreateScene(Project project, string className, string @namespace, Component parentComponent)
        {
            @namespace = ProjectModel.TransformNamespaceString(project, @namespace);

            ComponentProvider.Create(
                project,
                className,
                @namespace,
                parentComponent
            );
        }

        public void CreateDrawable(Project project, string className, string @namespace, Component parentComponent)
        {
            @namespace = ProjectModel.TransformNamespaceString(project, @namespace);

            ComponentProvider.Create(
                project,
                className,
                @namespace,
                parentComponent
            );
        }

        public void SetStartScene(Project project, Component scene)
        {
            if (!ComponentModel.IsOfType(scene, typeof(Scene))) return;
            project.StartScene = scene.FullName;

            SaveModel.AddSave<ProjectSaveContext>(project);

            var gameComponent = ComponentProvider.Load(project, $"{project.ProjectName}.{project.ProjectName}Game");

            ProjectModel.SaveCode(gameComponent.Template.Code);
        }
    }
}
