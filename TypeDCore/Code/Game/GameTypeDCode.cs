using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;

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

            AddUsings(new List<string>()
            {
                "TypeOEngine.Typedeaf.Core"
            });
            SetDynamicUsing(() =>
            {
                var usings = new List<string>();
                Component defaultScene = ComponentProvider.Load(Project, Project.StartScene);
                if (defaultScene != null)
                {
                    usings.Add(defaultScene.Namespace);
                }

                return usings;
            });

            AddFunction(new Function("protected override void Initialize()", () => {
                Writer.AddLine("base.Initialize();");
                Writer.NewLine();
                TypeDInitializeCode();
                Writer.AddLine("InternalInitialize();");
                Component defaultScene = ComponentProvider.Load(Project, Project.StartScene);
                if (defaultScene != null)
                {
                    Writer.AddLine($"Scenes.SetScene<{defaultScene.ClassName}>();");
                }
            }));
            AddFunction(new Function("public override void Update(double dt)", () => {
                Writer.AddLine("Scenes.Update(dt);");
            }));
            AddFunction(new Function("public override void Draw()", () => {
                Writer.AddLine("Scenes.Draw();");
            }));
            AddFunction(new Function("protected override void Cleanup()", () => {
                Writer.AddLine("Scenes.Cleanup();");
                Writer.AddLine("InternalCleanup();");
            }));
        }
    }
}