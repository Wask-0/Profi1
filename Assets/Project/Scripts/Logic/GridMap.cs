using System;
using System.Collections.Generic;

namespace Labyrinthino.Logic
{
    /// <summary>
    /// Immutable pair of row and column used to address a grid cell.
    /// </summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        /// <summary>
        /// Creates a position.
        /// </summary>
        /// <param name="row">Zero based row, counted from the top.</param>
        /// <param name="col">Zero based column, counted from the left.</param>
        public GridPos(int row, int col)
        {
            Row = row;
            Col = col;
        }

        /// <summary>Zero based row.</summary>
        public int Row { get; }

        /// <summary>Zero based column.</summary>
        public int Col { get; }

        /// <summary>
        /// Checks whether another position touches this one by a side.
        /// </summary>
        /// <param name="other">Position to compare with.</param>
        /// <returns>True when the Manhattan distance equals one.</returns>
        public bool IsAdjacentTo(GridPos other)
        {
            return Math.Abs(Row - other.Row) + Math.Abs(Col - other.Col) == 1;
        }

        /// <inheritdoc />
        public bool Equals(GridPos other)
        {
            return Row == other.Row && Col == other.Col;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (Row * 397) ^ Col;
        }
    }

    /// <summary>
    /// Rectangular character map parsed from text rows, used to describe levels.
    /// </summary>
    public sealed class GridMap
    {
        /// <summary>Character of an obstacle cell.</summary>
        public const char ObstacleChar = '#';

        private readonly char[,] _cells;

        /// <summary>
        /// Parses a map.
        /// </summary>
        /// <param name="rows">Text rows of equal length; every character is one cell.</param>
        public GridMap(IReadOnlyList<string> rows)
        {
            Rows = rows.Count;
            Cols = rows[0].Length;
            _cells = new char[Rows, Cols];
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    _cells[row, col] = rows[row][col];
                }
            }
        }

        /// <summary>Number of rows.</summary>
        public int Rows { get; }

        /// <summary>Number of columns.</summary>
        public int Cols { get; }

        /// <summary>
        /// Checks that a position lies inside the grid.
        /// </summary>
        /// <param name="row">Row to test.</param>
        /// <param name="col">Column to test.</param>
        /// <returns>True when the cell exists.</returns>
        public bool InBounds(int row, int col)
        {
            return row >= 0 && row < Rows && col >= 0 && col < Cols;
        }

        /// <summary>
        /// Returns the character stored in a cell.
        /// </summary>
        /// <param name="row">Row of the cell.</param>
        /// <param name="col">Column of the cell.</param>
        /// <returns>The map character.</returns>
        public char At(int row, int col)
        {
            return _cells[row, col];
        }

        /// <summary>
        /// Finds all cells that contain a character.
        /// </summary>
        /// <param name="symbol">Character to search for.</param>
        /// <returns>Positions in row-major order.</returns>
        public List<GridPos> FindAll(char symbol)
        {
            List<GridPos> result = new List<GridPos>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (_cells[row, col] == symbol)
                    {
                        result.Add(new GridPos(row, col));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Tests whether a cell is an obstacle.
        /// </summary>
        /// <param name="position">Cell to test.</param>
        /// <returns>True for obstacle cells.</returns>
        public bool IsObstacle(GridPos position)
        {
            return _cells[position.Row, position.Col] == ObstacleChar;
        }
    }
}
