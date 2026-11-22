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
    /// Meadow mini-game: turn every brown cell green before moves or time are over.
    /// </summary>
    public sealed class MeadowController : MiniGameBase
    {
        [SerializeField] private BoardView _board;
        [SerializeField] private Text _movesText;
        [SerializeField] private Text _hintText;
        [SerializeField] private Sprite _grassIcon;

        private MeadowModel _model;
        private BoardCell[,] _cells;

        /// <inheritdoc />
        public override MiniGameId Id => MiniGameId.Meadow;

        /// <inheritdoc />
        protected override void LoadLevel(bool restoreSnapshot)
        {
            int[] level = MiniGameCatalog.MeadowLevels[LevelIndex];
            _model = new MeadowModel(level[0], level[1], level[2], level[3], (LevelIndex * 7919) + 3);
            float seconds = level[4];
            if (restoreSnapshot)
            {
                _model.RestoreBoard(Progress.Board, Progress.Counter);
                seconds = Progress.TimeLeft;
            }

            _cells = _board.Build(_model.Rows, _model.Cols, OnCellClicked);
            for (int row = 0; row < _model.Rows; row++)
            {
                for (int col = 0; col < _model.Cols; col++)
                {
                    PaintCell(row, col);
                }
            }

            RefreshTexts();
            Header.SetTimerVisible(true);
            Header.SetTimer(seconds);
            Timer.Start(seconds);
        }

        /// <inheritdoc />
        protected override void WriteSnapshot(MiniGameProgress progress)
        {
            progress.Board = _model.SerializeBoard();
            progress.Counter = _model.MovesLeft;
        }

        /// <inheritdoc />
        protected override void OnTimeExpired()
        {
            if (!InputLocked)
            {
                Lose("Время вышло, а луг остался нескошенным");
            }
        }

        private void OnCellClicked(int row, int col)
        {
            if (InputLocked)
            {
                return;
            }

            MeadowTapResult result = _model.Tap(row, col);
            if (result == MeadowTapResult.Ignored)
            {
                return;
            }

            if (result == MeadowTapResult.Painted)
            {
                PlaySound(SoundId.Tap);
                PaintCell(row, col);
                StartCoroutine(UiTween.Scale(_cells[row, col].transform, Vector3.one * 0.6f, Vector3.one, 0.3f));
            }
            else
            {
                PlaySound(SoundId.Error);
                StartCoroutine(UiTween.Shake(_cells[row, col].Rect, 8f, 0.25f));
            }

            RefreshTexts();
            if (_model.IsComplete)
            {
                StartCoroutine(WinRoutine());
            }
            else if (_model.IsOutOfMoves)
            {
                Lose("Ходы закончились, а коричневые клетки остались");
            }
        }

        private IEnumerator WinRoutine()
        {
            InputLocked = true;
            LevelActive = false;
            Timer.Stop();
            PlaySound(SoundId.Correct);
            _hintText.text = "Весь луг зелёный!";
            _hintText.color = Palette.Teal;
            Confetti.Celebrate(60);
            yield return new WaitForSecondsRealtime(1.4f);
            AdvanceAfterLevel();
        }

        private void Lose(string reason)
        {
            InputLocked = true;
            LevelActive = false;
            Timer.Stop();
            Progress.HasSnapshot = false;
            RunAfter(0.6f, () => Root.ShowDefeat(Id, reason));
        }

        private void PaintCell(int row, int col)
        {
            bool green = _model.IsGreen(row, col);
            _cells[row, col].SetColor(green ? Palette.Grass : Palette.Soil);
            _cells[row, col].SetIcon(green ? _grassIcon : null, new Color(1f, 1f, 1f, 0.55f));
        }

        private void RefreshTexts()
        {
            _movesText.text = "Ходов: " + _model.MovesLeft + "   Осталось: " + _model.BrownCount;
            _hintText.text = "Нажимай на коричневые клетки";
            _hintText.color = Palette.Cream;
        }
    }
}
