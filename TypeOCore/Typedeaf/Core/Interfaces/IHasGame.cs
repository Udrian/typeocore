namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        public interface IHasGame
        {
            public Game Game { get; set; }
        }

        public interface IHasGame<G> : IHasGame where G : Game
        {
            Game IHasGame.Game { get => Game; set => Game = value as G; }
            public new G Game { get; set; }
        }
    }
}
