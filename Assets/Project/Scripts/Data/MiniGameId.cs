namespace Labyrinthino.Data
{
    /// <summary>
    /// Identifies one of the four mini-games. The numeric value is persisted in the save file.
    /// </summary>
    public enum MiniGameId
    {
        /// <summary>Picture quiz where the player guesses a word from a revealed fragment.</summary>
        Quiz = 0,

        /// <summary>Memorise a path and walk it across a grid.</summary>
        FindPath = 1,

        /// <summary>Turn every brown cell green before moves or time run out.</summary>
        Meadow = 2,

        /// <summary>Draw a route through all mandatory points.</summary>
        ConnectDots = 3
    }
}
