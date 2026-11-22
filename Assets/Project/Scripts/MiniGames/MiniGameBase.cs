using System.Collections;
using Labyrinthino.Audio;
using Labyrinthino.Core;
using Labyrinthino.Data;
using Labyrinthino.Logic;
using Labyrinthino.UI;
using UnityEngine;

namespace Labyrinthino.MiniGames
{
    /// <summary>
    /// Common behaviour of all mini-games: header, timer, level progression and saving.
    /// Concrete games only describe how one level is built and played.
    /// </summary>
    public abstract class MiniGameBase : MonoBehaviour
    {
        [SerializeField] private GameRoot _root;
        [SerializeField] private GameHeader _header;
        [SerializeField] private UiConfetti _confetti;

        private bool _pendingRestore;
        private bool _hasPendingStart;

        /// <summary>Identifier of the concrete mini-game.</summary>
        public abstract MiniGameId Id { get; }

        /// <summary>Game flow controller.</summary>
        protected GameRoot Root => _root;

        /// <summary>Header of the window.</summary>
        protected GameHeader Header => _header;

        /// <summary>Particle effect of the window.</summary>
        protected UiConfetti Confetti => _confetti;

        /// <summary>Level countdown.</summary>
        protected CountdownTimer Timer { get; } = new CountdownTimer();

        /// <summary>Saved progress of this mini-game.</summary>
        protected MiniGameProgress Progress { get; private set; }

        /// <summary>Zero based index of the level being played.</summary>
        protected int LevelIndex { get; private set; }

        /// <summary>Total number of levels.</summary>
        protected int LevelCount => MiniGameCatalog.Get(Id).LevelCount;

        /// <summary>True when the current level is the last one.</summary>
        protected bool IsLastLevel => LevelIndex >= LevelCount - 1;

        /// <summary>True while the player must not interact with the board.</summary>
        protected bool InputLocked { get; set; }

        /// <summary>True while a level is running and its state may be saved.</summary>
        protected bool LevelActive { get; set; }

        /// <summary>
        /// Starts the mini-game at the first level that was not passed yet,
        /// restoring an unfinished level when a snapshot exists.
        /// </summary>
        public void Begin()
        {
            Progress = _root.Save.GetProgress(Id);
            LevelIndex = Mathf.Min(Progress.LevelsCompleted, LevelCount - 1);
            _header.SetTitle(MiniGameCatalog.Get(Id).Title);
            _header.BackButton.onClick.RemoveAllListeners();
            _header.BackButton.onClick.AddListener(() => _root.LeaveGame(Id));
            RequestStart(Progress.HasSnapshot);
        }

        /// <summary>
        /// Starts the current level from scratch.
        /// </summary>
        public void RestartLevel()
        {
            Progress = _root.Save.GetProgress(Id);
            LevelIndex = Mathf.Min(Progress.LevelsCompleted, LevelCount - 1);
            Progress.HasSnapshot = false;
            RequestStart(false);
        }

        /// <summary>
        /// Stores the state of the running level so it can be resumed later.
        /// </summary>
        public void SaveSnapshot()
        {
            if (!LevelActive)
            {
                return;
            }

            Progress.HasSnapshot = true;
            Progress.TimeLeft = Timer.Remaining;
            WriteSnapshot(Progress);
        }

        /// <summary>
        /// Builds the current level.
        /// </summary>
        /// <param name="restoreSnapshot">True when the saved snapshot must be applied.</param>
        protected abstract void LoadLevel(bool restoreSnapshot);

        /// <summary>
        /// Copies game specific state into the progress record.
        /// </summary>
        /// <param name="progress">Record that receives the state.</param>
        protected abstract void WriteSnapshot(MiniGameProgress progress);

        /// <summary>
        /// Called when the level timer reaches zero.
        /// </summary>
        protected abstract void OnTimeExpired();

        /// <summary>
        /// Records the current level as passed and moves on: to the next level or to the victory window.
        /// </summary>
        protected void AdvanceAfterLevel()
        {
            LevelActive = false;
            Progress.LevelsCompleted = LevelIndex + 1;
            Progress.HasSnapshot = false;
            _root.Save.Save();
            if (IsLastLevel)
            {
                _root.ShowVictory(Id);
                return;
            }

            LevelIndex++;
            StartLevel(false);
        }

        /// <summary>
        /// Records the current level as finished without success and moves on:
        /// to the next level or, after the last one, back to the map of mini-games.
        /// </summary>
        protected void AdvanceAfterFailedLevel()
        {
            LevelActive = false;
            Progress.LevelsCompleted = LevelIndex + 1;
            Progress.HasSnapshot = false;
            _root.Save.Save();
            if (IsLastLevel)
            {
                _root.GoToMap();
                return;
            }

            LevelIndex++;
            StartLevel(false);
        }

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="sound">Effect to play.</param>
        protected void PlaySound(SoundId sound)
        {
            _root.Audio.Play(sound);
        }

        /// <summary>
        /// Runs an action after a delay.
        /// </summary>
        /// <param name="seconds">Delay in seconds.</param>
        /// <param name="action">Action to run.</param>
        protected void RunAfter(float seconds, System.Action action)
        {
            StartCoroutine(DelayRoutine(seconds, action));
        }

        private IEnumerator DelayRoutine(float seconds, System.Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action();
        }

        private void RequestStart(bool restoreSnapshot)
        {
            _pendingRestore = restoreSnapshot;
            _hasPendingStart = true;
            if (isActiveAndEnabled)
            {
                _hasPendingStart = false;
                StartLevel(restoreSnapshot);
            }
        }

        private void OnEnable()
        {
            if (_hasPendingStart)
            {
                _hasPendingStart = false;
                StartLevel(_pendingRestore);
            }
        }

        private void StartLevel(bool restoreSnapshot)
        {
            StopAllCoroutines();
            _confetti.Clear();
            _header.SetLevel(LevelIndex, LevelCount);
            InputLocked = false;
            LoadLevel(restoreSnapshot);
            LevelActive = true;
        }

        private void Update()
        {
            if (!Timer.IsRunning)
            {
                return;
            }

            bool expired = Timer.Tick(Time.deltaTime);
            _header.SetTimer(Timer.Remaining);
            if (expired)
            {
                OnTimeExpired();
            }
        }
    }
}
