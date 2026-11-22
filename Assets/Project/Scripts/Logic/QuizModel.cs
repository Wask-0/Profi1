using System;
using System.Collections.Generic;
using System.Text;

namespace Labyrinthino.Logic
{
    /// <summary>
    /// Outcome of submitting an answer in the quiz.
    /// </summary>
    public enum QuizSubmitResult
    {
        /// <summary>Nothing was typed or the word is incomplete.</summary>
        Empty,

        /// <summary>The answer is right.</summary>
        Correct,

        /// <summary>The answer is wrong and attempts remain.</summary>
        Wrong,

        /// <summary>The answer is wrong and it was the last attempt.</summary>
        OutOfAttempts
    }

    /// <summary>
    /// Rules of one quiz level: shuffled letter buttons, the typed text and the attempts counter.
    /// </summary>
    public sealed class QuizModel
    {
        private const string ExtraLetterPool = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЭЮЯ";

        private readonly List<char> _letters = new List<char>();
        private readonly List<bool> _used = new List<bool>();
        private readonly List<int> _typedIndices = new List<int>();

        /// <summary>
        /// Creates a level.
        /// </summary>
        /// <param name="answer">The correct word in upper case.</param>
        /// <param name="extraLetters">How many wrong letters to add (2 or 3 per the design).</param>
        /// <param name="seed">Seed that makes the shuffle reproducible.</param>
        /// <param name="attempts">Attempts the player has.</param>
        public QuizModel(string answer, int extraLetters, int seed, int attempts)
        {
            Answer = answer;
            AttemptsLeft = attempts;
            TotalAttempts = attempts;
            Random random = new Random(seed);
            foreach (char letter in answer)
            {
                _letters.Add(letter);
            }

            while (_letters.Count < answer.Length + extraLetters)
            {
                char extra = ExtraLetterPool[random.Next(ExtraLetterPool.Length)];
                if (!answer.Contains(extra.ToString()) && !_letters.Contains(extra))
                {
                    _letters.Add(extra);
                }
            }

            for (int i = _letters.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_letters[i], _letters[j]) = (_letters[j], _letters[i]);
            }

            foreach (char unused in _letters)
            {
                _used.Add(false);
            }
        }

        /// <summary>The correct word.</summary>
        public string Answer { get; }

        /// <summary>Attempts that are still available.</summary>
        public int AttemptsLeft { get; private set; }

        /// <summary>Letters offered on the buttons in display order.</summary>
        public IReadOnlyList<char> Letters => _letters;

        /// <summary>Text typed so far.</summary>
        public string Typed
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (int index in _typedIndices)
                {
                    builder.Append(_letters[index]);
                }

                return builder.ToString();
            }
        }

        /// <summary>Quarter of the picture that is revealed, from 0 (25 percent) to 3 (full picture).</summary>
        public int RevealStage => Math.Min(3, TotalAttempts - AttemptsLeft);

        private int TotalAttempts { get; }

        /// <summary>
        /// Checks whether a letter button is already used.
        /// </summary>
        /// <param name="index">Index of the letter button.</param>
        /// <returns>True when the letter is in the typed text.</returns>
        public bool IsUsed(int index)
        {
            return _used[index];
        }

        /// <summary>
        /// Appends the letter of a button to the typed text.
        /// </summary>
        /// <param name="index">Index of the letter button.</param>
        /// <returns>True when the letter was added.</returns>
        public bool TryTypeLetter(int index)
        {
            if (index < 0 || index >= _letters.Count || _used[index] || _typedIndices.Count >= Answer.Length)
            {
                return false;
            }

            _used[index] = true;
            _typedIndices.Add(index);
            return true;
        }

        /// <summary>
        /// Removes the last typed letter and frees its button.
        /// </summary>
        /// <returns>True when a letter was removed.</returns>
        public bool Erase()
        {
            if (_typedIndices.Count == 0)
            {
                return false;
            }

            int last = _typedIndices[_typedIndices.Count - 1];
            _typedIndices.RemoveAt(_typedIndices.Count - 1);
            _used[last] = false;
            return true;
        }

        /// <summary>
        /// Sets the number of attempts left, used when restoring a saved game.
        /// </summary>
        /// <param name="attemptsLeft">Attempts left; clamped to the total.</param>
        public void RestoreAttempts(int attemptsLeft)
        {
            AttemptsLeft = Math.Max(0, Math.Min(TotalAttempts, attemptsLeft));
        }

        /// <summary>
        /// Checks the typed text against the answer and updates the attempts.
        /// </summary>
        /// <returns>The result of the check.</returns>
        public QuizSubmitResult Submit()
        {
            if (Typed.Length != Answer.Length)
            {
                return QuizSubmitResult.Empty;
            }

            if (Typed == Answer)
            {
                return QuizSubmitResult.Correct;
            }

            AttemptsLeft--;
            ClearTyped();
            return AttemptsLeft <= 0 ? QuizSubmitResult.OutOfAttempts : QuizSubmitResult.Wrong;
        }

        private void ClearTyped()
        {
            _typedIndices.Clear();
            for (int i = 0; i < _used.Count; i++)
            {
                _used[i] = false;
            }
        }
    }
}
