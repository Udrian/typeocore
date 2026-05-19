using Dock.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using TypeD;
using TypeD.Models.Data;
using TypeD.Models.Data.Hooks;
using TypeD.Models.Data.SettingContexts;
using TypeD.Models.Interfaces;
using TypeD.Models.Providers.Interfaces;
using TypeD.View;
using TypeD.View.TreeNodes;
using TypeDCore.Commands;
using TypeDCore.Commands.Data;
using TypeDCore.Components;
using TypeDCore.Models;
using TypeDCore.Models.Interfaces;
using TypeDCore.View.Panels;
using TypeDCore.View.Viewer;
using TypeOEngine.Typedeaf.Core;
using TypeOEngine.Typedeaf.Core.Attributes;

namespace TypeDCore
{
    internal class TypeDCoreInitializer : TypeDModuleInitializer
    {
        // Project
        Project Project { get; set; }

        // Providers
        IComponentProvider ComponentProvider { get; set; }

        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        ITypeDCoreRestoreModel TypeDCoreRestoreModel { get; set; }
        ISettingModel SettingModel { get; set; }
        IPanelModel PanelModel { get; set; }
        IComponentModel ComponentModel { get; set; }

        // Commands
        CreateEntityTypeCommand CreateEntityTypeCommand { get; set; }
        CreateSceneTypeCommand CreateSceneTypeCommand { get; set; }
        CreateDrawableTypeCommand CreateDrawableTypeCommand { get; set; }
        DeleteComponentTypeCommand DeleteComponentTypeCommand { get; set; }
        RenameComponentTypeCommand RenameComponentTypeCommand { get; set; }
        SetStartSceneCommand SetStartSceneCommand { get; set; }
        AddComponentCommand AddComponentCommand { get; set; }
        OpenComponentCommand OpenComponentCommand { get; set; }
        CloseComponentCommand CloseComponentCommand { get; set; }
        OpenInExternalCommand OpenInExternalCommand { get; set; }

        // Functions
        public override void Initializer(Project project)
        {
            Project = project;

            // Internal Models
            TypeDCoreProjectModel = new TypeDCoreProjectModel();
            TypeDCoreRestoreModel = new TypeDCoreRestoreModel();

            Resources.Add(new List<object>() {
                 TypeDCoreProjectModel,
                 TypeDCoreRestoreModel,
            });

            // Providers
            ComponentProvider = Resources.Get<IComponentProvider>();

            // Models
            SettingModel = Resources.Get<ISettingModel>();
            PanelModel = Resources.Get<IPanelModel>();
            ComponentModel = Resources.Get<IComponentModel>();

            // Commands
            CreateEntityTypeCommand = new CreateEntityTypeCommand(Resources);
            CreateSceneTypeCommand = new CreateSceneTypeCommand(Resources);
            CreateDrawableTypeCommand = new CreateDrawableTypeCommand(Resources);
            DeleteComponentTypeCommand = new DeleteComponentTypeCommand(Resources);
            RenameComponentTypeCommand = new RenameComponentTypeCommand(Resources);
            SetStartSceneCommand = new SetStartSceneCommand(Resources);
            AddComponentCommand = new AddComponentCommand(Resources);
            OpenComponentCommand = new OpenComponentCommand(Resources);
            CloseComponentCommand = new CloseComponentCommand(Resources);
            OpenInExternalCommand = new OpenInExternalCommand(Resources);

            // Hooks
            Hooks.AddHook<ProjectCreateHook>(ProjectCreate);
            Hooks.AddHook<InitUIHook>(InitUI);
            Hooks.AddHook<ComponentTypeBrowserContextMenuOpenedHook>(ComponentTypeBrowserContextMenuOpened);
            Hooks.AddHook<ComponentContextMenuHook>(ComponentContextMenuOpened);
            Hooks.AddHook<OptionsHook>(OptionsWindowOpened);
            Hooks.AddHook<ExtractPropertiesHook>(ExtractProperties);

            // Settings

            // Panels
            PanelModel.AttachPanel("typed_viewer", "Viewer", new ViewerPanel(project));
            PanelModel.AttachPanel("typed_componentexplorer", "Component Explorer", new ComponentExplorerPanel(project));
            PanelModel.AttachPanel("typed_output", "Output", new OutputPanel());
            PanelModel.AttachPanel("typed_componentbrowser", "Component Browser", new ComponentBrowserPanel(project));
            PanelModel.AttachPanel("typed_properties", "Properties", new PropertiesPanel(project));

            // Viewers
            PanelModel.AddViewer<ConsoleViewer>();

            // Data
            ComponentProvider.AddBaseTypeComponent(CoreComponent.EntityComponent());
            ComponentProvider.AddBaseTypeComponent(CoreComponent.SceneComponent());
            ComponentProvider.AddBaseTypeComponent(CoreComponent.DrawableComponent());
            ComponentProvider.AddBaseTypeComponent(CoreComponent.GameComponent());
        }

