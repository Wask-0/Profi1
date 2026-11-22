using UnityEngine;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Colour palette shared by all windows so the interface keeps one visual style.
    /// </summary>
    public static class Palette
    {
        /// <summary>Gold accent colour.</summary>
        public static readonly Color Gold = new Color32(255, 207, 74, 255);

        /// <summary>Teal accent colour.</summary>
        public static readonly Color Teal = new Color32(62, 214, 195, 255);

        /// <summary>Coral colour used for errors.</summary>
        public static readonly Color Coral = new Color32(255, 107, 107, 255);

        /// <summary>Warm white used for text.</summary>
        public static readonly Color Cream = new Color32(255, 244, 220, 255);

        /// <summary>Dark violet used for panels.</summary>
        public static readonly Color Panel = new Color32(46, 28, 92, 235);

        /// <summary>Darker violet used for text on bright buttons.</summary>
        public static readonly Color DarkText = new Color32(44, 22, 78, 255);

        /// <summary>Colour of a meadow cell that is already green.</summary>
        public static readonly Color Grass = new Color32(94, 214, 92, 255);

        /// <summary>Colour of a meadow cell that is still brown.</summary>
        public static readonly Color Soil = new Color32(140, 92, 48, 255);

        /// <summary>Colour of an empty board cell.</summary>
        public static readonly Color Cell = new Color32(214, 200, 255, 255);

        /// <summary>Colour of an obstacle cell.</summary>
        public static readonly Color Wall = new Color32(70, 48, 120, 255);

        /// <summary>Colour of disabled or locked elements.</summary>
        public static readonly Color Locked = new Color32(120, 110, 150, 255);

        /// <summary>Accent colour of each mini-game card in map order.</summary>
        public static readonly Color[] GameColors =
        {
            new Color32(255, 170, 60, 255),
            new Color32(62, 214, 195, 255),
            new Color32(110, 220, 90, 255),
            new Color32(255, 107, 150, 255)
        };
    }
}
