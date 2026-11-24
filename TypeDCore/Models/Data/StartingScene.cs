using System.Collections.Generic;
using System.Linq;
using TypeD.Models.Data;

namespace TypeDCore.Models.Data
{
    /// <summary>Helpers for the dynamic StartingScene property of the game component.</summary>
    public static class StartingScene
    {
        /// <summary>The name of the property holding the full name of the scene to start the game with.</summary>
        public const string PropertyName = "StartingScene";

        /// <summary>Lists the full names of the scenes added to the game, which are the selectable starting scenes.</summary>
        public static IEnumerable<string> Options(Component game)
        {
            return game.Children
                .Where(child => !child.Removed && child.IsBasedOn(typeof(TypeOEngine.Typedeaf.Core.Scene)))
                .Select(child => child.FullName)
                .Distinct();
        }

        /// <summary>Gets the selected starting scene, or null when none is selected or it is no longer a child of the game.</summary>
        public static string Get(Component game)
        {
            var selected = game.Properties.FirstOrDefault(property => property.Name == PropertyName)?.Value as string;
            return !string.IsNullOrEmpty(selected) && Options(game).Contains(selected) ? selected : null;
        }
    }
}
