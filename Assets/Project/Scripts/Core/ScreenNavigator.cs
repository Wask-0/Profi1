using System;
using System.Collections;
using Labyrinthino.UI;
using UnityEngine;

namespace Labyrinthino.Core
{
    /// <summary>
    /// Names of all windows of the game.
    /// </summary>
    public enum ScreenId
    {
        /// <summary>Loading screen shown at start.</summary>
        Loading,

        /// <summary>Main screen with the map of mini-games.</summary>
        MiniGames,

        /// <summary>Card of a selected mini-game with description and levels.</summary>
        MiniGameDetails,

        /// <summary>Audio settings.</summary>
        Settings,

        /// <summary>Quiz mini-game.</summary>
        Quiz,

        /// <summary>Find the Path mini-game.</summary>
        FindPath,

        /// <summary>Meadow mini-game.</summary>
        Meadow,

        /// <summary>Connect the Dots mini-game.</summary>
        ConnectDots,

        /// <summary>Victory window.</summary>
        Victory,

        /// <summary>Defeat window.</summary>
        Defeat
    }

    /// <summary>
    /// Binds a screen identifier to its canvas group.
    /// </summary>
    [Serializable]
    public class ScreenEntry
    {
        /// <summary>Identifier of the window.</summary>
        public ScreenId Id;

        /// <summary>Canvas group that holds the whole window.</summary>
        public CanvasGroup Group;
    }

    /// <summary>
    /// Shows one window at a time with a short cross fade.
    /// </summary>
    public sealed class ScreenNavigator : MonoBehaviour
    {
        private const float FadeSeconds = 0.18f;

        [SerializeField] private ScreenEntry[] _screens;

        private ScreenEntry _current;
        private Coroutine _transition;

        /// <summary>Identifier of the window that is shown now.</summary>
        public ScreenId CurrentId => _current != null ? _current.Id : ScreenId.Loading;

        /// <summary>
        /// Shows a window without animation. Used for the first screen.
        /// </summary>
        /// <param name="id">Window to show.</param>
        public void ShowImmediate(ScreenId id)
        {
            foreach (ScreenEntry entry in _screens)
            {
                bool isTarget = entry.Id == id;
                entry.Group.gameObject.SetActive(isTarget);
                entry.Group.alpha = isTarget ? 1f : 0f;
                entry.Group.interactable = isTarget;
                entry.Group.blocksRaycasts = isTarget;
                if (isTarget)
                {
                    _current = entry;
                }
            }
        }

        /// <summary>
        /// Switches to another window with a fade.
        /// </summary>
        /// <param name="id">Window to show.</param>
        public void Show(ScreenId id)
        {
            if (_transition != null)
            {
                StopCoroutine(_transition);
                FinishPendingTransition();
            }

            _transition = StartCoroutine(Transition(Find(id)));
        }

        private ScreenEntry Find(ScreenId id)
        {
            foreach (ScreenEntry entry in _screens)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }

            throw new ArgumentException("Unknown screen " + id);
        }

        private void FinishPendingTransition()
        {
            foreach (ScreenEntry entry in _screens)
            {
                if (entry != _current)
                {
                    entry.Group.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator Transition(ScreenEntry target)
        {
            ScreenEntry previous = _current;
            _current = target;
            if (previous != null && previous != target)
            {
                previous.Group.interactable = false;
                previous.Group.blocksRaycasts = false;
                yield return UiTween.Fade(previous.Group, 0f, FadeSeconds);
                previous.Group.gameObject.SetActive(false);
            }

            target.Group.alpha = 0f;
            target.Group.gameObject.SetActive(true);
            target.Group.interactable = true;
            target.Group.blocksRaycasts = true;
            yield return UiTween.Fade(target.Group, 1f, FadeSeconds);
            _transition = null;
        }
    }
}
