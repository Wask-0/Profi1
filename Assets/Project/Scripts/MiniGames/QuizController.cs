using System.Collections;
using Labyrinthino.Audio;
using Labyrinthino.Data;
using Labyrinthino.Logic;
using Labyrinthino.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.MiniGames
{
    /// <summary>
    /// Quiz mini-game: guess the word from a picture that is revealed piece by piece.
    /// </summary>
    public sealed class QuizController : MiniGameBase
    {
        private const float ResultPauseSeconds = 2.2f;

        private static readonly int[][] RevealedTiles =
        {
            new[] { 5, 6, 9, 10 },
            new[] { 4, 5, 6, 7, 8, 9, 10, 11 },
            new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
            new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }
        };

        [SerializeField] private Image _picture;
        [SerializeField] private Sprite[] _pictures;
        [SerializeField] private CanvasGroup[] _maskTiles;
        [SerializeField] private Text _messageText;
        [SerializeField] private Image[] _hearts;
        [SerializeField] private InputField _answerField;
        [SerializeField] private RectTransform _lettersRoot;
        [SerializeField] private Button _letterTemplate;
        [SerializeField] private Button _eraseButton;
        [SerializeField] private Button _submitButton;

        private QuizModel _model;
        private Button[] _letterButtons;
        private int _shownStage;

        /// <inheritdoc />
        public override MiniGameId Id => MiniGameId.Quiz;

        private void Awake()
        {
            _eraseButton.onClick.AddListener(OnEraseClicked);
            _submitButton.onClick.AddListener(OnSubmitClicked);
        }

        /// <inheritdoc />
        protected override void LoadLevel(bool restoreSnapshot)
        {
            string word = MiniGameCatalog.QuizWords[LevelIndex];
            int extraLetters = 2 + (LevelIndex % 2);
            _model = new QuizModel(word, extraLetters, (LevelIndex * 13) + 5, MiniGameCatalog.QuizAttempts);
            float seconds = MiniGameCatalog.QuizSecondsPerLevel;
            if (restoreSnapshot)
            {
                _model.RestoreAttempts(Progress.Counter);
                seconds = Progress.TimeLeft;
            }

            _picture.sprite = _pictures[LevelIndex];
            _messageText.text = "Что это? Собери слово из букв";
            _messageText.color = Palette.Cream;
            _shownStage = _model.RevealStage;
            ApplyReveal(_shownStage, false);
            BuildLetterButtons();
            RefreshTyped();
            RefreshHearts();
            Header.SetTimerVisible(true);
            Header.SetTimer(seconds);
            Timer.Start(seconds);
        }

        /// <inheritdoc />
        protected override void WriteSnapshot(MiniGameProgress progress)
        {
            progress.Counter = _model.AttemptsLeft;
        }

        /// <inheritdoc />
        protected override void OnTimeExpired()
        {
            if (InputLocked)
            {
                return;
            }

            FailLevel("Время вышло! Это — " + _model.Answer);
        }

        private void BuildLetterButtons()
        {
            for (int i = _lettersRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_lettersRoot.GetChild(i).gameObject);
            }

            _letterButtons = new Button[_model.Letters.Count];
            for (int i = 0; i < _letterButtons.Length; i++)
            {
                int index = i;
                Button button = Instantiate(_letterTemplate, _lettersRoot);
                button.gameObject.SetActive(true);
                button.GetComponentInChildren<Text>().text = _model.Letters[i].ToString();
                button.onClick.AddListener(() => OnLetterClicked(index));
                _letterButtons[i] = button;
            }
        }

        private void OnLetterClicked(int index)
        {
            if (InputLocked || !_model.TryTypeLetter(index))
            {
                return;
            }

            PlaySound(SoundId.Tap);
            RefreshTyped();
        }

        private void OnEraseClicked()
        {
            if (!InputLocked && _model.Erase())
            {
                RefreshTyped();
            }
        }

        private void OnSubmitClicked()
        {
            if (InputLocked)
            {
                return;
            }

            switch (_model.Submit())
            {
                case QuizSubmitResult.Empty:
                    ShowMessage("Введи слово целиком", Palette.Gold);
                    PlaySound(SoundId.Error);
                    StartCoroutine(UiTween.Shake((RectTransform)_answerField.transform, 18f, 0.3f));
                    break;
                case QuizSubmitResult.Correct:
                    StartCoroutine(CorrectRoutine());
                    break;
                case QuizSubmitResult.Wrong:
                    OnWrongAnswer();
                    break;
                default:
                    FailLevel("Попытки закончились. Это — " + _model.Answer);
                    break;
            }
        }

        private void OnWrongAnswer()
        {
            PlaySound(SoundId.Error);
            ShowMessage("Неверно! Осталось попыток: " + _model.AttemptsLeft, Palette.Coral);
            StartCoroutine(UiTween.Shake((RectTransform)_answerField.transform, 18f, 0.3f));
            RefreshTyped();
            RefreshHearts();
            ApplyReveal(_model.RevealStage, true);
        }

        private IEnumerator CorrectRoutine()
        {
            InputLocked = true;
            Timer.Stop();
            LevelActive = false;
            PlaySound(SoundId.Correct);
            ApplyReveal(3, true);
            ShowMessage("Верно! Это — " + _model.Answer, Palette.Teal);
            Confetti.Celebrate(40);
            yield return new WaitForSecondsRealtime(ResultPauseSeconds);
            AdvanceAfterLevel();
        }

        private void FailLevel(string message)
        {
            InputLocked = true;
            Timer.Stop();
            LevelActive = false;
            PlaySound(SoundId.Error);
            RefreshHearts();
            ApplyReveal(3, true);
            ShowMessage(message, Palette.Coral);
            RunAfter(ResultPauseSeconds, AdvanceAfterFailedLevel);
        }

        private void ShowMessage(string text, Color color)
        {
            _messageText.text = text;
            _messageText.color = color;
            StartCoroutine(UiTween.Scale(_messageText.transform, Vector3.one * 0.7f, Vector3.one, 0.3f));
        }

        private void RefreshTyped()
        {
            _answerField.text = _model.Typed;
            for (int i = 0; i < _letterButtons.Length; i++)
            {
                _letterButtons[i].interactable = !_model.IsUsed(i);
            }
        }

        private void RefreshHearts()
        {
            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].color = i < _model.AttemptsLeft ? Palette.Coral : new Color(1f, 1f, 1f, 0.2f);
            }
        }

        private void ApplyReveal(int stage, bool animate)
        {
            int[] visible = RevealedTiles[stage];
            for (int tile = 0; tile < _maskTiles.Length; tile++)
            {
                bool shouldShow = System.Array.IndexOf(visible, tile) >= 0;
                bool wasShown = System.Array.IndexOf(RevealedTiles[_shownStage], tile) >= 0;
                if (animate && shouldShow && !wasShown)
                {
                    StartCoroutine(UiTween.Fade(_maskTiles[tile], 0f, 0.35f + (tile * 0.02f)));
                }
                else
                {
                    _maskTiles[tile].alpha = shouldShow ? 0f : 1f;
                }
            }

            _shownStage = stage;
        }
    }
}
