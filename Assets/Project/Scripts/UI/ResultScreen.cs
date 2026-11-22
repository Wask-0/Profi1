using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Victory or defeat window with an animated emblem, stars, particles and two buttons.
    /// </summary>
    public sealed class ResultScreen : MonoBehaviour
    {
        [SerializeField] private bool _isVictory;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private RectTransform _emblem;
        [SerializeField] private RectTransform _rays;
        [SerializeField] private Text _title;
        [SerializeField] private Text _subtitle;
        [SerializeField] private RectTransform[] _stars;
        [SerializeField] private Button _primaryButton;
        [SerializeField] private Text _primaryLabel;
        [SerializeField] private Button _secondaryButton;
        [SerializeField] private Text _secondaryLabel;
        [SerializeField] private UiConfetti _confetti;
        [SerializeField] private CanvasGroup _buttonsGroup;

        private Vector2 _resting;
        private bool _hasResting;

        /// <summary>
        /// Sets the texts and button actions of the window.
        /// </summary>
        /// <param name="subtitle">Line that explains the result.</param>
        /// <param name="primaryLabel">Text of the main button.</param>
        /// <param name="onPrimary">Called when the main button is pressed.</param>
        /// <param name="secondaryLabel">Text of the secondary button.</param>
        /// <param name="onSecondary">Called when the secondary button is pressed.</param>
        public void Configure(string subtitle, string primaryLabel, Action onPrimary, string secondaryLabel,
            Action onSecondary)
        {
            _subtitle.text = subtitle;
            _primaryLabel.text = primaryLabel;
            _secondaryLabel.text = secondaryLabel;
            _primaryButton.onClick.RemoveAllListeners();
            _primaryButton.onClick.AddListener(() => onPrimary());
            _secondaryButton.onClick.RemoveAllListeners();
            _secondaryButton.onClick.AddListener(() => onSecondary());
        }

        private void OnEnable()
        {
            StartCoroutine(PlayIntro());
        }

        private void OnDisable()
        {
            _confetti.Clear();
        }

        private void Update()
        {
            if (_isVictory)
            {
                _rays.Rotate(0f, 0f, -18f * Time.unscaledDeltaTime);
            }
        }

        private IEnumerator PlayIntro()
        {
            _buttonsGroup.alpha = 0f;
            _title.transform.localScale = Vector3.zero;
            _subtitle.transform.localScale = Vector3.zero;
            _emblem.localScale = Vector3.zero;
            foreach (RectTransform star in _stars)
            {
                star.localScale = Vector3.zero;
            }

            if (!_hasResting)
            {
                _resting = _panel.anchoredPosition;
                _hasResting = true;
            }

            Vector2 resting = _resting;
            yield return UiTween.Move(_panel, resting + (Vector2.down * 500f), resting, 0.4f);
            StartCoroutine(UiTween.Scale(_emblem, Vector3.zero, Vector3.one, 0.6f));
            yield return new WaitForSecondsRealtime(0.25f);
            StartCoroutine(UiTween.Scale(_title.transform, Vector3.zero, Vector3.one, 0.5f));
            if (_isVictory)
            {
                _confetti.Celebrate(110);
            }
            else
            {
                _confetti.Rain(70);
            }

            yield return new WaitForSecondsRealtime(0.2f);
            StartCoroutine(UiTween.Scale(_subtitle.transform, Vector3.zero, Vector3.one, 0.4f));
            foreach (RectTransform star in _stars)
            {
                StartCoroutine(UiTween.Scale(star, Vector3.zero, Vector3.one, 0.4f));
                yield return new WaitForSecondsRealtime(0.15f);
            }

            yield return UiTween.Fade(_buttonsGroup, 1f, 0.3f);
        }
    }
}