        public override void Uninitializer()
        {
            // Panels
            PanelModel.DetachPanel("typed_viewer");
            PanelModel.DetachPanel("typed_component");
            PanelModel.DetachPanel("typed_output");
            PanelModel.DetachPanel("typed_componenttypebrowser");

            // Internal Models
            Resources.Remove("TypeDCoreProjectModel");
            Resources.Remove("TypeDCoreRestoreModel");

            // Hooks
            Hooks.RemoveHook<ProjectCreateHook>();
            Hooks.RemoveHook<InitUIHook>();
            Hooks.RemoveHook<ComponentTypeBrowserContextMenuOpenedHook>();
            Hooks.RemoveHook<ComponentContextMenuHook>();
            Hooks.RemoveHook<OptionsHook>();
            Hooks.RemoveHook<ExtractPropertiesHook>();

            // Settings

            // Viewers
            PanelModel.RemoveViewer<ConsoleViewer>();

            // Data
            ComponentProvider.RemoveBaseTypeComponent(CoreComponent.EntityComponent());
            ComponentProvider.RemoveBaseTypeComponent(CoreComponent.SceneComponent());
            ComponentProvider.RemoveBaseTypeComponent(CoreComponent.DrawableComponent());
            ComponentProvider.RemoveBaseTypeComponent(CoreComponent.GameComponent());
        }

        // Events
        void ProjectCreate(ProjectCreateHook hook)
        {
            ComponentProvider.Create(
                hook.Project,
                $"{hook.Project.ProjectName}Game",
                hook.Project.ProjectName,
                CoreComponent.GameComponent()
            );
            var scene = ComponentProvider.Create(
                hook.Project,
                "StartScene",
                $"{hook.Project.ProjectName}.Scenes",
                CoreComponent.SceneComponent()
            );

            TypeDCoreProjectModel.SetStartScene(hook.Project, scene.Component);
        }

