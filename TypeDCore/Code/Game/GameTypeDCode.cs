using System.Collections.Generic;
using TypeD.Code;
using TypeD.Models.Data;
using TypeD.Models.Providers.Interfaces;

namespace TypeDCore.Code.Game
{
    public partial class GameCode : ComponentTypeCode
    {
        // Provider
        IComponentProvider ComponentProvider { get; set; }

        // Constructors
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

            AddFunction(new Function("public override void Initialize()", () => {
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
            AddFunction(new Function("public override void Cleanup()", () => {
                Writer.AddLine("Scenes.Cleanup();");
                Writer.AddLine("InternalCleanup();");
            }));
        }
    }
}