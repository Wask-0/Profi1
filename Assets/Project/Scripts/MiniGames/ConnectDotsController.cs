using System.Collections;
using Labyrinthino.Audio;
using Labyrinthino.Data;
using Labyrinthino.Logic;
using Labyrinthino.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Labyrinthino.MiniGames
{
    /// <summary>
    /// Connect the Dots mini-game: draw a route from a start point through all mandatory
    /// points to the destination.
    /// </summary>
    public sealed class ConnectDotsController : MiniGameBase
    {
        private const float LineThickness = 34f;

        [SerializeField] private BoardView _board;
        [SerializeField] private RectTransform _lineLayer;
        [SerializeField] private Sprite _squareSprite;
        [SerializeField] private Sprite _circleSprite;
        [SerializeField] private Text _hintText;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Sprite _startIcon;
        [SerializeField] private Sprite _mandatoryIcon;
        [SerializeField] private Sprite _destinationIcon;

        private GridMap _map;
        private ConnectDotsModel _model;
        private BoardCell[,] _cells;
        private Color _lineColor = Palette.Gold;

        /// <inheritdoc />
        public override MiniGameId Id => MiniGameId.ConnectDots;

        private void Awake()
        {
            _resetButton.onClick.AddListener(OnResetClicked);
        }

        /// <inheritdoc />
        protected override void LoadLevel(bool restoreSnapshot)
        {
            _map = new GridMap(MiniGameCatalog.ConnectDotsMaps[LevelIndex]);
            _model = new ConnectDotsModel(_map);
            _cells = _board.Build(_map.Rows, _map.Cols, OnCellClicked);
            _lineLayer.sizeDelta = _board.CellContainer.sizeDelta;
            _lineColor = Palette.Gold;
            ShowHint("Начни с подсвеченной точки", Palette.Cream);
            Repaint();
            float seconds = restoreSnapshot ? Progress.TimeLeft : MiniGameCatalog.ConnectDotsSeconds[LevelIndex];
            Header.SetTimerVisible(true);
            Header.SetTimer(seconds);
            Timer.Start(seconds);
        }

        /// <inheritdoc />
        protected override void WriteSnapshot(MiniGameProgress progress)
        {
            progress.Board = string.Empty;
        }

        /// <inheritdoc />
        protected override void OnTimeExpired()
        {
            if (InputLocked)
            {
                return;
            }

            InputLocked = true;
            LevelActive = false;
            Progress.HasSnapshot = false;
            RunAfter(0.4f, () => Root.ShowDefeat(Id, "Время вышло, а маршрут так и не построен"));
        }

        private void OnCellClicked(int row, int col)
        {
            if (InputLocked)
            {
                return;
            }

            bool wasStarted = _model.IsStarted;
            switch (_model.Tap(row, col))
            {
                case RouteResult.Ignored:
                    if (!wasStarted)
                    {
                        PlaySound(SoundId.Error);
                        ShowHint("Маршрут начинается только со стартовой точки", Palette.Gold);
                        StartCoroutine(UiTween.Shake(_cells[row, col].Rect, 8f, 0.25f));
                    }

                    break;
                case RouteResult.Started:
                    PlaySound(SoundId.Tap);
                    ShowHint("Веди маршрут по соседним клеткам", Palette.Cream);
                    Repaint();
                    break;
                case RouteResult.Extended:
                case RouteResult.Undone:
                    PlaySound(SoundId.Step);
                    Repaint();
                    break;
                case RouteResult.Blocked:
                    PlaySound(SoundId.Error);
                    StartCoroutine(UiTween.Shake(_cells[row, col].Rect, 8f, 0.25f));
                    break;
                case RouteResult.Completed:
                    StartCoroutine(CompletedRoutine());
                    break;
                case RouteResult.MissingPoints:
                    StartCoroutine(MissingPointsRoutine());
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
            ShowHint("Начни с подсвеченной точки", Palette.Cream);
            Repaint();
        }

        private IEnumerator CompletedRoutine()
        {
            InputLocked = true;
            LevelActive = false;
            Timer.Stop();
            _lineColor = Palette.Teal;
            Repaint();
            PlaySound(SoundId.Correct);
            ShowHint("Маршрут защёлкнулся!", Palette.Teal);
            Confetti.Celebrate(60);
            foreach (GridPos position in _model.Route)
            {
                StartCoroutine(UiTween.Scale(_cells[position.Row, position.Col].transform, Vector3.one * 0.6f, Vector3.one, 0.4f));
                yield return new WaitForSecondsRealtime(0.07f);
            }

            yield return new WaitForSecondsRealtime(1.2f);
            AdvanceAfterLevel();
        }

        private IEnumerator MissingPointsRoutine()
        {
            InputLocked = true;
            PlaySound(SoundId.Error);
            _lineColor = Palette.Coral;
            Repaint();
            ShowHint("Не все обязательные точки посещены!", Palette.Coral);
            yield return new WaitForSecondsRealtime(1f);
            _model.Reset();
            _lineColor = Palette.Gold;
            ShowHint("Начни с подсвеченной точки", Palette.Cream);
            Repaint();
            InputLocked = false;
        }

        private void ShowHint(string text, Color color)
        {
            _hintText.text = text;
            _hintText.color = color;
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

            DrawRoute();
        }

        private void PaintCell(BoardCell cell, char symbol, GridPos position)
        {
            bool onRoute = false;
            foreach (GridPos step in _model.Route)
            {
                onRoute |= step.Equals(position);
            }

            cell.SetColor(onRoute ? new Color32(190, 250, 238, 255) : Palette.Cell);
            cell.SetIcon(null, Color.white);
            FloatAnimator pulse = cell.IconRect.GetComponent<FloatAnimator>();
            if (pulse != null)
            {
                Destroy(pulse);
            }

            switch (symbol)
            {
                case GridMap.ObstacleChar:
                    cell.SetColor(Palette.Wall);
                    break;
                case 'S':
                    cell.SetColor(Palette.Teal);
                    cell.SetIcon(_startIcon, Palette.DarkText);
                    if (!_model.IsStarted)
                    {
                        cell.IconRect.gameObject.AddComponent<FloatAnimator>().Configure(0f, 4f, 0.18f, 0f);
                    }

                    break;
                case 'M':
                    cell.SetColor(Palette.Gold);
                    cell.SetIcon(_mandatoryIcon, Palette.DarkText);
                    break;
                case 'E':
                    cell.SetColor(Palette.Coral);
                    cell.SetIcon(_destinationIcon, Color.white);
                    break;
            }
        }

        private void DrawRoute()
        {
            for (int i = _lineLayer.childCount - 1; i >= 0; i--)
            {
                Destroy(_lineLayer.GetChild(i).gameObject);
            }

            for (int i = 0; i < _model.Route.Count; i++)
            {
                Vector2 center = CellCenter(_model.Route[i]);
                CreateSegment(_circleSprite, center, new Vector2(LineThickness * 1.3f, LineThickness * 1.3f));
                if (i == 0)
                {
                    continue;
                }

                Vector2 previous = CellCenter(_model.Route[i - 1]);
                Vector2 middle = (previous + center) / 2f;
                bool horizontal = !Mathf.Approximately(previous.x, center.x);
                Vector2 size = horizontal
                    ? new Vector2(Mathf.Abs(center.x - previous.x), LineThickness)
                    : new Vector2(LineThickness, Mathf.Abs(center.y - previous.y));
                CreateSegment(_squareSprite, middle, size);
            }
        }

        private Vector2 CellCenter(GridPos position)
        {
            Vector2 total = _board.CellContainer.sizeDelta;
            float x = (-total.x / 2f) + (position.Col * _board.CellStep) + (_board.CellSize / 2f);
            float y = (total.y / 2f) - (position.Row * _board.CellStep) - (_board.CellSize / 2f);
            return new Vector2(x, y);
        }

        private void CreateSegment(Sprite sprite, Vector2 position, Vector2 size)
        {
            GameObject instance = new GameObject("RouteSegment", typeof(RectTransform), typeof(Image));
            RectTransform rect = (RectTransform)instance.transform;
            rect.SetParent(_lineLayer, false);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = instance.GetComponent<Image>();
            image.sprite = sprite;
            image.color = _lineColor;
            image.raycastTarget = false;
        }
    }
}
