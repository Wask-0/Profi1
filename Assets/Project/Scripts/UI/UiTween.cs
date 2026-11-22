using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Small collection of coroutine based animations for UI elements. All of them use
    /// unscaled time so they keep working while the game is paused.
    /// </summary>
    public static class UiTween
    {
        /// <summary>
        /// Smooth ease-out curve with a small overshoot.
        /// </summary>
        /// <param name="t">Normalised time from 0 to 1.</param>
        /// <returns>Eased value that may briefly exceed 1.</returns>
        public static float EaseOutBack(float t)
        {
            const float overshoot = 1.70158f;
            float shifted = t - 1f;
            return 1f + (shifted * shifted * (((overshoot + 1f) * shifted) + overshoot));
        }

        /// <summary>
        /// Fades a canvas group to a target alpha.
        /// </summary>
        /// <param name="group">Group to fade.</param>
        /// <param name="target">Final alpha from 0 to 1.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <returns>Coroutine enumerator.</returns>
        public static IEnumerator Fade(CanvasGroup group, float target, float duration)
        {
            float start = group.alpha;
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(start, target, time / duration);
                yield return null;
            }

            group.alpha = target;
        }

        /// <summary>
        /// Scales a transform from one scale to another with an overshoot.
        /// </summary>
        /// <param name="target">Transform to animate.</param>
        /// <param name="from">Starting scale.</param>
        /// <param name="to">Final scale.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <returns>Coroutine enumerator.</returns>
        public static IEnumerator Scale(Transform target, Vector3 from, Vector3 to, float duration)
        {
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                if (target == null)
                {
                    yield break;
                }

                target.localScale = Vector3.LerpUnclamped(from, to, EaseOutBack(time / duration));
                yield return null;
            }

            if (target != null)
            {
                target.localScale = to;
            }
        }

        /// <summary>
        /// Makes an element pop in from zero size.
        /// </summary>
        /// <param name="target">Transform to animate.</param>
        /// <param name="delay">Seconds to wait before the animation starts.</param>
        /// <param name="duration">Duration of the pop in seconds.</param>
        /// <returns>Coroutine enumerator.</returns>
        public static IEnumerator PopIn(Transform target, float delay, float duration)
        {
            target.localScale = Vector3.zero;
            yield return new WaitForSecondsRealtime(delay);
            yield return Scale(target, Vector3.zero, Vector3.one, duration);
        }

        /// <summary>
        /// Shakes a rectangle horizontally, for example to signal an error.
        /// </summary>
        /// <param name="target">Rectangle to shake.</param>
        /// <param name="strength">Maximum offset in pixels.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <returns>Coroutine enumerator.</returns>
        public static IEnumerator Shake(RectTransform target, float strength, float duration)
        {
            Vector2 origin = target.anchoredPosition;
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                if (target == null)
                {
                    yield break;
                }

                float fade = 1f - (time / duration);
                target.anchoredPosition = origin + (Vector2.right * Mathf.Sin(time * 60f) * strength * fade);
                yield return null;
            }

            if (target != null)
            {
                target.anchoredPosition = origin;
            }
        }

        /// <summary>
        /// Blends the colour of a graphic to a target colour.
        /// </summary>
        /// <param name="graphic">Graphic to tint.</param>
        /// <param name="target">Final colour.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <returns>Coroutine enumerator.</returns>
        public static IEnumerator ColorTo(Graphic graphic, Color target, float duration)
        {
            Color start = graphic.color;
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                if (graphic == null)
                {
                    yield break;
                }

                graphic.color = Color.Lerp(start, target, time / duration);
                yield return null;
            }

            if (graphic != null)
            {
                graphic.color = target;
            }
        }

        /// <summary>
        /// Moves a rectangle between two anchored positions with an ease-out curve.
        /// </summary>
        /// <param name="target">Rectangle to move.</param>
        /// <param name="from">Start anchored position.</param>
        /// <param name="to">Final anchored position.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <returns>Coroutine enumerator.</returns>
        public static IEnumerator Move(RectTransform target, Vector2 from, Vector2 to, float duration)
        {
            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                float t = time / duration;
                target.anchoredPosition = Vector2.LerpUnclamped(from, to, 1f - ((1f - t) * (1f - t)));
                yield return null;
            }

            target.anchoredPosition = to;
        }
    }
}
