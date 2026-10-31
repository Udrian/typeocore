namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        /// <summary>Provides a reference to the current scene.</summary>
        public interface IHasScene
        {
            /// <summary>Gets or sets the associated scene.</summary>
            public Scene Scene { get; set; }
        }

        /// <summary>Provides a scene reference of a specific type.</summary>
        /// <typeparam name="S">The scene type.</typeparam>
        public interface IHasScene<S> : IHasScene where S : Scene
        {
            Scene IHasScene.Scene { get => Scene; set => Scene = value as S; }
            /// <summary>Gets or sets the strongly typed scene.</summary>
            public new S Scene { get; set; }
        }
    }
}
