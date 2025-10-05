using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TypeD.Code;
using TypeD.Models.Data;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Components;
using TypeDCore.Models.Data.Hooks;
using TypeDCore.Models.Data.SaveContexts;
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
            var types = project.Assembly?.GetTypes().ToList() ?? new List<Type>();

            // Check if we have types that are missing .component file
            foreach (var type in types)
            {
                // Only restore if type is a subclass to any of the following types
                if (new List<Type>() { typeof(Entity), typeof(Scene), typeof(Drawable) }.Find(t => type.IsSubclassOf(t)) == null)
                {
                    continue;
                }

                // True if .component files are missing, we need to restore them
                if (!ComponentProvider.Exists(project, type))
                {
                    // Fetch parent component, will be null if there is none
                    var parentComponent = ComponentProvider.Load(project, type.BaseType.FullName);

                    if(parentComponent == null)
                    {
                        var componentBaseTypes = ComponentProvider.GetBaseTypeComponents();
                        parentComponent = componentBaseTypes.Find(c => c.FullName == type.BaseType.FullName);
                    }

                    if (type.IsSubclassOf(typeof(Entity)))
                    {
                        var updatable = type.GetInterfaces().Contains(typeof(IUpdatable));
                        var drawable = type.GetInterfaces().Contains(typeof(IDrawable));
                        TypeDCoreProjectModel.CreateEntity(project, type.Name, type.Namespace, parentComponent, updatable, drawable);
                    }
                    else if (type.IsSubclassOf(typeof(Scene)))
                    {
                        TypeDCoreProjectModel.CreateScene(project, type.Name, type.Namespace, parentComponent);
                    }
                    else if (type.IsSubclassOf(typeof(Drawable)))
                    {
                        TypeDCoreProjectModel.CreateDrawable(project, type.Name, type.Namespace, parentComponent);
                    }
                    parentComponent.Template.Init();

                    var csFilePath = Path.Combine(project.Location, $"{type.FullName.Replace('.', Path.DirectorySeparatorChar)}.cs");
                    // If CS files already exists, check if we need to convert them to typed.cs pair
                    if (File.Exists(csFilePath))
                    {
                        var fileContent = File.ReadAllText(csFilePath);
                        bool needToSave = false;

                        ReplaceCode(ref fileContent, ref needToSave, $"class {type.Name}", $"partial class {type.Name}");
                        if (type == typeof(Entity) || type == typeof(Scene))
                        {
                            ReplaceCode(ref fileContent, ref needToSave, $"public override void Initialize()", $"protected void InternalInitialize()");
                        }

                        if (needToSave)
                        {
                            var typeDCoreRestoreSaveContext = SaveModel.GetSaveContext<TypeDCoreRestoreSaveContext>();
                            typeDCoreRestoreSaveContext.RestoreCodes.Add(csFilePath, fileContent);
                            SaveModel.AddSave<TypeDCoreRestoreSaveContext>();
                        }
                    }
                }
            }

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

        private void ReplaceCode(ref string code, ref bool needToSave, string search, string replace)
        {
            if (!code.Contains(replace))
            {
                needToSave = true;
                code = code.Replace(search, replace);
            }
        }
    }
}
