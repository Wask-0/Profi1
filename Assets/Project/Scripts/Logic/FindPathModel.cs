using System.Collections.Generic;

namespace Labyrinthino.Logic
{
    /// <summary>
    /// Outcome of an attempt to move in the Find the Path game.
    /// </summary>
    public enum MoveResult
    {
        /// <summary>Nothing happened (outside the grid, not adjacent, already visited).</summary>
        Ignored,

        /// <summary>The target cell is an obstacle.</summary>
        Blocked,

        /// <summary>The player advanced to a new cell.</summary>
        Moved,

        /// <summary>The player stepped back to the previous cell.</summary>
        Backtracked,

        /// <summary>The player reached the finish cell.</summary>
        Finished
    }

    /// <summary>
    /// Rules of the Find the Path mini-game: the player walks from start to finish
    /// through adjacent cells and cannot enter obstacles.
    /// </summary>
    public sealed class FindPathModel
    {
        private readonly GridMap _map;
        private readonly List<GridPos> _route = new List<GridPos>();

        /// <summary>
        /// Creates a model for a level.
        /// </summary>
        /// <param name="map">Level map containing exactly one S and one F cell.</param>
        public FindPathModel(GridMap map)
        {
            _map = map;
            Start = map.FindAll('S')[0];
            Finish = map.FindAll('F')[0];
            Reset();
        }

        /// <summary>Start cell of the level.</summary>
        public GridPos Start { get; }

        /// <summary>Finish cell of the level.</summary>
        public GridPos Finish { get; }

        /// <summary>Cell the player currently stands on.</summary>
        public GridPos Current => _route[_route.Count - 1];

        /// <summary>Cells visited so far, beginning with the start cell.</summary>
        public IReadOnlyList<GridPos> Route => _route;

        /// <summary>True once the finish was reached.</summary>
        public bool IsFinished { get; private set; }

        /// <summary>
        /// Returns the player to the start cell.
        /// </summary>
        public void Reset()
        {
            _route.Clear();
            _route.Add(Start);
            IsFinished = false;
        }

        /// <summary>
        /// Tries to move the player to a cell.
        /// </summary>
        /// <param name="row">Row of the target cell.</param>
        /// <param name="col">Column of the target cell.</param>
        /// <returns>What happened because of the move.</returns>
        public MoveResult TryMove(int row, int col)
        {
            if (IsFinished || !_map.InBounds(row, col))
            {
                return MoveResult.Ignored;
            }

            GridPos target = new GridPos(row, col);
            if (!target.IsAdjacentTo(Current))
            {
                return MoveResult.Ignored;
            }

            if (_map.IsObstacle(target))
            {
                return MoveResult.Blocked;
            }

            if (_route.Count > 1 && target.Equals(_route[_route.Count - 2]))
            {
                _route.RemoveAt(_route.Count - 1);
                return MoveResult.Backtracked;
            }

            if (_route.Contains(target))
            {
                return MoveResult.Ignored;
            }

            _route.Add(target);
            if (target.Equals(Finish))
            {
                IsFinished = true;
                return MoveResult.Finished;
            }

            return MoveResult.Moved;
        }

        /// <summary>
        /// Computes the shortest path from start to finish with breadth-first search.
        /// </summary>
        /// <returns>Cells from start to finish inclusive, or an empty list when unreachable.</returns>
        public List<GridPos> FindShortestPath()
        {
            Dictionary<GridPos, GridPos> parents = new Dictionary<GridPos, GridPos>();
            Queue<GridPos> queue = new Queue<GridPos>();
            queue.Enqueue(Start);
            parents[Start] = Start;
            int[] rowSteps = { 1, -1, 0, 0 };
            int[] colSteps = { 0, 0, 1, -1 };
            while (queue.Count > 0)
            {
                GridPos node = queue.Dequeue();
                if (node.Equals(Finish))
                {
                    return BuildPath(parents);
                }

                for (int i = 0; i < 4; i++)
                {
                    int row = node.Row + rowSteps[i];
                    int col = node.Col + colSteps[i];
                    GridPos next = new GridPos(row, col);
                    if (_map.InBounds(row, col) && !_map.IsObstacle(next) && !parents.ContainsKey(next))
                    {
                        parents[next] = node;
                        queue.Enqueue(next);
                    }
                }
            }

            return new List<GridPos>();
        }

        private List<GridPos> BuildPath(Dictionary<GridPos, GridPos> parents)
        {
            List<GridPos> path = new List<GridPos>();
            GridPos node = Finish;
            while (!node.Equals(Start))
            {
                path.Add(node);
                node = parents[node];
            }

            path.Add(Start);
            path.Reverse();
            return path;
        }
    }
}
