using System.Collections.Generic;
using System.Linq;
using TypeD.Code;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;
using TypeDCore.Models.Data.PropertyDefinitions;
using Property = TypeD.Code.Codalyzer.Property;

namespace TypeDCore.Code.Game
{
    /// <summary>
    /// Represents a specialized <see cref="Component"/> type for managing game-specific functionality, including initialization,
    /// updates, rendering, and cleanup of game scenes.
    /// </summary>
    /// <remarks>This class extends <see cref="ComponentTypeCode"/> to provide game-specific behavior. It
    /// integrates with a <see cref="Component"/> provider to load and manage game scenes dynamically. The class is designed to
    /// handle the lifecycle of game scenes, including initialization, updates, rendering, and cleanup, by leveraging
    /// the provided <see cref="Component"/> system.</remarks>
    public partial class GameCode : ComponentTypeCode
    {
        // Provider
        IComponentProvider ComponentProvider { get; set; }

        // Constructors
        /// <summary>
        /// Initializes the TypeD class by configuring components, dynamic usings, and core functions.
        /// </summary>
        /// <remarks>This method sets up the necessary components and functions required for the TypeD
        /// class to operate. It adds required namespaces, and defines core lifecycle
        /// functions such as <c>Initialize</c>, <c>Update</c>, <c>Draw</c>, and <c>Cleanup</c>. The dynamic usings are
        /// determined based on the project's start scene, if available.</remarks>
        protected override void InitTypeDClass()
        {
            ComponentProvider = Resources.Get<IComponentProvider>();

            var inheritedDynamicProperties = TypeDFile.DynamicProperties;
            TypeDFile.DynamicProperties = () =>
            {
                var properties = inheritedDynamicProperties?.Invoke() ?? new List<Property>();
                properties.AddRange(WindowProperties());
                return properties;
            };

            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core"
            });

            AddFunction(new Function("protected override void Initialize()", () => {
                Writer.AddLine("base.Initialize();");
                Writer.NewLine();
                WindowInitializeCode();
                TypeDInitializeCode();
                Writer.AddLine("InternalInitialize();");
                foreach (var child in Component.Children)
                {
                    if (child.Removed || !child.IsBasedOn(typeof(TypeOEngine.Typedeaf.Core.Scene)))
                        continue;

                    Writer.AddLeftCurlyBracket();
                    Writer.AddLine($"var scene = Scenes.CreateScene<{child.FullName}>();");
                    foreach (var property in child.Properties)
                    {
                        if (child.HasPropertyOverridesFor(property.Name))
                            AddPropertyCode(property, "scene.");
                    }
                    Writer.AddRightCurlyBrackets();
                }

                var startingScene = Models.Data.StartingScene.Get(Component);
                if (startingScene != null)
                {
                    Writer.AddLine($"Scenes.SetScene<{startingScene}>();");
                }
            }));
            AddFunction(new Function("public override void Update(double dt)", () => {
                Writer.AddLine("Scenes.Update(dt);");
            }));
            AddFunction(new Function("public override void Draw()", () => {
                Writer.AddLine("Scenes.Draw();");
            }));
            AddFunction(new Function("protected override void Cleanup()", () => {
                Writer.AddLine("InternalCleanup();");
            }));
        }

        private List<Component> WindowChildren()
        {
            return Component.Children
                .Where(child => !child.Removed && child.IsBasedOn(typeof(TypeOEngine.Typedeaf.Core.Engine.Graphics.Window)))
                .ToList();
        }

        private List<Property> WindowProperties()
        {
            var windows = WindowChildren();
            var properties = new List<Property>();
            if (windows.Count == 0)
                return properties;

            properties.Add(new Property("public TypeOEngine.Typedeaf.Desktop.Engine.Services.WindowService WindowService"));
            // The first window is the main window, a list is only needed for the windows beyond it
            if (windows.Count > 1)
            {
                properties.Add(new Property("public System.Collections.Generic.List<TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces.IWindow> Windows")
                {
                    Initializer = "new System.Collections.Generic.List<TypeOEngine.Typedeaf.Core.Engine.Graphics.Interfaces.IWindow>()"
                });
            }
            return properties;
        }

        private void WindowInitializeCode()
        {
            var windows = WindowChildren();
            for (var i = 0; i < windows.Count; i++)
            {
                var create = $"WindowService.CreateWindow({WindowValueCode(windows[i], "Title")}, {WindowValueCode(windows[i], "Position")}, {WindowValueCode(windows[i], "Size")})";
                Writer.AddLine(i == 0 ? $"MainWindow = {create};" : $"Windows.Add({create});");
            }
            if (windows.Count > 0)
                Writer.NewLine();
        }

        private string WindowValueCode(Component window, string propertyName)
        {
            var property = window.Properties.FirstOrDefault(p => p.Name == propertyName);
            if (property == null)
                return propertyName == "Title" ? "\"Window\"" : "default";
            if (propertyName == "Title")
            {
                var title = (property.Value as string ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                return $"\"{title}\"";
            }
            if (property.Value is TypeOEngine.Typedeaf.Core.Common.Vec2i vec)
                return new Vec2iPropertyDefinition().ToCode(vec);
            return GetValueCode(property.Type, property.Value);
        }
    }
}