using System.Collections.Generic;

namespace Labyrinthino.Logic
{
    /// <summary>
    /// Outcome of a tap in the Connect the Dots game.
    /// </summary>
    public enum RouteResult
    {
        /// <summary>Tap had no effect.</summary>
        Ignored,

        /// <summary>The route was started on a start point.</summary>
        Started,

        /// <summary>The route grew by one cell.</summary>
        Extended,

        /// <summary>The last cell was removed from the route.</summary>
        Undone,

        /// <summary>The move is forbidden (obstacle or cell already used).</summary>
        Blocked,

        /// <summary>The route reached the destination and all mandatory points were visited.</summary>
        Completed,

        /// <summary>The route reached the destination but some mandatory points were missed.</summary>
        MissingPoints
    }

    /// <summary>
    /// Rules of the Connect the Dots mini-game: build a route from any start point
    /// through every mandatory point to the destination without crossing itself.
    /// </summary>
    public sealed class ConnectDotsModel
    {
        private readonly GridMap _map;
        private readonly List<GridPos> _route = new List<GridPos>();
        private readonly List<GridPos> _mandatory;
        private readonly GridPos _destination;

        /// <summary>
        /// Creates a model for a level.
        /// </summary>
        /// <param name="map">Level map with S, E, M and # characters.</param>
        public ConnectDotsModel(GridMap map)
        {
            _map = map;
            StartPoints = map.FindAll('S');
            _mandatory = map.FindAll('M');
            _destination = map.FindAll('E')[0];
        }

        /// <summary>Cells where a route may begin.</summary>
        public IReadOnlyList<GridPos> StartPoints { get; }

        /// <summary>Mandatory cells that the route must visit.</summary>
        public IReadOnlyList<GridPos> MandatoryPoints => _mandatory;

        /// <summary>Destination cell that closes the route.</summary>
        public GridPos Destination => _destination;

        /// <summary>Cells of the current route in order.</summary>
        public IReadOnlyList<GridPos> Route => _route;

        /// <summary>True once the route has been started.</summary>
        public bool IsStarted => _route.Count > 0;

        /// <summary>
        /// Begins the route on a start point.
        /// </summary>
        /// <param name="row">Row of the tapped cell.</param>
        /// <param name="col">Column of the tapped cell.</param>
        /// <returns><see cref="RouteResult.Started"/> on success, otherwise <see cref="RouteResult.Ignored"/>.</returns>
        public RouteResult TryStartRoute(int row, int col)
        {
            if (IsStarted || !_map.InBounds(row, col) || _map.At(row, col) != 'S')
            {
                return RouteResult.Ignored;
            }

            _route.Add(new GridPos(row, col));
            return RouteResult.Started;
        }

        /// <summary>
        /// Handles a tap: starts the route when it is empty, otherwise extends it.
        /// </summary>
        /// <param name="row">Row of the tapped cell.</param>
        /// <param name="col">Column of the tapped cell.</param>
        /// <returns>What happened because of the tap.</returns>
        public RouteResult Tap(int row, int col)
        {
            return IsStarted ? TryExtend(row, col) : TryStartRoute(row, col);
        }

        /// <summary>
        /// Clears the route so the player can begin again.
        /// </summary>
        public void Reset()
        {
            _route.Clear();
        }

        private RouteResult TryExtend(int row, int col)
        {
            if (!_map.InBounds(row, col))
            {
                return RouteResult.Ignored;
            }

            GridPos target = new GridPos(row, col);
            GridPos last = _route[_route.Count - 1];
            if (!target.IsAdjacentTo(last))
            {
                return RouteResult.Ignored;
            }

            if (_map.IsObstacle(target))
            {
                return RouteResult.Blocked;
            }

            if (_route.Count > 1 && target.Equals(_route[_route.Count - 2]))
            {
                _route.RemoveAt(_route.Count - 1);
                return RouteResult.Undone;
            }

            if (_route.Contains(target))
            {
                return RouteResult.Blocked;
            }

            _route.Add(target);
            if (!target.Equals(_destination))
            {
                return RouteResult.Extended;
            }

            return AllMandatoryVisited() ? RouteResult.Completed : RouteResult.MissingPoints;
        }

        private bool AllMandatoryVisited()
        {
            foreach (GridPos point in _mandatory)
            {
                if (!_route.Contains(point))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks by exhaustive search that a level can be solved.
        /// </summary>
        /// <returns>True when at least one valid route exists.</returns>
        public bool IsSolvable()
        {
            foreach (GridPos start in StartPoints)
            {
                if (Search(new List<GridPos> { start }))
                {
                    return true;
                }
            }

            return false;
        }

        private bool Search(List<GridPos> path)
        {
            GridPos last = path[path.Count - 1];
            if (last.Equals(_destination))
            {
                return _mandatory.TrueForAll(point => path.Contains(point));
            }

            int[] rowSteps = { 1, -1, 0, 0 };
            int[] colSteps = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                GridPos next = new GridPos(last.Row + rowSteps[i], last.Col + colSteps[i]);
                if (!_map.InBounds(next.Row, next.Col) || _map.IsObstacle(next) || path.Contains(next))
                {
                    continue;
                }

                path.Add(next);
                bool found = Search(path);
                path.RemoveAt(path.Count - 1);
                if (found)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
