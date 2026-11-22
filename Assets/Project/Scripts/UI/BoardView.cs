using System;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Builds a square grid of <see cref="BoardCell"/> objects that fits into a fixed board area.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private const float Spacing = 12f;
        private const float Padding = 18f;

        [SerializeField] private RectTransform _cellContainer;
        [SerializeField] private BoardCell _cellTemplate;
        [SerializeField] private float _boardSize = 900f;

        /// <summary>Rectangle that contains all cells.</summary>
        public RectTransform CellContainer => _cellContainer;

        /// <summary>Cells created by the last call to <see cref="Build"/>.</summary>
        public BoardCell[,] Cells { get; private set; }

        /// <summary>Edge length of one cell in pixels.</summary>
        public float CellSize { get; private set; }

        /// <summary>Distance between neighbouring cell centres in pixels.</summary>
        public float CellStep => CellSize + Spacing;

        /// <summary>
        /// Removes old cells and creates a new grid.
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="onClick">Callback invoked with the row and column of a tapped cell.</param>
        /// <returns>The created cells indexed by row and column.</returns>
        public BoardCell[,] Build(int rows, int cols, Action<int, int> onClick)
        {
            for (int i = _cellContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_cellContainer.GetChild(i).gameObject);
            }

            int largest = Mathf.Max(rows, cols);
            CellSize = Mathf.Floor((_boardSize - (2f * Padding) - (Spacing * (largest - 1))) / largest);
            GridLayoutGroup grid = _cellContainer.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellSize, CellSize);
            grid.spacing = new Vector2(Spacing, Spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
            Vector2 size = new Vector2(
                (cols * CellSize) + ((cols - 1) * Spacing),
                (rows * CellSize) + ((rows - 1) * Spacing));
            _cellContainer.sizeDelta = size;

            Cells = new BoardCell[rows, cols];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    BoardCell cell = Instantiate(_cellTemplate, _cellContainer);
                    cell.gameObject.SetActive(true);
                    cell.name = "Cell_" + row + "_" + col;
                    cell.Setup(row, col, onClick);
                    Cells[row, col] = cell;
                }
            }

            return Cells;
        }
    }
}
