using System.IO;
using Labyrinthino.Audio;
using Labyrinthino.Data;
using Labyrinthino.MiniGames;
using Labyrinthino.UI;
using UnityEngine;

namespace Labyrinthino.Core
{
    /// <summary>
    /// Entry point and flow controller of the game. It owns the save data and decides
    /// which window is shown after every event.
    /// </summary>
    public sealed class GameRoot : MonoBehaviour
    {
        private static readonly ScreenId[] GameScreens =
        {
            ScreenId.Quiz, ScreenId.FindPath, ScreenId.Meadow, ScreenId.ConnectDots
        };

        [SerializeField] private ScreenNavigator _navigator;
        [SerializeField] private AudioController _audio;
        [SerializeField] private LoadingScreen _loading;
        [SerializeField] private MiniGamesScreen _map;
        [SerializeField] private DetailsScreen _details;
        [SerializeField] private SettingsScreen _settings;
        [SerializeField] private ResultScreen _victory;
        [SerializeField] private ResultScreen _defeat;
        [SerializeField] private MiniGameBase[] _games;

        private MiniGameBase _activeGame;

        /// <summary>Save data and progress storage.</summary>
        public SaveService Save { get; private set; }

        /// <summary>Audio controller used for all sounds.</summary>
        public AudioController Audio => _audio;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Save = new SaveService(Path.Combine(Application.persistentDataPath, "labyrinthino_save.json"));
            Save.Load();
            ButtonSound.SetAudio(_audio);
            _audio.Initialise(Save.Data);
        }

        private void Start()
        {
            _navigator.ShowImmediate(ScreenId.Loading);
            _loading.Run(GoToMap);
        }

        /// <summary>
        /// Shows the map of mini-games.
        /// </summary>
        public void GoToMap()
        {
            _activeGame = null;
            _map.Bind(Save, OpenDetails, OpenSettings);
            _navigator.Show(ScreenId.MiniGames);
        }

        /// <summary>
        /// Shows the card window of a mini-game.
        /// </summary>
        /// <param name="id">Mini-game to describe.</param>
        public void OpenDetails(MiniGameId id)
        {
            _activeGame = null;
            _details.Bind(MiniGameCatalog.Get(id), Save.GetProgress(id), () => StartGame(id), GoToMap);
            _navigator.Show(ScreenId.MiniGameDetails);
        }

        /// <summary>
        /// Starts or resumes a mini-game.
        /// </summary>
        /// <param name="id">Mini-game to play.</param>
        public void StartGame(MiniGameId id)
        {
            _activeGame = _games[(int)id];
            _activeGame.Begin();
            _navigator.Show(GameScreens[(int)id]);
        }

        /// <summary>
        /// Restarts the current level of a mini-game, used by the defeat window.
        /// </summary>
        /// <param name="id">Mini-game to restart.</param>
        public void RestartLevel(MiniGameId id)
        {
            _activeGame = _games[(int)id];
            _activeGame.RestartLevel();
            _navigator.Show(GameScreens[(int)id]);
        }

        /// <summary>
        /// Leaves a mini-game, keeping the unfinished level so it can be resumed.
        /// </summary>
        /// <param name="id">Mini-game that is being left.</param>
        public void LeaveGame(MiniGameId id)
        {
            PersistProgress();
            OpenDetails(id);
        }

        /// <summary>
        /// Shows the victory window after the last level of a mini-game.
        /// </summary>
        /// <param name="id">Mini-game that was completed.</param>
        public void ShowVictory(MiniGameId id)
        {
            _activeGame = null;
            Save.Save();
            string title = MiniGameCatalog.Get(id).Title;
            _victory.Configure("Мини-игра «" + title + "» пройдена!", "Продолжить", () => ContinueAfterVictory(id),
                "На главную", GoToMap);
            _audio.Play(SoundId.Victory);
            _navigator.Show(ScreenId.Victory);
        }

        /// <summary>
        /// Shows the defeat window.
        /// </summary>
        /// <param name="id">Mini-game that was lost.</param>
        /// <param name="reason">Short explanation of why the level was lost.</param>
        public void ShowDefeat(MiniGameId id, string reason)
        {
            _activeGame = null;
            Save.Save();
            _defeat.Configure(reason, "Попробовать ещё раз", () => RestartLevel(id), "На главную", GoToMap);
            _audio.Play(SoundId.Defeat);
            _navigator.Show(ScreenId.Defeat);
        }

        private void OpenSettings()
        {
            SaveData data = Save.Data;
            _settings.Bind(data.MusicVolume, data.SoundVolume, OnMusicVolumeChanged, OnSoundVolumeChanged,
                () =>
                {
                    Save.Save();
                    GoToMap();
                },
                OnProgressReset);
            _navigator.Show(ScreenId.Settings);
        }

        private void OnMusicVolumeChanged(float value)
        {
            Save.Data.MusicVolume = value;
            _audio.SetMusicVolume(value);
        }

        private void OnSoundVolumeChanged(float value)
        {
            Save.Data.SoundVolume = value;
            _audio.SetSoundVolume(value);
            _audio.Play(SoundId.Tap);
        }

        private void OnProgressReset()
        {
            Save.ResetProgress();
            GoToMap();
        }

        private void ContinueAfterVictory(MiniGameId finished)
        {
            for (int step = 1; step <= MiniGameCatalog.All.Length; step++)
            {
                MiniGameInfo next = MiniGameCatalog.All[((int)finished + step) % MiniGameCatalog.All.Length];
                if (Save.GetProgress(next.Id).LevelsCompleted < next.LevelCount)
                {
                    OpenDetails(next.Id);
                    return;
                }
            }

            GoToMap();
        }

        private void PersistProgress()
        {
            if (_activeGame != null)
            {
                _activeGame.SaveSnapshot();
            }

            Save.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && Save != null)
            {
                PersistProgress();
            }
        }

        private void OnApplicationQuit()
        {
            if (Save != null)
            {
                PersistProgress();
            }
        }
    }
}
