using System.Collections.Generic;
using System.Linq;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SaveContexts;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Models.Interfaces;
using TypeDCore.Models.Data;
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
        IHookModel HookModel { get; set; }

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
            HookModel = resourceModel.Get<IHookModel>();
        }

        // Functions
        public Component CreateEntity(Project project, string className, string @namespace, Component parentComponent,         bool updatable, bool drawable, string icon = null)
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

            return ComponentProvider.Create(
                project,
                className,
                @namespace,
                parentComponent,
                interfaces,
                icon
            ).Component;
        }

        public Component         CreateScene(Project project, string className, string @namespace, Component parentComponent, string icon = null)
        {
            @namespace = ProjectModel.TransformNamespaceString(project, @namespace);

            return ComponentProvider.Create(
                project,
                className,
                @namespace,
                        parentComponent,
                        icon: icon
                    ).Component;
        }

        public Component         CreateDrawable(Project project, string className, string @namespace, Component parentComponent, string icon = null)
        {
            @namespace = ProjectModel.TransformNamespaceString(project, @namespace);

            return ComponentProvider.Create(
                project,
                className,
                @namespace,
                        parentComponent,
                        icon: icon
                    ).Component;
        }

        public void SetStartScene(Project project, Component scene)
        {
            if (!ComponentModel.IsOfType(scene, typeof(Scene))) return;
            SetStartScene(project, scene.FullName);
        }

        public void SetStartScene(Project project, string sceneFullName)
        {
            var game = ComponentProvider.Load(project, $"{project.ProjectName}.{project.ProjectName}Game");
            if (game == null)
                return;

            if (sceneFullName != null && !StartingScene.Options(game).Contains(sceneFullName))
            {
                var scene = ComponentProvider.Load(project, sceneFullName);
                if (scene == null || !ComponentModel.IsOfType(scene, typeof(Scene)))
                    return;
                ComponentModel.Add(project, game, scene);
            }

            var property = game.Properties.FirstOrDefault(p => p.Name == StartingScene.PropertyName);
            if (property == null)
                return;
            property.Value = sceneFullName;

            ComponentProvider.Save(project, game);
            ProjectModel.SaveCode(game.Template.Code);
            HookModel.Shoot(new PropertyChangedHook { Component = game, Property = property });
        }

        public string GetStartScene(Project project)
        {
            var game = ComponentProvider.Load(project, $"{project.ProjectName}.{project.ProjectName}Game");
            return game == null ? null : StartingScene.Get(game);
        }
    }
}
