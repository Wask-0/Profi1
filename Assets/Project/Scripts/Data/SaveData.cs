using System;
using System.Collections.Generic;

namespace Labyrinthino.Data
{
    /// <summary>
    /// Persisted progress of a single mini-game, including an optional mid-level snapshot.
    /// </summary>
    [Serializable]
    public class MiniGameProgress
    {
        /// <summary>Numeric value of the <see cref="MiniGameId"/> this record belongs to.</summary>
        public int GameId;

        /// <summary>Number of levels that were already passed.</summary>
        public int LevelsCompleted;

        /// <summary>True when the player left the game in the middle of a level.</summary>
        public bool HasSnapshot;

        /// <summary>Seconds left on the level timer when the snapshot was taken.</summary>
        public float TimeLeft;

        /// <summary>Game specific counter (attempts left, moves left).</summary>
        public int Counter;

        /// <summary>Game specific board state serialised as a string.</summary>
        public string Board = string.Empty;
    }

    /// <summary>
    /// Root object written to disk: audio settings and progress of every mini-game.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>Music volume in the range 0..1.</summary>
        public float MusicVolume = 0.7f;

        /// <summary>Sound effects volume in the range 0..1.</summary>
        public float SoundVolume = 1f;

        /// <summary>Progress records, one per mini-game.</summary>
        public List<MiniGameProgress> Games = new List<MiniGameProgress>();
    }
}
