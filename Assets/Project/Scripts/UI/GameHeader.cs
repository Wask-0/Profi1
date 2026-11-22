using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Top bar of a mini-game window: back button, title, level counter and timer.
    /// </summary>
    public sealed class GameHeader : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _levelText;
        [SerializeField] private GameObject _timerRoot;
        [SerializeField] private Text _timerText;

        /// <summary>Button that leaves the mini-game.</summary>
        public Button BackButton => _backButton;

        /// <summary>
        /// Sets the title of the mini-game.
        /// </summary>
        /// <param name="title">Display name of the mini-game.</param>
        public void SetTitle(string title)
        {
            _titleText.text = title;
        }

        /// <summary>
        /// Shows the number of the current level.
        /// </summary>
        /// <param name="level">Zero based level index.</param>
        /// <param name="total">Total number of levels.</param>
        public void SetLevel(int level, int total)
        {
            _levelText.text = "Уровень " + (level + 1) + " из " + total;
        }

        /// <summary>
        /// Shows or hides the timer.
        /// </summary>
        /// <param name="visible">True to show the timer.</param>
        public void SetTimerVisible(bool visible)
        {
            _timerRoot.SetActive(visible);
        }

        /// <summary>
        /// Shows the remaining time as minutes and seconds.
        /// </summary>
        /// <param name="seconds">Remaining time in seconds.</param>
        public void SetTimer(float seconds)
        {
            int whole = Mathf.CeilToInt(seconds);
            _timerText.text = (whole / 60).ToString("00") + ":" + (whole % 60).ToString("00");
            _timerText.color = seconds <= 10f ? Palette.Coral : Palette.Cream;
        }
    }
}
