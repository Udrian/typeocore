namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        public interface IHasScene
        {
            public Scene Scene { get; set; }
        }

        public interface IHasScene<S> : IHasScene where S : Scene
        {
            Scene IHasScene.Scene { get => Scene; set => Scene = value as S; }
            public new S Scene { get; set; }
        }
    }
}
