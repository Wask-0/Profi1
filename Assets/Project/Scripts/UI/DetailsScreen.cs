using System;
using Labyrinthino.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Window of a selected mini-game with its card, description and the list of levels.
    /// </summary>
    public sealed class DetailsScreen : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _description;
        [SerializeField] private Text _statusText;
        [SerializeField] private Image _icon;
        [SerializeField] private Sprite[] _icons;
        [SerializeField] private RectTransform _levelsRoot;
        [SerializeField] private GameObject _levelChipTemplate;
        [SerializeField] private Sprite _checkSprite;
        [SerializeField] private Button _playButton;
        [SerializeField] private Text _playLabel;
        [SerializeField] private Image _playBackground;
        [SerializeField] private Button _backButton;

        /// <summary>
        /// Fills the window for a mini-game.
        /// </summary>
        /// <param name="info">Description of the mini-game.</param>
        /// <param name="progress">Saved progress of the mini-game.</param>
        /// <param name="onPlay">Called when the play button is pressed.</param>
        /// <param name="onBack">Called when the back button is pressed.</param>
        public void Bind(MiniGameInfo info, MiniGameProgress progress, Action onPlay, Action onBack)
        {
            Color accent = Palette.GameColors[(int)info.Id];
            bool isComplete = progress.LevelsCompleted >= info.LevelCount;
            _title.text = info.Title;
            _description.text = info.Description;
            _icon.sprite = _icons[(int)info.Id];
            BuildLevelChips(info, progress, accent);

            _playButton.interactable = !isComplete;
            _playBackground.color = isComplete ? Palette.Locked : Palette.Gold;
            if (isComplete)
            {
                _playLabel.text = "Пройдено";
                _statusText.text = "Все уровни пройдены. Повторно играть нельзя.";
            }
            else if (progress.LevelsCompleted > 0 || progress.HasSnapshot)
            {
                _playLabel.text = "Продолжить";
                _statusText.text = "Следующий уровень: " + (progress.LevelsCompleted + 1);
            }
            else
            {
                _playLabel.text = "Играть";
                _statusText.text = "Уровней: " + info.LevelCount;
            }

            _playButton.onClick.RemoveAllListeners();
            _playButton.onClick.AddListener(() => onPlay());
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(() => onBack());
        }

        private void BuildLevelChips(MiniGameInfo info, MiniGameProgress progress, Color accent)
        {
            for (int i = _levelsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_levelsRoot.GetChild(i).gameObject);
            }

            for (int level = 0; level < info.LevelCount; level++)
            {
                GameObject chip = Instantiate(_levelChipTemplate, _levelsRoot);
                chip.SetActive(true);
                Image background = chip.GetComponent<Image>();
                Text label = chip.GetComponentInChildren<Text>();
                Image mark = chip.transform.Find("Mark").GetComponent<Image>();
                bool done = level < progress.LevelsCompleted;
                bool current = level == progress.LevelsCompleted;
                background.color = done ? Palette.Grass : (current ? accent : Palette.Locked);
                label.text = done ? string.Empty : (level + 1).ToString();
                mark.sprite = _checkSprite;
                mark.enabled = done;
            }
        }
    }
}
