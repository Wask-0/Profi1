using UnityEngine;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Horizontal progress bar built from a stretched fill rectangle.
    /// </summary>
    public sealed class ProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform _fill;

        /// <summary>Current progress from 0 to 1.</summary>
        public float Value { get; private set; }

        /// <summary>
        /// Sets the progress.
        /// </summary>
        /// <param name="value">Progress from 0 to 1; values outside are clamped.</param>
        public void SetValue(float value)
        {
            Value = Mathf.Clamp01(value);
            _fill.anchorMax = new Vector2(Value, 1f);
        }
    }
}
