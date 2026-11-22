using System;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// One tappable cell of a square board used by the grid based mini-games.
    /// </summary>
    public sealed class BoardCell : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _icon;
        [SerializeField] private Button _button;

        private Action<int, int> _onClick;

        /// <summary>Zero based row of the cell.</summary>
        public int Row { get; private set; }

        /// <summary>Zero based column of the cell.</summary>
        public int Col { get; private set; }

        /// <summary>Rectangle of the cell, used to place route lines and animations.</summary>
        public RectTransform Rect => (RectTransform)transform;

        /// <summary>Rectangle of the icon in the middle of the cell.</summary>
        public RectTransform IconRect => (RectTransform)_icon.transform;

        /// <summary>Background image of the cell.</summary>
        public Image Background => _background;

        /// <summary>
        /// Prepares the cell after it was created.
        /// </summary>
        /// <param name="row">Row of the cell.</param>
        /// <param name="col">Column of the cell.</param>
        /// <param name="onClick">Callback that receives the row and column when the cell is tapped.</param>
        public void Setup(int row, int col, Action<int, int> onClick)
        {
            Row = row;
            Col = col;
            _onClick = onClick;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(HandleClick);
            SetIcon(null, Color.white);
        }

        /// <summary>
        /// Changes the background colour.
        /// </summary>
        /// <param name="color">New colour.</param>
        public void SetColor(Color color)
        {
            _background.color = color;
        }

        /// <summary>
        /// Shows or hides an icon in the middle of the cell.
        /// </summary>
        /// <param name="sprite">Icon sprite or null to hide the icon.</param>
        /// <param name="color">Tint of the icon.</param>
        public void SetIcon(Sprite sprite, Color color)
        {
            _icon.sprite = sprite;
            _icon.color = color;
            _icon.enabled = sprite != null;
        }

        private void HandleClick()
        {
            _onClick?.Invoke(Row, Col);
        }
    }
}