        void InitUI(InitUIHook hook)
        {
            hook.Menu.Items.Add(
                new MenuItem() {
                    Name = "_Project",
                    Items = new List<MenuItem>()
                    {
                        new MenuItem()
                        {
                            Name = "_Open Component",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                OpenComponentCommand.Execute(new OpenComponentCommandData() { Project = param as Project});
                            }
                        },
                        new MenuItem()
                        {
                            Name = "_Create Component",
                            Items = new List<MenuItem>()
                            {
                                new MenuItem() {
                                    Name = "_Entity",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) => {
                                        CreateEntityTypeCommand.Execute(new CreateComponentCommandData(param as Project, $"Entities"));
                                    }
                                },
                                new MenuItem() {
                                    Name = "_Scene",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) => {
                                        CreateSceneTypeCommand.Execute(new CreateComponentCommandData(param as Project, $"Scenes"));
                                    }
                                },
                                new MenuItem() {
                                    Name = "_Drawable",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) => {
                                        CreateDrawableTypeCommand.Execute(new CreateComponentCommandData(param as Project, $"Drawables"));
                                    }
                                }
                            }
                        },
                        new MenuItem()
                        {
                            Name = "Open _Project in...",
                            Items = new List<MenuItem>()
                            {
                                new MenuItem()
                                {
                                    Name = "_Explorer",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) =>
                                    {
                                        var project = param as Project;
                                        OpenInExternalCommand.Execute(new OpenInExternalCommandData(project.Location, OpenInExternalCommandData.CommandAction.OpenInFolder));
                                    }
                                },
                                new MenuItem()
                                {
                                    Name = "E_xternal Editor",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) =>
                                    {
                                        var project = param as Project;
                                        OpenInExternalCommand.Execute(new OpenInExternalCommandData(project.Location, OpenInExternalCommandData.CommandAction.OpenInEditor));
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        void ComponentTypeBrowserContextMenuOpened(ComponentTypeBrowserContextMenuOpenedHook hook)
        {
            hook.Menu.Items.Add(
                new MenuItem()
                {
                    Name = "_Create Component",
                    Items = new List<MenuItem>()
                    {
                        new MenuItem() {
                            Name = "_Entity",
                            ClickParameter = "LoadedProject",
                            Click = (param) => {
                                var @namespace = "Entities";
                                if(hook.Node != null)
                                {
                                    Func<Node, string> getParentName = null;
                                    getParentName = (node) => {
                                        var retVal = "";
                                        if(node.Parent != null)
                                        {
                                            retVal = getParentName(node.Parent);
                                        }

                                        if(node.Nodes.Count != 0)
                                            return retVal == "" ? node.Name : $"{retVal}.{node.Name}";
                                        return retVal;
                                    };
                                    @namespace = getParentName(hook.Node);
                                }
                                CreateEntityTypeCommand.Execute(new CreateComponentCommandData(param as Project, @namespace));
                            }
                        },
                        new MenuItem() {
                            Name = "_Scene",
                            ClickParameter = "LoadedProject",
                            Click = (param) => {
                                var @namespace = "Scenes";
                                if(hook.Node != null)
                                {
                                    Func<Node, string> getParentName = null;
                                    getParentName = (node) => {
                                        var retVal = "";
                                        if(node.Parent != null)
                                        {
                                            retVal = getParentName(node.Parent);
                                        }

                                        if(node.Nodes.Count != 0)
                                            return retVal == "" ? node.Name : $"{retVal}.{node.Name}";
                                        return retVal;
                                    };
                                    @namespace = getParentName(hook.Node);
                                }
                                CreateSceneTypeCommand.Execute(new CreateComponentCommandData(param as Project, @namespace));
                            }
                        },
                        new MenuItem() {
                            Name = "_Drawable",
                            ClickParameter = "LoadedProject",
                            Click = (param) => {
                                var @namespace = "Drawables";
                                if(hook.Node != null)
                                {
                                    Func<Node, string> getParentName = null;
                                    getParentName = (node) => {
                                        var retVal = "";
                                        if(node.Parent != null)
                                        {
                                            retVal = getParentName(node.Parent);
                                        }

                                        if(node.Nodes.Count != 0)
                                            return retVal == "" ? node.Name : $"{retVal}.{node.Name}";
                                        return retVal;
                                    };
                                    @namespace = getParentName(hook.Node);
                                }
                                CreateDrawableTypeCommand.Execute(new CreateComponentCommandData(param as Project, @namespace));
                            }
                        }
                    }
                }
            );
            if(hook.Node != null && hook.Node.Item is Component)
            {
                var component = hook.Node.Item as Component;

                if(!ComponentModel.IsOfType(component, typeof(Game)))
                {
                    hook.Menu.Items.Add(
                        new MenuItem()
                        {
                            Name = "_Delete Component",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                DeleteComponentTypeCommand.Execute(new ComponentCommandData() { Component = component, Project = param as Project });
                            }
                        }
                    );
                    hook.Menu.Items.Add(
                        new MenuItem()
                        {
                            Name = "_Rename Component",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                RenameComponentTypeCommand.Execute(new ComponentCommandData() { Component = component, Project = param as Project });
                            }
                        }
                    );
                }
                if (ComponentModel.IsOfType(component, typeof(Scene)))
                {
                    hook.Menu.Items.Add(
                       new MenuItem()
                       {
                           Name = "_Set Start Scene",
                           ClickParameter = "LoadedProject",
                           Click = (param) =>
                           {
                               SetStartSceneCommand.Execute(new ComponentCommandData() { Component = component, Project = param as Project });
                           }
                       }
                   );
                }

                hook.Menu.Items.AddRange(new List<MenuItem>()
                {
                    new MenuItem()
                    {
                        Name = "_Open Component in...",
                        Items = new List<MenuItem>()
                        {
                            new MenuItem()
                            {
                                Name = "_Explorer",
                                ClickParameter = "LoadedProject",
                                Click = (param) =>
                                {
                                    var path = ComponentProvider.GetPath(param as Project, component);
                                    OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                                }
                            },
                            new MenuItem()
                            {
                                Name = "E_xternal Editor",
                                ClickParameter = "LoadedProject",
                                Click = (param) =>
                                {
                                    var path = ComponentProvider.GetPath(param as Project, component);
                                    OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
                                }
                            }
                        }
                    },
                    new MenuItem()
                    {
                        Name = "O_pen Code in...",
                        Items = new List<MenuItem>()
                        {
                            new MenuItem()
                            {
                                Name = "_Explorer",
                                ClickParameter = "LoadedProject",
                                Click = (param) =>
                                {
                                    var path = component.Template.Code.FilePath();
                                    OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                                }
                            },
                            new MenuItem()
                            {
                                Name = "E_xternal Editor",
                                ClickParameter = "LoadedProject",
                                Click = (param) =>
                                {
                                    var path = component.Template.Code.FilePath();
                                    OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
                                }
                            }
                        }
                    }
                });
            }
        }

