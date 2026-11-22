using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// First window of the game: animated logo, spinner and a progress bar that names
    /// the current loading stage.
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        private static readonly string[] Stages =
        {
            "Загрузка ресурсов...",
            "Чтение сохранений...",
            "Построение лабиринтов...",
            "Всё готово!"
        };

        [SerializeField] private RectTransform _logo;
        [SerializeField] private RectTransform _spinner;
        [SerializeField] private Text _stageText;
        [SerializeField] private Text _percentText;
        [SerializeField] private ProgressBar _progressBar;

        private bool _isRunning;

        /// <summary>
        /// Plays the loading sequence.
        /// </summary>
        /// <param name="onFinished">Called when the progress bar is full.</param>
        public void Run(Action onFinished)
        {
            StartCoroutine(LoadingRoutine(onFinished));
        }

        private IEnumerator LoadingRoutine(Action onFinished)
        {
            _isRunning = true;
            _progressBar.SetValue(0f);
            StartCoroutine(UiTween.Scale(_logo, Vector3.one * 0.3f, Vector3.one, 0.8f));
            const float stageSeconds = 0.65f;
            for (int stage = 0; stage < Stages.Length; stage++)
            {
                _stageText.text = Stages[stage];
                float from = (float)stage / Stages.Length;
                float to = (float)(stage + 1) / Stages.Length;
                for (float time = 0f; time < stageSeconds; time += Time.unscaledDeltaTime)
                {
                    float value = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, time / stageSeconds));
                    _progressBar.SetValue(value);
                    _percentText.text = Mathf.RoundToInt(value * 100f) + "%";
                    yield return null;
                }
            }

            _progressBar.SetValue(1f);
            _percentText.text = "100%";
            yield return new WaitForSecondsRealtime(0.3f);
            _isRunning = false;
            onFinished?.Invoke();
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            _spinner.Rotate(0f, 0f, -220f * Time.unscaledDeltaTime);
            float pulse = 1f + (Mathf.Sin(Time.unscaledTime * 3f) * 0.03f);
            if (_progressBar.Value > 0.15f)
            {
                _logo.localScale = Vector3.one * pulse;
            }
        }
    }
}
