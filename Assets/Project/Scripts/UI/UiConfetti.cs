using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Particle effect made of UI images: confetti for victories and falling drops for defeats.
    /// Works in a screen space overlay canvas where the particle system component is not drawn.
    /// </summary>
    public sealed class UiConfetti : MonoBehaviour
    {
        private const float Gravity = -1500f;

        [SerializeField] private RectTransform _area;
        [SerializeField] private Sprite _pieceSprite;

        private readonly List<Piece> _pieces = new List<Piece>();

        private struct Piece
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Velocity;
            public float Spin;
            public float Life;
            public float Drag;
        }

        /// <summary>
        /// Shoots colourful pieces upwards from the centre of the area.
        /// </summary>
        /// <param name="count">Number of pieces to create.</param>
        public void Celebrate(int count)
        {
            Color[] colors = { Palette.Gold, Palette.Teal, Palette.Coral, Palette.Cream, Palette.GameColors[2] };
            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(40f, 140f) * Mathf.Deg2Rad;
                float speed = Random.Range(900f, 2100f);
                Spawn(Vector2.zero, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed,
                    colors[Random.Range(0, colors.Length)], Random.Range(16f, 30f), 0.4f);
            }
        }

        /// <summary>
        /// Lets grey blue drops fall from the top of the area.
        /// </summary>
        /// <param name="count">Number of drops to create.</param>
        public void Rain(int count)
        {
            Rect rect = _area.rect;
            for (int i = 0; i < count; i++)
            {
                Vector2 start = new Vector2(Random.Range(rect.xMin, rect.xMax), rect.yMax + Random.Range(0f, 400f));
                Spawn(start, new Vector2(0f, Random.Range(-900f, -500f)), new Color32(150, 190, 255, 220),
                    Random.Range(10f, 18f), 0f);
            }
        }

        /// <summary>
        /// Removes all pieces immediately.
        /// </summary>
        public void Clear()
        {
            foreach (Piece piece in _pieces)
            {
                if (piece.Rect != null)
                {
                    Destroy(piece.Rect.gameObject);
                }
            }

            _pieces.Clear();
        }

        private void Spawn(Vector2 position, Vector2 velocity, Color color, float size, float drag)
        {
            GameObject instance = new GameObject("Piece", typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)instance.transform;
            rect.SetParent(_area, false);
            rect.sizeDelta = new Vector2(size, size * Random.Range(0.6f, 1.6f));
            rect.anchoredPosition = position;
            Image image = instance.GetComponent<Image>();
            image.sprite = _pieceSprite;
            image.color = color;
            image.raycastTarget = false;
            _pieces.Add(new Piece
            {
                Rect = rect,
                Image = image,
                Velocity = velocity,
                Spin = Random.Range(-360f, 360f),
                Life = 2.6f,
                Drag = drag
            });
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            float bottom = _area.rect.yMin - 60f;
            for (int i = _pieces.Count - 1; i >= 0; i--)
            {
                Piece piece = _pieces[i];
                piece.Life -= delta;
                bool hasGravity = piece.Drag > 0f;
                if (hasGravity)
                {
                    piece.Velocity.y += Gravity * delta;
                    piece.Velocity.x *= 1f - (piece.Drag * delta);
                }

                piece.Rect.anchoredPosition += piece.Velocity * delta;
                piece.Rect.Rotate(0f, 0f, piece.Spin * delta);
                if (piece.Life <= 0f || piece.Rect.anchoredPosition.y < bottom)
                {
                    Destroy(piece.Rect.gameObject);
                    _pieces.RemoveAt(i);
                    continue;
                }

                _pieces[i] = piece;
            }
        }
    }
}
