namespace TypeOEngine.Typedeaf.Core
{
    namespace Interfaces
    {
        /// <summary>Provides a reference to the current game.</summary>
        public interface IHasGame
        {
            /// <summary>Gets or sets the associated game.</summary>
            public Game Game { get; set; }
        }

        /// <summary>Provides a game reference of a specific type.</summary>
        /// <typeparam name="G">The game type.</typeparam>
        public interface IHasGame<G> : IHasGame where G : Game
        {
            Game IHasGame.Game { get => Game; set => Game = value as G; }
            /// <summary>Gets or sets the strongly typed game.</summary>
            public new G Game { get; set; }
        }
    }
}
