using Dock.Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
using TypeDCore.Models.Data;
using TypeDCore.Models.Data.PropertyDefinitions;
using TypeDCore.Models.Data.Hooks;
using TypeOEngine.Typedeaf.Core.Common;
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
        IContentProvider ContentProvider { get; set; }

        // Models
        ITypeDCoreProjectModel TypeDCoreProjectModel { get; set; }
        ITypeDCoreRestoreModel TypeDCoreRestoreModel { get; set; }
        ICommandHistoryModel CommandHistoryModel { get; set; }
        ISettingModel SettingModel { get; set; }
        IPanelModel PanelModel { get; set; }
        IComponentModel ComponentModel { get; set; }
        IPropertyModel PropertyModel { get; set; }
        IEditorCommandRegistry EditorCommandRegistry { get; set; }
        NodeSelectionModel NodeSelectionModel { get; set; }
        EditorToolModel EditorToolModel { get; set; }

        // Commands
        CreateEntityTypeCommand CreateEntityTypeCommand { get; set; }
        CreateSceneTypeCommand CreateSceneTypeCommand { get; set; }
        CreateDrawableTypeCommand CreateDrawableTypeCommand { get; set; }
        DeleteComponentTypeCommand DeleteComponentTypeCommand { get; set; }
        RenameComponentTypeCommand RenameComponentTypeCommand { get; set; }
        SetStartSceneCommand SetStartSceneCommand { get; set; }
        AddNodeCommand AddNodeCommand { get; set; }
        RemoveNodeCommand RemoveNodeCommand { get; set; }
        ComponentMoveCommand ComponentMoveCommand { get; set; }
        RestoreNodeCommand RestoreNodeCommand { get; set; }
        ChangePropertyCommand ChangePropertyCommand { get; set; }
        OpenComponentCommand OpenComponentCommand { get; set; }
        CloseComponentCommand CloseComponentCommand { get; set; }
        OpenInExternalCommand OpenInExternalCommand { get; set; }

        // Functions
        public override void Initializer(Project project)
        {
            Project = project;

            // Models
            PropertyModel = Resources.Get<IPropertyModel>();

            // Properties
            PropertyModel.Register(new BooleanPropertyDefinition());
            PropertyModel.Register(new StringPropertyDefinition());
            PropertyModel.Register(new Int32PropertyDefinition());
            PropertyModel.Register(new DoublePropertyDefinition());
            PropertyModel.Register(new Vec2PropertyDefinition());
            PropertyModel.Register(new ColorPropertyDefinition());
            PropertyModel.Register(new EnumPropertyDefinition());
            PropertyModel.Register(new ContentPropertyDefinition(Resources.Get<IContentProvider>()));

            // Internal Models
            TypeDCoreProjectModel = new TypeDCoreProjectModel();
            TypeDCoreRestoreModel = new TypeDCoreRestoreModel();
            CommandHistoryModel = new CommandHistoryModel();
            NodeSelectionModel = new NodeSelectionModel();
            EditorToolModel = new EditorToolModel();

            Resources.Add(new List<object>() {
                 TypeDCoreProjectModel,
                 TypeDCoreRestoreModel,
                 CommandHistoryModel,
                 NodeSelectionModel,
                 EditorToolModel
            });

            // Providers
            ComponentProvider = Resources.Get<IComponentProvider>();
            ContentProvider = Resources.Get<IContentProvider>();
            // Models
            SettingModel = Resources.Get<ISettingModel>();
            PanelModel = Resources.Get<IPanelModel>();
            ComponentModel = Resources.Get<IComponentModel>();
            EditorCommandRegistry = Resources.Get<IEditorCommandRegistry>();

            // Commands
            CreateEntityTypeCommand = new CreateEntityTypeCommand(project, Resources);
            CreateSceneTypeCommand = new CreateSceneTypeCommand(project, Resources);
            CreateDrawableTypeCommand = new CreateDrawableTypeCommand(project, Resources);
            DeleteComponentTypeCommand = new DeleteComponentTypeCommand(Resources);
            RenameComponentTypeCommand = new RenameComponentTypeCommand(Resources);
            SetStartSceneCommand = new SetStartSceneCommand(Resources);
            AddNodeCommand = new AddNodeCommand(Resources);
            RemoveNodeCommand = new RemoveNodeCommand(Resources);
            ComponentMoveCommand = new ComponentMoveCommand(Resources);
            RestoreNodeCommand = new RestoreNodeCommand(Resources);
            ChangePropertyCommand = new ChangePropertyCommand(Resources);
            OpenComponentCommand = new OpenComponentCommand(project, Resources);
            CloseComponentCommand = new CloseComponentCommand(Resources);
            OpenInExternalCommand = new OpenInExternalCommand(project, Resources);

            // Hooks
            Hooks.AddHook<ProjectCreateHook>(ProjectCreate);
            Hooks.AddHook<InitUIHook>(InitUI);
            Hooks.AddHook<ComponentTypeBrowserContextMenuOpenedHook>(ComponentTypeBrowserContextMenuOpened);
            Hooks.AddHook<ComponentContextMenuHook>(ComponentContextMenuOpened);
            Hooks.AddHook<OptionsHook>(OptionsWindowOpened);
            Hooks.AddHook<ExtractPropertiesHook>(ExtractProperties);
            Hooks.AddHook<PropertyChangedHook>(ContentPropertyChanged);

            // Settings

            // Panels
            PanelModel.AttachPanel("typed_viewer", "Viewer", new ViewerPanel(project), Alignment.Unset);
            PanelModel.AttachPanel("typed_properties", "Properties", new PropertiesPanel(project), Alignment.Right);
            PanelModel.AttachPanel("typed_commandhistory", "Command History", new CommandHistoryPanel(), Alignment.Right);
            PanelModel.AttachPanel("typed_componentexplorer", "Component Explorer", new ComponentExplorerPanel(project), Alignment.Left);
            PanelModel.AttachPanel("typed_content", "Content", new ContentPanel(project), Alignment.Bottom);
            PanelModel.AttachPanel("typed_output", "Output", new OutputPanel(), Alignment.Bottom);
            PanelModel.AttachPanel("typed_componentbrowser", "Component Browser", new ComponentBrowserPanel(project), Alignment.Left);

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
            // Properties
            foreach (var type in new[] { typeof(bool), typeof(string), typeof(int), typeof(double), typeof(Vec2), typeof(Color), typeof(Enum), typeof(TypeOEngine.Typedeaf.Core.Engine.Contents.Content) })
                PropertyModel.Unregister(type);

            CreateEntityTypeCommand.Dispose();            CreateSceneTypeCommand.Dispose();
            CreateDrawableTypeCommand.Dispose();
            DeleteComponentTypeCommand.Dispose();
            RenameComponentTypeCommand.Dispose();
            SetStartSceneCommand.Dispose();
            AddNodeCommand.Dispose();
            RemoveNodeCommand.Dispose();
            ComponentMoveCommand.Dispose();
            RestoreNodeCommand.Dispose();
            ChangePropertyCommand.Dispose();
            OpenComponentCommand.Dispose();
            CloseComponentCommand.Dispose();
            OpenInExternalCommand.Dispose();

            // Panels
            PanelModel.DetachPanel("typed_viewer");
            PanelModel.DetachPanel("typed_componentexplorer");
            PanelModel.DetachPanel("typed_output");
            PanelModel.DetachPanel("typed_componentbrowser");
            PanelModel.DetachPanel("typed_properties");
            PanelModel.DetachPanel("typed_commandhistory");

            // Internal Models
            NodeSelectionModel.Dispose();
            Resources.Remove("TypeDCoreProjectModel");
            Resources.Remove("TypeDCoreRestoreModel");
            Resources.Remove("CommandHistoryModel");
            Resources.Remove("NodeSelectionModel");
            Resources.Remove("EditorToolModel");

            // Hooks
            Hooks.RemoveHook<ProjectCreateHook>();
            Hooks.RemoveHook<InitUIHook>();
            Hooks.RemoveHook<ComponentTypeBrowserContextMenuOpenedHook>();
            Hooks.RemoveHook<ComponentContextMenuHook>();
            Hooks.RemoveHook<OptionsHook>();
            Hooks.RemoveHook<ExtractPropertiesHook>();
            Hooks.RemoveHook<PropertyChangedHook>(ContentPropertyChanged);

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
            AddToolbarTool(hook, "Select", EditorTool.Selection, "M5,3 L5,19 L9.5,15 L12.5,21 L15,20 L12,14 L18,14 Z");
            AddToolbarTool(hook, "Move", EditorTool.Move, "M12,2 L16,6 L13,6 L13,11 L18,11 L18,8 L22,12 L18,16 L18,13 L13,13 L13,18 L16,18 L12,22 L8,18 L11,18 L11,13 L6,13 L6,16 L2,12 L6,8 L6,11 L11,11 L11,6 L8,6 Z");
            AddToolbarTool(hook, "Resize", EditorTool.Resize, "M14,3 L21,3 L21,10 L18.5,7.5 L7.5,18.5 L10,21 L3,21 L3,14 L5.5,16.5 L16.5,5.5 Z");

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
                                EditorCommandRegistry.Execute("component.open", new OpenComponentCommandData() { Project = param as Project});
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
                                        EditorCommandRegistry.Execute("component.create-entity", new CreateComponentCommandData(param as Project, "Entities"));
                                    }
                                },
                                new MenuItem() {
                                    Name = "_Scene",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) => {
                                        EditorCommandRegistry.Execute("component.create-scene", new CreateComponentCommandData(param as Project, "Scenes"));
                                    }
                                },
                                new MenuItem() {
                                    Name = "_Drawable",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) => {
                                        EditorCommandRegistry.Execute("component.create-drawable", new CreateComponentCommandData(param as Project, "Drawables"));
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
                                        EditorCommandRegistry.Execute("project.open-in-folder", new OpenInExternalCommandData(project.Location, OpenInExternalCommandData.CommandAction.OpenInFolder));
                                    }
                                },
                                new MenuItem()
                                {
                                    Name = "E_xternal Editor",
                                    ClickParameter = "LoadedProject",
                                    Click = (param) =>
                                    {
                                        var project = param as Project;
                                        EditorCommandRegistry.Execute("project.open-in-editor", new OpenInExternalCommandData(project.Location, OpenInExternalCommandData.CommandAction.OpenInEditor));
                                    }
                                }
                            }
                        }
                    }
                }
            );
        }

        void AddToolbarTool(InitUIHook hook, string name, EditorTool tool, string iconData)
        {
            hook.Toolbar.Add("Tools", new ToolbarItem()
            {
                Name = name,
                ToolTip = name,
                IconData = iconData,
                IsToggle = true,
                IsChecked = () => EditorToolModel.ActiveTool == tool,
                Click = _ => EditorToolModel.ActiveTool = tool
            });
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
                                EditorCommandRegistry.Execute("component.create-entity", new CreateComponentCommandData(param as Project, @namespace));
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
                                EditorCommandRegistry.Execute("component.create-scene", new CreateComponentCommandData(param as Project, @namespace));
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
                                EditorCommandRegistry.Execute("component.create-drawable", new CreateComponentCommandData(param as Project, @namespace));
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
                                EditorCommandRegistry.Execute("component.delete-type", new ComponentCommandData() { Component = component, Project = param as Project });
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
                                EditorCommandRegistry.Execute("component.rename-type", new ComponentCommandData() { Component = component, Project = param as Project });
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
                               EditorCommandRegistry.Execute("component.set-start-scene", new ComponentCommandData() { Component = component, Project = param as Project });
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
                                    EditorCommandRegistry.Execute("project.open-in-folder", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                                }
                            },
                            new MenuItem()
                            {
                                Name = "E_xternal Editor",
                                ClickParameter = "LoadedProject",
                                Click = (param) =>
                                {
                                    var path = ComponentProvider.GetPath(param as Project, component);
                                    EditorCommandRegistry.Execute("project.open-in-editor", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
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
                                    EditorCommandRegistry.Execute("project.open-in-folder", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                                }
                            },
                            new MenuItem()
                            {
                                Name = "E_xternal Editor",
                                ClickParameter = "LoadedProject",
                                Click = (param) =>
                                {
                                    var path = component.Template.Code.FilePath();
                                    EditorCommandRegistry.Execute("project.open-in-editor", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
                                }
                            }
                        }
                    }
                });
            }
        }

        void ComponentContextMenuOpened(ComponentContextMenuHook hook)
        {
            if (hook.OpenedComponent == null)
            {
                hook.Menu.Items.AddRange(new List<MenuItem>()
                {
                    new MenuItem()
                    {
                        Name = "_Open Component",
                        ClickParameter = "LoadedProject",
                        Click = (param) => {
                            EditorCommandRegistry.Execute("component.open", new OpenComponentCommandData() { Project = param as Project });
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
                                    var @namespace = "Entities";
                                    EditorCommandRegistry.Execute("component.create-entity", new CreateComponentCommandData(param as Project, @namespace));
                                }
                            },
                            new MenuItem() {
                                Name = "_Scene",
                                ClickParameter = "LoadedProject",
                                Click = (param) => {
                                    var @namespace = "Scenes";
                                    EditorCommandRegistry.Execute("component.create-scene", new CreateComponentCommandData(param as Project, @namespace));
                                }
                            },
                            new MenuItem() {
                                Name = "_Drawable",
                                ClickParameter = "LoadedProject",
                                Click = (param) => {
                                    var @namespace = "Drawables";
                                    EditorCommandRegistry.Execute("component.create-drawable", new CreateComponentCommandData(param as Project, @namespace));
                                }
                            }
                        }
                    }
                });

                return;
            }

            if (hook.SelectedComponent?.ReadOnly != true)
            {
                hook.Menu.Items.Add(
                    new MenuItem()
                    {
                        Name = "_Add Child Node",
                        ClickParameter = "LoadedProject",
                        Click = (param) =>
                        {
                            EditorCommandRegistry.Execute("node.add-child", new AddNodeCommandData { Project = param as Project });
                        }
                    }
                );
            }

            var contextSelection = NodeSelectionModel.SelectedNodes.Count > 0
                ? NodeSelectionModel.SelectedNodes
                : (hook.SelectedComponent != null ? new[] { hook.SelectedComponent } : System.Array.Empty<Component>());
            if (contextSelection.Any(node => node != hook.OpenedComponent && !node.Removed))
            {
                hook.Menu.Items.Add(
                    new MenuItem()
                    {
                        Name = "_Remove Node",
                        ClickParameter = "LoadedProject",
                        Click = (param) =>
                        {
                            EditorCommandRegistry.Execute("node.remove", new RemoveNodeCommandData { Project = param as Project });
                        }
                    }
                );
            }

            if (contextSelection.Any(node => node.Removed))
            {
                hook.Menu.Items.Add(
                    new MenuItem()
                    {
                        Name = "_Restore deleted Node",
                        ClickParameter = "LoadedProject",
                        Click = (param) =>
                        {
                            EditorCommandRegistry.Execute("node.restore", new RemoveNodeCommandData { Project = param as Project });
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
                                var path = ComponentProvider.GetPath(param as Project, hook.SelectedComponent == null ? hook.OpenedComponent : hook.SelectedComponent);
                                EditorCommandRegistry.Execute("project.open-in-folder", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                            }
                        },
                        new MenuItem()
                        {
                            Name = "E_xternal Editor",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                var path = ComponentProvider.GetPath(param as Project, hook.SelectedComponent == null ? hook.OpenedComponent : hook.SelectedComponent);
                                EditorCommandRegistry.Execute("project.open-in-editor", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
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
                                EditorCommandRegistry.Execute("project.open-in-folder", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInFolder));
                            }
                        },
                        new MenuItem()
                        {
                            Name = "E_xternal Editor",
                            ClickParameter = "LoadedProject",
                            Click = (param) =>
                            {
                                var path = hook.SelectedComponent == null ? hook.OpenedComponent.Template.Code.FilePath() : hook.SelectedComponent.Template.Code.FilePath();
                                EditorCommandRegistry.Execute("project.open-in-editor", new OpenInExternalCommandData(path, OpenInExternalCommandData.CommandAction.OpenInEditor));
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
                        EditorCommandRegistry.Execute("component.close", new CloseComponentCommandData() { Project = param as Project, Component = hook.OpenedComponent });
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
                }, "General"),
                new SettingItem("Viewer", "Sets the Type Fullname for the Viewer to use for Components", mainWindowSettingContext.ViewerType, (value) => {
                    var viewerType = value as string;
                    if(PanelModel.ListViewers().Contains(viewerType))
                    {
                        mainWindowSettingContext.ViewerType.Value = viewerType;
                        SettingModel.SetContext(mainWindowSettingContext);
                    }
                }, "Viewer")
            });
        }

        void ContentPropertyChanged(PropertyChangedHook hook)
        {
            if (hook.Property?.Type == null || PropertyModel.Get(hook.Property.Type) is not ContentPropertyDefinition definition)
                return;

            var path = hook.Property.Value as string;
            var request = new ContentLoadRequestedHook
            {
                Component = hook.Component,
                Property = hook.Property,
                Path = path,
                Association = definition.GetAssociation(path)
            };

            if (!Resources.Get<ICommandHistoryModel>().IsReplaying)
                request.Loaded = content => ContentLoaded(hook.Component, content);

            Hooks.Shoot(request);
        }

        void ContentLoaded(Component component, object content)
        {
            if (content is not TypeOEngine.Typedeaf.Core.Engine.Contents.Texture texture)
                return;

            var sizeProperty = component.Properties.FirstOrDefault(p => p.Name == "Size" && p.Type == typeof(Vec2));
            if (sizeProperty == null || sizeProperty.ReadOnly)
                return;

            var previousSize = sizeProperty.Value;
            var newSize = new Vec2(texture.Size.X, texture.Size.Y);
            if (Equals(previousSize, newSize))
                return;

            void Apply(object size)
            {
                sizeProperty.Value = size;
                Hooks.Shoot(new PropertyChangedHook { Component = component, Property = sizeProperty });
            }

            Apply(newSize);
            Resources.Get<ICommandHistoryModel>().AmendCurrent(() => Apply(previousSize), () => Apply(newSize));
        }

        private object GetDefaultValue(Type componentType, PropertyInfo property)
        {
            var definition = PropertyModel.Get(property.PropertyType);
            var referenceStored = definition != null && definition.ValueType != definition.Type;

            try
            {
                if (!referenceStored && !componentType.IsAbstract && property.CanRead)
                {
                    var instance = Activator.CreateInstance(componentType, true);
                    var value = property.GetValue(instance);
                    if (value != null)
                        return value;
                }
            }
            catch { }

            if (referenceStored)
                return null;

            return property.PropertyType.IsValueType || (!property.PropertyType.IsAbstract && property.PropertyType.GetConstructor(Type.EmptyTypes) != null)
                ? Activator.CreateInstance(property.PropertyType)
                : null;
        }

        public void ExtractProperties(ExtractPropertiesHook hook)
        {
            var componentType = ComponentModel.GetType(hook.Component);
            if(componentType == null)
                return;
            foreach (var property in componentType.GetProperties())
            {
                var typeOPropertyAttribute = property.GetCustomAttribute<TypeOPropertyAttribute>(true);

                if(typeOPropertyAttribute == null)
                {
                    var interfaces = property.DeclaringType.GetInterfaces();
                    foreach(var interfaze in interfaces)
                    {
                        var interfaceProp = interfaze.GetProperty(property.Name);
                        if (interfaceProp == null) continue;
                        typeOPropertyAttribute = interfaceProp.GetCustomAttribute<TypeOPropertyAttribute>(true);
                        if(typeOPropertyAttribute != null)
                            break;
                    }
                }

                if (typeOPropertyAttribute != null)
                {
                    hook.Properties.Add(new Property()
                    {
                        Name = property.Name,
                        Description = typeOPropertyAttribute.Description,
                        Type = property.PropertyType,
                        Value = typeOPropertyAttribute.DefaultValue ?? GetDefaultValue(componentType, property),
                        FromComponent = property.DeclaringType.FullName == componentType.FullName ? hook.Component : ComponentProvider.Load(Project, property.DeclaringType.FullName),
                        ReadOnly = (property.CanWrite && property.GetSetMethod(true).IsPublic) ? false : true
                    });
                }
            }
        }
    }
}
