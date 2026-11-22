using System;
using Labyrinthino.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Card of one mini-game on the map. It bobs gently and changes its look with the progress.
    /// </summary>
    public sealed class MiniGameCard : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _visual;
        [SerializeField] private Image _background;
        [SerializeField] private Image _glow;
        [SerializeField] private Image _badge;
        [SerializeField] private Text _title;
        [SerializeField] private Text _status;
        [SerializeField] private ProgressBar _progress;
        [SerializeField] private Image _icon;

        /// <summary>
        /// Fills the card with data and attaches the click handler.
        /// </summary>
        /// <param name="info">Static description of the mini-game.</param>
        /// <param name="progress">Saved progress of the mini-game.</param>
        /// <param name="onClick">Called with the mini-game id when the card is pressed.</param>
        public void Bind(MiniGameInfo info, MiniGameProgress progress, Action<MiniGameId> onClick)
        {
            Color accent = Palette.GameColors[(int)info.Id];
            bool isComplete = progress.LevelsCompleted >= info.LevelCount;
            bool isStarted = progress.LevelsCompleted > 0 || progress.HasSnapshot;
            _title.text = info.Title;
            _progress.SetValue((float)progress.LevelsCompleted / info.LevelCount);
            _background.color = isStarted ? accent : Color.Lerp(accent, Palette.Locked, 0.55f);
            _icon.color = isStarted ? Color.white : new Color(1f, 1f, 1f, 0.7f);
            _glow.enabled = isComplete;
            _badge.gameObject.SetActive(isComplete);
            if (isComplete)
            {
                _status.text = "Пройдено!";
            }
            else if (isStarted)
            {
                _status.text = "Уровень " + (progress.LevelsCompleted + 1) + " из " + info.LevelCount;
            }
            else
            {
                _status.text = "Новая игра";
            }

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClick(info.Id));
        }

        /// <summary>
        /// Plays the pop-in animation of the card.
        /// </summary>
        /// <param name="delay">Seconds to wait before the card appears.</param>
        public void PlayIntro(float delay)
        {
            StartCoroutine(UiTween.PopIn(_visual, delay, 0.45f));
        }
    }
}
