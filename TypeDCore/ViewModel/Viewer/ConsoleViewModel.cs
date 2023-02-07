using System;
using System.IO;
using System.Text;
using TypeD.Models.Data;
using TypeD.ViewModel;
using TypeDCore.View.Viewer;
using TypeOEngine.Typedeaf.Core.Entities.Drawables;

namespace TypeDCore.ViewModel.Viewer
{
    internal class ConsoleViewModel : ViewModelBase
    {
        class ConsoleWriter : TextWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }

            public override void Write(string value)
            {
                WriteEvent?.Invoke(this, value);
                base.Write(value);
            }

            public override void WriteLine(string value)
            {
                WriteEvent?.Invoke(this, value);
                base.WriteLine(value);
            }

            public event EventHandler<string> WriteEvent;
        }

        // Properties
        DrawableViewer DrawableViewer { get; set; }
        TextWriter OldTextWriter { get; set; }
        ConsoleWriter NewTextWriter { get; set; }

        // Data
        ConsoleViewer ConsoleViewer { get; set; }
        public Component Component { get; set; }

        // Constructors
        public ConsoleViewModel(ConsoleViewer consoleViewer) : base(consoleViewer)
        {
            ConsoleViewer = consoleViewer;

            OldTextWriter = Console.Out;
            NewTextWriter = new ConsoleWriter();
            NewTextWriter.WriteEvent += Write;
            Console.SetOut(NewTextWriter);
        }

        public void Init(Project project, Component component)
        {
            Component = component;

            if (Component.TypeOBaseType == typeof(Drawable))
            {
                DrawableViewer = new DrawableViewer(project, Component);
            }
        }

        // Functions
        public void Unload()
        {
            NewTextWriter.WriteEvent -= Write;
            Console.SetOut(OldTextWriter);
            DrawableViewer?.Close();
        }

        private void Write(object sender, string e)
        {
            ConsoleViewer.Dispatcher.Invoke(() =>
            {
                ConsoleViewer.Output.Text += e;
            });
        }
    }
}
