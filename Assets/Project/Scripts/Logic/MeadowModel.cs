using System;
using System.Collections.Generic;

namespace Labyrinthino.Logic
{
    /// <summary>
    /// Outcome of a tap in the Meadow game.
    /// </summary>
    public enum MeadowTapResult
    {
        /// <summary>Tap had no effect (outside the grid or game already over).</summary>
        Ignored,

        /// <summary>A brown cell turned green.</summary>
        Painted,

        /// <summary>A green cell was tapped again; the state does not change but the move is spent.</summary>
        Wasted
    }

    /// <summary>
    /// Rules of the Meadow mini-game: a grid of brown and green cells where the player
    /// has a limited number of taps to make every cell green.
    /// </summary>
    public sealed class MeadowModel
    {
        private readonly bool[,] _green;

        /// <summary>
        /// Creates a level with a random but reproducible distribution of cells.
        /// </summary>
        /// <param name="rows">Number of rows.</param>
        /// <param name="cols">Number of columns.</param>
        /// <param name="brownCount">How many cells start brown; the rest start green.</param>
        /// <param name="moves">Number of taps the player may spend.</param>
        /// <param name="seed">Seed that makes the distribution reproducible.</param>
        public MeadowModel(int rows, int cols, int brownCount, int moves, int seed)
        {
            Rows = rows;
            Cols = cols;
            MovesLeft = moves;
            _green = new bool[rows, cols];
            List<int> indices = new List<int>();
            for (int i = 0; i < rows * cols; i++)
            {
                indices.Add(i);
            }

            Random random = new Random(seed);
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            for (int i = brownCount; i < indices.Count; i++)
            {
                _green[indices[i] / cols, indices[i] % cols] = true;
            }
        }

        /// <summary>Number of rows.</summary>
        public int Rows { get; }

        /// <summary>Number of columns.</summary>
        public int Cols { get; }

        /// <summary>Taps the player can still spend.</summary>
        public int MovesLeft { get; private set; }

        /// <summary>Number of green cells.</summary>
        public int GreenCount => Count(true);

        /// <summary>Number of brown cells.</summary>
        public int BrownCount => Count(false);

        /// <summary>True when every cell is green.</summary>
        public bool IsComplete => BrownCount == 0;

        /// <summary>True when moves are exhausted and brown cells remain.</summary>
        public bool IsOutOfMoves => !IsComplete && MovesLeft <= 0;

        /// <summary>
        /// Tells whether a cell is green.
        /// </summary>
        /// <param name="row">Row of the cell.</param>
        /// <param name="col">Column of the cell.</param>
        /// <returns>True for green cells.</returns>
        public bool IsGreen(int row, int col)
        {
            return _green[row, col];
        }

        /// <summary>
        /// Taps a cell.
        /// </summary>
        /// <param name="row">Row of the tapped cell.</param>
        /// <param name="col">Column of the tapped cell.</param>
        /// <returns>The effect of the tap.</returns>
        public MeadowTapResult Tap(int row, int col)
        {
            bool outside = row < 0 || row >= Rows || col < 0 || col >= Cols;
            if (outside || IsComplete || MovesLeft <= 0)
            {
                return MeadowTapResult.Ignored;
            }

            MovesLeft--;
            if (_green[row, col])
            {
                return MeadowTapResult.Wasted;
            }

            _green[row, col] = true;
            return MeadowTapResult.Painted;
        }

        /// <summary>
        /// Serialises the board as a string of 0 (brown) and 1 (green) characters.
        /// </summary>
        /// <returns>Row-major board string.</returns>
        public string SerializeBoard()
        {
            char[] chars = new char[Rows * Cols];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = _green[i / Cols, i % Cols] ? '1' : '0';
            }

            return new string(chars);
        }

        /// <summary>
        /// Restores a board saved with <see cref="SerializeBoard"/>.
        /// </summary>
        /// <param name="board">Serialised board; ignored when its length does not match.</param>
        /// <param name="movesLeft">Moves that were left when the board was saved.</param>
        public void RestoreBoard(string board, int movesLeft)
        {
            if (string.IsNullOrEmpty(board) || board.Length != Rows * Cols)
            {
                return;
            }

            for (int i = 0; i < board.Length; i++)
            {
                _green[i / Cols, i % Cols] = board[i] == '1';
            }

            MovesLeft = movesLeft;
        }

        private int Count(bool green)
        {
            int count = 0;
            foreach (bool cell in _green)
            {
                if (cell == green)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
