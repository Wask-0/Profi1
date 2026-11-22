using System;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Window with sliders for music and sound effects volume.
    /// </summary>
    public sealed class SettingsScreen : MonoBehaviour
    {
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _soundSlider;
        [SerializeField] private Text _musicValue;
        [SerializeField] private Text _soundValue;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Text _resetLabel;

        private bool _resetArmed;

        /// <summary>
        /// Connects the window to the game.
        /// </summary>
        /// <param name="music">Current music volume from 0 to 1.</param>
        /// <param name="sound">Current sound volume from 0 to 1.</param>
        /// <param name="onMusicChanged">Called when the music slider moves.</param>
        /// <param name="onSoundChanged">Called when the sound slider moves.</param>
        /// <param name="onBack">Called when the back button is pressed.</param>
        /// <param name="onReset">Called after the player confirmed resetting the progress.</param>
        public void Bind(float music, float sound, Action<float> onMusicChanged, Action<float> onSoundChanged,
            Action onBack, Action onReset)
        {
            _musicSlider.onValueChanged.RemoveAllListeners();
            _soundSlider.onValueChanged.RemoveAllListeners();
            _musicSlider.SetValueWithoutNotify(music);
            _soundSlider.SetValueWithoutNotify(sound);
            UpdateValueTexts();
            _musicSlider.onValueChanged.AddListener(value =>
            {
                onMusicChanged(value);
                UpdateValueTexts();
            });
            _soundSlider.onValueChanged.AddListener(value =>
            {
                onSoundChanged(value);
                UpdateValueTexts();
            });
            _backButton.onClick.RemoveAllListeners();
            _backButton.onClick.AddListener(() => onBack());
            _resetButton.onClick.RemoveAllListeners();
            _resetButton.onClick.AddListener(() => HandleResetClick(onReset));
            DisarmReset();
        }

        private void HandleResetClick(Action onReset)
        {
            if (!_resetArmed)
            {
                _resetArmed = true;
                _resetLabel.text = "Точно сбросить?";
                return;
            }

            DisarmReset();
            onReset();
        }

        private void DisarmReset()
        {
            _resetArmed = false;
            _resetLabel.text = "Сбросить прогресс";
        }

        private void UpdateValueTexts()
        {
            _musicValue.text = Mathf.RoundToInt(_musicSlider.value * 100f) + "%";
            _soundValue.text = Mathf.RoundToInt(_soundSlider.value * 100f) + "%";
        }

        private void OnDisable()
        {
            DisarmReset();
        }
    }
}
