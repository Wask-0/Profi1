using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Labyrinthino.Audio;
using Labyrinthino.Data;
using Labyrinthino.Logic;
using Labyrinthino.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.MiniGames
{
    /// <summary>
    /// Find the Path mini-game: memorise the shown path, then walk it cell by cell.
    /// </summary>
    public sealed class FindPathController : MiniGameBase
    {
        private static readonly Color PreviewColor = new Color32(255, 214, 110, 255);

        [SerializeField] private BoardView _board;
        [SerializeField] private Text _statusText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Sprite _startIcon;
        [SerializeField] private Sprite _finishIcon;
        [SerializeField] private Sprite _obstacleIcon;
        [SerializeField] private Sprite _markerIcon;

        private GridMap _map;
        private FindPathModel _model;
        private BoardCell[,] _cells;

        /// <inheritdoc />
        public override MiniGameId Id => MiniGameId.FindPath;

        private void Awake()
        {
            _resetButton.onClick.AddListener(OnResetClicked);
        }

        /// <inheritdoc />
        protected override void LoadLevel(bool restoreSnapshot)
        {
            _map = new GridMap(MiniGameCatalog.FindPathMaps[LevelIndex]);
            _model = new FindPathModel(_map);
            _cells = _board.Build(_map.Rows, _map.Cols, OnCellClicked);
            Header.SetTimerVisible(false);
            Timer.Stop();
            Repaint();
            StartCoroutine(PreviewRoutine());
        }

        /// <inheritdoc />
        protected override void WriteSnapshot(MiniGameProgress progress)
        {
            progress.Board = string.Empty;
        }

        /// <inheritdoc />
        protected override void OnTimeExpired()
        {
        }

        private IEnumerator PreviewRoutine()
        {
            InputLocked = true;
            _resetButton.interactable = false;
            List<GridPos> path = _model.FindShortestPath();
            foreach (GridPos position in path)
            {
                if (!position.Equals(_model.Start) && !position.Equals(_model.Finish))
                {
                    _cells[position.Row, position.Col].SetColor(PreviewColor);
                }
            }

            for (float left = MiniGameCatalog.PathPreviewSeconds; left > 0f; left -= Time.unscaledDeltaTime)
            {
                _statusText.text = "Запомни путь! " + Mathf.CeilToInt(left);
                _statusText.color = Palette.Gold;
                yield return null;
            }

            _statusText.text = "Пройди путь от старта до финиша";
            _statusText.color = Palette.Cream;
            _resetButton.interactable = true;
            InputLocked = false;
            Repaint();
        }

        private void OnCellClicked(int row, int col)
        {
            if (InputLocked)
            {
                return;
            }

            switch (_model.TryMove(row, col))
            {
                case MoveResult.Blocked:
                    PlaySound(SoundId.Error);
                    StartCoroutine(BlockedRoutine(_cells[row, col]));
                    break;
                case MoveResult.Moved:
                case MoveResult.Backtracked:
                    PlaySound(SoundId.Step);
                    Repaint();
                    StartCoroutine(UiTween.Scale(_cells[row, col].transform, Vector3.one * 0.7f, Vector3.one, 0.25f));
                    break;
                case MoveResult.Finished:
                    StartCoroutine(SuccessRoutine());
                    break;
            }
        }

        private void OnResetClicked()
        {
            if (InputLocked)
            {
                return;
            }

            _model.Reset();
            Repaint();
        }

        private IEnumerator BlockedRoutine(BoardCell cell)
        {
            InputLocked = true;
            cell.SetColor(Palette.Coral);
            yield return UiTween.Shake(cell.Rect, 10f, 0.3f);
            InputLocked = false;
            Repaint();
        }

        private IEnumerator SuccessRoutine()
        {
            InputLocked = true;
            LevelActive = false;
            PlaySound(SoundId.Correct);
            Repaint();
            _statusText.text = "Путь найден!";
            _statusText.color = Palette.Teal;
            Confetti.Celebrate(60);
            foreach (GridPos position in _model.Route)
            {
                StartCoroutine(UiTween.Scale(_cells[position.Row, position.Col].transform, Vector3.one * 0.6f, Vector3.one, 0.4f));
                yield return new WaitForSecondsRealtime(0.08f);
            }

            yield return new WaitForSecondsRealtime(1.2f);
            AdvanceAfterLevel();
        }

        private void Repaint()
        {
            for (int row = 0; row < _map.Rows; row++)
            {
                for (int col = 0; col < _map.Cols; col++)
                {
                    PaintCell(_cells[row, col], _map.At(row, col), new GridPos(row, col));
                }
            }
        }

        private void PaintCell(BoardCell cell, char symbol, GridPos position)
        {
            bool visited = _model.Route.Contains(position);
            bool isCurrent = position.Equals(_model.Current);
            cell.SetColor(Palette.Cell);
            cell.SetIcon(null, Color.white);
            if (symbol == GridMap.ObstacleChar)
            {
                cell.SetColor(Palette.Wall);
                cell.SetIcon(_obstacleIcon, new Color(1f, 1f, 1f, 0.35f));
            }
            else if (symbol == 'F')
            {
                cell.SetColor(Palette.Gold);
                cell.SetIcon(_finishIcon, Palette.DarkText);
            }
            else if (visited)
            {
                cell.SetColor(Palette.Teal);
            }

            if (symbol == 'S')
            {
                cell.SetColor(Palette.Teal);
                cell.SetIcon(_startIcon, Palette.DarkText);
            }

            if (isCurrent && symbol != 'F')
            {
                cell.SetIcon(_markerIcon, Color.white);
            }
        }
    }
}
