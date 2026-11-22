namespace Labyrinthino.Data
{
    /// <summary>
    /// Static description of a mini-game shown on its card.
    /// </summary>
    public sealed class MiniGameInfo
    {
        /// <summary>
        /// Creates the description.
        /// </summary>
        /// <param name="id">Mini-game identifier.</param>
        /// <param name="title">Display name.</param>
        /// <param name="description">Short rules text shown on the card.</param>
        /// <param name="levelCount">Number of levels.</param>
        public MiniGameInfo(MiniGameId id, string title, string description, int levelCount)
        {
            Id = id;
            Title = title;
            Description = description;
            LevelCount = levelCount;
        }

        /// <summary>Mini-game identifier.</summary>
        public MiniGameId Id { get; }

        /// <summary>Display name.</summary>
        public string Title { get; }

        /// <summary>Short rules text.</summary>
        public string Description { get; }

        /// <summary>Number of levels.</summary>
        public int LevelCount { get; }
    }

    /// <summary>
    /// Registry of all mini-games and the static level data used by them.
    /// </summary>
    public static class MiniGameCatalog
    {
        /// <summary>Time in seconds that the correct path is shown in Find the Path.</summary>
        public const float PathPreviewSeconds = 3f;

        /// <summary>Seconds per level in the quiz.</summary>
        public const float QuizSecondsPerLevel = 60f;

        /// <summary>Attempts the player has for one quiz word.</summary>
        public const int QuizAttempts = 3;

        /// <summary>All mini-games in the order they appear on the map.</summary>
        public static readonly MiniGameInfo[] All =
        {
            new MiniGameInfo(MiniGameId.Quiz, "Викторина",
                "Угадай слово по маленькому кусочку картинки. Каждая ошибка открывает больше. У тебя 3 попытки и 60 секунд.", 5),
            new MiniGameInfo(MiniGameId.FindPath, "Найди путь",
                "Запомни путь от старта до финиша, а потом пройди его по клеткам. Не наступай на препятствия!", 3),
            new MiniGameInfo(MiniGameId.Meadow, "Лугомания",
                "Превращай коричневые клетки в зелёные. Ходов и времени мало, поэтому не трать их зря.", 3),
            new MiniGameInfo(MiniGameId.ConnectDots, "Соединяй точки",
                "Начни маршрут с подсвеченной точки, пройди все обязательные точки и закончи на финише.", 3)
        };

        /// <summary>Quiz answers; index matches the quiz picture index.</summary>
        public static readonly string[] QuizWords = { "ДОМ", "РЫБА", "ГРИБ", "ЛОДКА", "СОЛНЦЕ" };

        /// <summary>Find the Path levels. S start, F finish, # obstacle, . empty cell.</summary>
        public static readonly string[][] FindPathMaps =
        {
            new[] { "S..#", ".#..", ".#.#", "...F" },
            new[] { "S.#..", "#.#.#", "...#.", ".#...", "...#F" },
            new[] { "S#....", ".#.##.", "...#..", "##.#.#", "...#..", ".#...F" }
        };

        /// <summary>Connect the Dots levels. S start, E destination, M mandatory, # obstacle.</summary>
        public static readonly string[][] ConnectDotsMaps =
        {
            new[] { "S.#.", ".M..", "#..M", "S..E" },
            new[] { "S....", ".#.#.", ".M..M", ".#.#.", "S...E" },
            new[] { "S..#..", ".#...#", "...#M.", ".M...#", "#.#M..", "S...#E" }
        };

        /// <summary>Meadow levels: rows, columns, brown cells, allowed moves, seconds.</summary>
        public static readonly int[][] MeadowLevels =
        {
            new[] { 4, 4, 6, 9, 30 },
            new[] { 5, 5, 12, 16, 40 },
            new[] { 6, 6, 20, 26, 50 }
        };

        /// <summary>Seconds per Connect the Dots level.</summary>
        public static readonly float[] ConnectDotsSeconds = { 40f, 50f, 60f };

        /// <summary>
        /// Finds the description of a mini-game.
        /// </summary>
        /// <param name="id">Mini-game identifier.</param>
        /// <returns>The matching description.</returns>
        public static MiniGameInfo Get(MiniGameId id)
        {
            return All[(int)id];
        }
    }
}
