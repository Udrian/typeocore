using TypeD.Code;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Components;
using TypeDCore.Models.Interfaces;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Entities;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;
using TypeOEngine.Typedeaf.Core.Entities.Interfaces;
using TypeOEngine.Typedeaf.Core.Interfaces;

namespace TypeDCore.Models
{
    internal class TypeDCoreRestoreModel : ITypeDCoreRestoreModel
    {
        // Models
        IRestoreModel RestoreModel { get; set; }
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        ISaveModel SaveModel { get; set; }
        IProjectModel ProjectModel { get; set; }
        IComponentModel ComponentModel { get; set; }

        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Constructors
        public TypeDCoreRestoreModel() { }

        public void Init(IResourceModel resourceModel)
        {
            RestoreModel = resourceModel.Get<IRestoreModel>();
            TypeDCoreProjectModel = resourceModel.Get<ITypeDCoreProjectModel>();
            SaveModel = resourceModel.Get<ISaveModel>();
            ProjectModel = resourceModel.Get<IProjectModel>();
            ComponentModel = resourceModel.Get<IComponentModel>();

            ComponentProvider = resourceModel.Get<IComponentProvider>();

            RestoreModel.AddRestoreMethod(Restore);
        }

        // Functions
        public void Restore(Project project)
        {
            // Check if we are missing Program.cs
            if (!File.Exists(Path.Combine(project.Location, project.ProjectName, "Program.cs")))
            {
                ProjectModel.InitAndSaveCode(project, new ProgramCode());
            }

            bool updateTree = false;
            // Check if we have components that are missing CS files
            foreach (var component in ComponentProvider.ListAll(project))
            {
                var csFile = component.Template.Code.FilePath();
                var csTypeDFile = component.Template.Code.FilePathTypeD();

                if (ComponentModel.IsOfType(component, typeof(Entity)))
                {
                    if (!File.Exists(csFile) || !File.Exists(csTypeDFile))
                    {
                        var updatable = component.Interfaces.Contains(typeof(IUpdatable));
                        var drawable = component.Interfaces.Contains(typeof(IDrawable));
                        ProjectModel.SaveCode(component.Template.Code);
                        updateTree = true;
                    }
                }
                else if (ComponentModel.IsOfType(component, typeof(Scene)))
                {
                    if (!File.Exists(csFile) || !File.Exists(csTypeDFile))
                    {
                        ProjectModel.SaveCode(component.Template.Code);
                        updateTree = true;
                    }
                }
                else if (ComponentModel.IsOfType(component, typeof(Drawable)))
                {
                    if (!File.Exists(csFile))
                    {
                        ProjectModel.SaveCode(component.Template.Code);
                        updateTree = true;
                    }
                }
                else if(ComponentModel.IsOfType(component, typeof(Game)))
                {
                    if (!File.Exists(Path.Combine(project.Location, project.ProjectName, $"{project.ProjectName}Game.cs")))
                    {
                        ProjectModel.SaveCode(component.Template.Code);
                    }
                }
            }

            if (updateTree)
            {
                ProjectModel.BuildComponentTree(project);
            }

            // Check if we are missing Game.cs and StartScene
            if (!File.Exists(Path.Combine(project.Location, project.ProjectName, $"{project.ProjectName}Game.cs")))
            {
                ComponentProvider.Create(
                    project,
                    $"{project.ProjectName}Game",
                    project.ProjectName,
                    CoreComponent.GameComponent()
                );
                if (!File.Exists(Path.Combine(project.Location, project.ProjectName, "Scenes", "StartScene.cs")) && (project.StartScene == null || project.StartScene == $"{project.ProjectName}.Scenes.StartScene"))
                {
                    var scene = ComponentProvider.Create(
                        project,
                        "StartScene",
                        $"{project.ProjectName}.Scenes",
                        CoreComponent.SceneComponent()
                    );

                    TypeDCoreProjectModel.SetStartScene(project, scene.Component);
                }
            }
        }
    }
}
