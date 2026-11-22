using System;
using Labyrinthino.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Main window of the game: the map with one card per mini-game and the settings button.
    /// </summary>
    public sealed class MiniGamesScreen : MonoBehaviour
    {
        [SerializeField] private MiniGameCard[] _cards;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Text _summaryText;

        /// <summary>
        /// Refreshes all cards from the save data.
        /// </summary>
        /// <param name="save">Save service that stores the progress.</param>
        /// <param name="onGameSelected">Called when the player presses a card.</param>
        /// <param name="onSettings">Called when the player presses the settings button.</param>
        public void Bind(SaveService save, Action<MiniGameId> onGameSelected, Action onSettings)
        {
            int finished = 0;
            for (int i = 0; i < _cards.Length; i++)
            {
                MiniGameInfo info = MiniGameCatalog.All[i];
                MiniGameProgress progress = save.GetProgress(info.Id);
                _cards[i].Bind(info, progress, onGameSelected);
                if (progress.LevelsCompleted >= info.LevelCount)
                {
                    finished++;
                }
            }

            _summaryText.text = "Пройдено игр: " + finished + " из " + _cards.Length;
            _settingsButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.AddListener(() => onSettings());
        }

        private void OnEnable()
        {
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].PlayIntro(0.08f * i);
            }
        }
    }
}