        void ComponentContextMenuOpened(ComponentContextMenuHook hook)
        {
            if(hook.OpenedComponent == null)
            {
                hook.Menu.Items.Add(
                    new MenuItem()
                    {
                        Name = "_Open Component",
                        ClickParameter = "LoadedProject",
                        Click = (param) => {
                            OpenComponentCommand.Execute(new OpenComponentCommandData() { Project = param as Project });
                        }
                    }
                );

                return;
            }

            hook.Menu.Items.Add(
                new MenuItem()
                {
                    Name = "_Add Child Component",
                    ClickParameter = "LoadedProject",
                    Click = (param) =>
                    {
                        AddComponentCommand.Execute(new AddComponentCommandData() { ToComponent = hook.SelectedComponent ?? hook.OpenedComponent, Project = param as Project});
                    }
                }
            );

            hook.Menu.Items.AddRange(new List<MenuItem>()
            {
                new MenuItem()
                {
                    Name = "_Open Component in...",
                    Items = new List<MenuItem>()
                    {
                        new MenuItem()
                        {
                            Name = "_Explorer",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                var path = ComponentProvider.GetPath(param as Project, hook.SelectedComponent == null ? hook.OpenedComponent : hook.SelectedComponent);
                                OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                            }
                        },
                        new MenuItem()
                        {
                            Name = "E_xternal Editor",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                var path = ComponentProvider.GetPath(param as Project, hook.SelectedComponent == null ? hook.OpenedComponent : hook.SelectedComponent);
                                OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
                            }
                        }
                    }
                },
                new MenuItem()
                {
                    Name = "O_pen Code in...",
                    Items = new List<MenuItem>()
                    {
                        new MenuItem()
                        {
                            Name = "_Explorer",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                var path = hook.SelectedComponent == null ? hook.OpenedComponent.Template.Code.FilePath() : hook.SelectedComponent.Template.Code.FilePath();
                                OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                            }
                        },
                        new MenuItem()
                        {
                            Name = "E_xternal Editor",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                var path = hook.SelectedComponent == null ? hook.OpenedComponent.Template.Code.FilePath() : hook.SelectedComponent.Template.Code.FilePath();
                                OpenInExternalCommand.Execute(new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
                            }
                        }
                    }
                }
            });

            hook.Menu.Items.Add(
                new MenuItem()
                {
                    Name = "_Close",
                    ClickParameter = "LoadedProject",
                    Click = (param) =>
                    {
                        CloseComponentCommand.Execute(new CloseComponentCommandData() { Project = param as Project, Component = hook.OpenedComponent });
                    }
                }
            );
        }

        void OptionsWindowOpened(OptionsHook hook)
        {
            var mainWindowSettingContext = SettingModel.GetContext<MainWindowSettingContext>(hook.Level);

            hook.Items.AddRange(new List<SettingItem>()
            { 
                new SettingItem("External Editor", "Command to run when opening files in external editor, can insert value {path} that points to the file", mainWindowSettingContext.ExternalEditor, (value) => {
                    mainWindowSettingContext.ExternalEditor.Value = value as string;
                    SettingModel.SetContext(mainWindowSettingContext);
                }),
                new SettingItem("Viewer", "Sets the Type Fullname for the Viewer to use for Components", mainWindowSettingContext.ViewerType, (value) => {
                    var viewerType = value as string;
                    if(PanelModel.ListViewers().Contains(viewerType))
                    {
                        mainWindowSettingContext.ViewerType.Value = viewerType;
                        SettingModel.SetContext(mainWindowSettingContext);
                    }
                })
            });
        }

        public void ExtractProperties(ExtractPropertiesHook hook)
        {
            var componentType = ComponentModel.GetType(hook.Component);
            if(componentType == null)
                return;
            foreach (var property in componentType.GetProperties())
            {
                var typeOPropertyAttribute = property.GetCustomAttribute<TypeOPropertyAttribute>(true);
                if (typeOPropertyAttribute != null)
                {
                    hook.Properties.Add(new Property()
                    {
                        Name = property.Name,
                        Description = typeOPropertyAttribute.Description,
                        Type = property.PropertyType,
                        Value = typeOPropertyAttribute.DefaultValue ?? Activator.CreateInstance(property.PropertyType),
                        FromComponent = property.DeclaringType.FullName == componentType.FullName ? hook.Component : ComponentProvider.Load(Project, property.DeclaringType.FullName),
                        ReadOnly = (property.CanWrite && property.GetSetMethod(true).IsPublic) ? false : true
                    });
                }
            }
        }
    }
}
