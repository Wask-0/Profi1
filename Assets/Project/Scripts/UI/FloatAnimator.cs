using UnityEngine;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Makes an element gently bob up and down and breathe in size.
    /// </summary>
    public sealed class FloatAnimator : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 10f;
        [SerializeField] private float _speed = 1.6f;
        [SerializeField] private float _scaleAmount = 0.02f;
        [SerializeField] private float _phase;

        private RectTransform _rect;
        private Vector2 _basePosition;

        /// <summary>
        /// Changes the animation parameters at runtime.
        /// </summary>
        /// <param name="amplitude">Vertical movement in pixels.</param>
        /// <param name="speed">Animation speed in radians per second.</param>
        /// <param name="scaleAmount">Relative size change.</param>
        /// <param name="phase">Phase offset in radians.</param>
        public void Configure(float amplitude, float speed, float scaleAmount, float phase)
        {
            _amplitude = amplitude;
            _speed = speed;
            _scaleAmount = scaleAmount;
            _phase = phase;
        }

        private void OnEnable()
        {
            _rect = (RectTransform)transform;
            _basePosition = _rect.anchoredPosition;
        }

        private void OnDisable()
        {
            if (_rect != null)
            {
                _rect.anchoredPosition = _basePosition;
                _rect.localScale = Vector3.one;
            }
        }

        private void Update()
        {
            float wave = Mathf.Sin((Time.unscaledTime * _speed) + _phase);
            _rect.anchoredPosition = _basePosition + (Vector2.up * wave * _amplitude);
            if (_scaleAmount > 0f)
            {
                _rect.localScale = Vector3.one * (1f + (wave * _scaleAmount));
            }
        }
    }
}
