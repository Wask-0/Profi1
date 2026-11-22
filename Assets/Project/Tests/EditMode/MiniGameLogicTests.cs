using Labyrinthino.Data;
using Labyrinthino.Logic;
using NUnit.Framework;

namespace Labyrinthino.Tests
{
    /// <summary>
    /// Edit mode tests for the pure logic of the four mini-games.
    /// </summary>
    public class MiniGameLogicTests
    {
        /// <summary>
        /// Quiz: the Erase button removes only the last typed letter.
        /// </summary>
        [Test]
        public void Quiz_EraseButton_RemovesLastTypedLetter()
        {
            QuizModel model = new QuizModel("ДОМ", 3, 1, 3);
            TypeWord(model, "ДОМ");

            bool erased = model.Erase();

            Assert.IsTrue(erased);
            Assert.AreEqual("ДО", model.Typed);
        }

        /// <summary>
        /// Quiz: letter buttons contain every answer letter plus two or three extra ones.
        /// </summary>
        [Test]
        public void Quiz_Letters_ContainAnswerAndExtraLetters()
        {
            QuizModel model = new QuizModel("ЛОДКА", 3, 5, 3);

            Assert.AreEqual(8, model.Letters.Count);
            string offered = new string(ToArray(model));
            foreach (char letter in "ЛОДКА")
            {
                StringAssert.Contains(letter.ToString(), offered);
            }
        }

        /// <summary>
        /// Find the Path: the Reset button returns the player to the start cell.
        /// </summary>
        [Test]
        public void FindPath_ResetButton_ReturnsToStartCell()
        {
            FindPathModel model = new FindPathModel(new GridMap(MiniGameCatalog.FindPathMaps[0]));
            model.TryMove(1, 0);
            model.TryMove(2, 0);

            model.Reset();

            Assert.AreEqual(model.Start, model.Current);
            Assert.AreEqual(1, model.Route.Count);
        }

        /// <summary>
        /// Find the Path: obstacles block movement and outside taps are ignored.
        /// </summary>
        [Test]
        public void FindPath_Obstacle_IsBlockedAndOutsideIsIgnored()
        {
            FindPathModel model = new FindPathModel(new GridMap(MiniGameCatalog.FindPathMaps[0]));

            Assert.AreEqual(MoveResult.Ignored, model.TryMove(-1, 0));
            Assert.AreEqual(MoveResult.Moved, model.TryMove(0, 1));
            Assert.AreEqual(MoveResult.Blocked, model.TryMove(1, 1));
        }

        /// <summary>
        /// Find the Path: every level can be solved.
        /// </summary>
        [Test]
        public void FindPath_AllLevels_HaveShortestPath()
        {
            foreach (string[] rows in MiniGameCatalog.FindPathMaps)
            {
                FindPathModel model = new FindPathModel(new GridMap(rows));
                Assert.IsNotEmpty(model.FindShortestPath());
            }
        }

        /// <summary>
        /// Meadow: a new level starts with both brown and green cells and the counts add up.
        /// </summary>
        [Test]
        public void Meadow_InitialState_HasCorrectGreenAndBrownDistribution()
        {
            foreach (int[] level in MiniGameCatalog.MeadowLevels)
            {
                MeadowModel model = new MeadowModel(level[0], level[1], level[2], level[3], 7);

                Assert.AreEqual(level[2], model.BrownCount);
                Assert.AreEqual(level[0] * level[1] - level[2], model.GreenCount);
                Assert.Greater(model.GreenCount, 0);
                Assert.Greater(model.BrownCount, 0);
            }
        }

        /// <summary>
        /// Meadow: tapping a brown cell paints it and tapping a green one is ignored.
        /// </summary>
        [Test]
        public void Meadow_Tap_PaintsBrownAndIgnoresGreen()
        {
            MeadowModel model = new MeadowModel(4, 4, 6, 9, 3);
            int brownRow = -1;
            int brownCol = -1;
            for (int i = 0; i < 16 && brownRow < 0; i++)
            {
                if (!model.IsGreen(i / 4, i % 4))
                {
                    brownRow = i / 4;
                    brownCol = i % 4;
                }
            }

            Assert.AreEqual(MeadowTapResult.Painted, model.Tap(brownRow, brownCol));
            Assert.AreEqual(MeadowTapResult.Wasted, model.Tap(brownRow, brownCol));
            Assert.AreEqual(5, model.BrownCount);
        }

        /// <summary>
        /// Connect the Dots: the route can only begin on a start point.
        /// </summary>
        [Test]
        public void ConnectDots_Route_StartsOnlyOnStartPoint()
        {
            ConnectDotsModel model = new ConnectDotsModel(new GridMap(MiniGameCatalog.ConnectDotsMaps[0]));

            Assert.AreEqual(RouteResult.Ignored, model.TryStartRoute(1, 1));
            Assert.IsFalse(model.IsStarted);
            Assert.AreEqual(RouteResult.Started, model.TryStartRoute(0, 0));
            Assert.AreEqual(new GridPos(0, 0), model.Route[0]);
        }

        /// <summary>
        /// Connect the Dots: a route cannot cross itself or pass through obstacles.
        /// </summary>
        [Test]
        public void ConnectDots_Route_RejectsObstaclesAndRepeatedCells()
        {
            ConnectDotsModel model = new ConnectDotsModel(new GridMap(MiniGameCatalog.ConnectDotsMaps[0]));
            model.Tap(0, 0);
            model.Tap(0, 1);

            Assert.AreEqual(RouteResult.Blocked, model.Tap(0, 2));
            Assert.AreEqual(RouteResult.Extended, model.Tap(1, 1));
            Assert.AreEqual(RouteResult.Extended, model.Tap(1, 2));
        }

        /// <summary>
        /// Connect the Dots: every level has a valid solution.
        /// </summary>
        [Test]
        public void ConnectDots_AllLevels_AreSolvable()
        {
            foreach (string[] rows in MiniGameCatalog.ConnectDotsMaps)
            {
                Assert.IsTrue(new ConnectDotsModel(new GridMap(rows)).IsSolvable());
            }
        }

        /// <summary>
        /// Save: progress survives a save and load cycle.
        /// </summary>
        [Test]
        public void SaveService_SavedProgress_IsRestoredOnLoad()
        {
            string path = System.IO.Path.Combine(UnityEngine.Application.temporaryCachePath, "test_save.json");
            SaveService writer = new SaveService(path);
            writer.Data.MusicVolume = 0.25f;
            writer.GetProgress(MiniGameId.Meadow).LevelsCompleted = 2;
            writer.Save();

            SaveService reader = new SaveService(path);
            reader.Load();

            Assert.AreEqual(0.25f, reader.Data.MusicVolume, 0.001f);
            Assert.AreEqual(2, reader.GetProgress(MiniGameId.Meadow).LevelsCompleted);
        }

        private static void TypeWord(QuizModel model, string word)
        {
            foreach (char letter in word)
            {
                for (int i = 0; i < model.Letters.Count; i++)
                {
                    if (model.Letters[i] == letter && !model.IsUsed(i))
                    {
                        model.TryTypeLetter(i);
                        break;
                    }
                }
            }
        }

        private static char[] ToArray(QuizModel model)
        {
            char[] letters = new char[model.Letters.Count];
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i] = model.Letters[i];
            }

            return letters;
        }
    }
}
