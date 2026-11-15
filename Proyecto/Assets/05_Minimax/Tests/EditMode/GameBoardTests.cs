// Miguel Rodríguez Gallego
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AICookbook.Minimax.Tests
{
    /// <summary>
    ///     Rules of Connect Four on <see cref="GameBoard"/>: legal moves, wins in every direction, draws
    ///     and the temporary boards the AIs use to look ahead.
    ///     The board is board[column, row] with row 0 at the top, so pieces fall towards row 5.
    /// </summary>
    public class GameBoardTests
    {
        const int Blue = 1, Red = 2;

        GameBoard board;

        [SetUp]
        public void SetUp()
        {
            // Awake (field creation, GameManager lookup) does not run in Edit Mode: only the data is used
            board = new GameObject("Board").AddComponent<GameBoard>();
            board.board = new int[board.columns, board.rows];
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(board.gameObject);

        /// <summary> Drops a piece in a column like a real move, without the animation </summary>
        void Drop(int column, int player)
        {
            for (int row = board.rows - 1; row >= 0; row--)
            {
                if (board.board[column, row] == 0)
                {
                    board.board[column, row] = player;
                    return;
                }
            }
            Assert.Fail($"Column {column} is full");
        }

        // ---------------------------------------------------------------- moves

        [Test]
        public void EmptyBoard_EveryColumnIsPlayable()
        {
            CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6 }, board.GetPossibleMoves());
            for (int column = 0; column < board.columns; column++)
                Assert.IsTrue(board.IsValidMove(column));
            Assert.IsTrue(board.FieldContainsEmptyCell());
            Assert.AreEqual(GameState.Playing, board.CheckWin(Blue));
        }

        [Test]
        public void FullColumn_IsNoLongerPlayable()
        {
            for (int i = 0; i < board.rows; i++)
                Drop(2, i % 2 == 0 ? Blue : Red);

            Assert.IsFalse(board.IsValidMove(2));
            CollectionAssert.DoesNotContain(board.GetPossibleMoves(), 2);
            Assert.AreEqual(board.columns - 1, board.GetPossibleMoves().Count);
        }

        // ---------------------------------------------------------------- wins

        [Test]
        public void FourStackedInAColumn_Wins()
        {
            for (int i = 0; i < 4; i++)
                Drop(3, Blue);

            Assert.AreEqual(GameState.Win, board.CheckWin(Blue));
            Assert.AreEqual(GameState.Playing, board.CheckWin(Red));
        }

        [Test]
        public void FourSideBySideInARow_Wins()
        {
            for (int column = 2; column < 6; column++)
                Drop(column, Red);

            Assert.AreEqual(GameState.Win, board.CheckWin(Red));
            Assert.AreEqual(GameState.Playing, board.CheckWin(Blue));
        }

        [TestCase(0, 2, 1, 1, TestName = "Diagonal going down-right wins")]
        [TestCase(0, 5, 1, -1, TestName = "Diagonal going up-right wins")]
        [TestCase(3, 2, 1, 1, TestName = "Diagonal touching the right edge wins")]
        public void FourInADiagonal_Wins(int startColumn, int startRow, int columnStep, int rowStep)
        {
            for (int i = 0; i < 4; i++)
                board.board[startColumn + i * columnStep, startRow + i * rowStep] = Blue;

            Assert.AreEqual(GameState.Win, board.CheckWin(Blue));
        }

        [Test]
        public void ThreeInARow_IsNotAWin()
        {
            for (int column = 0; column < 3; column++)
                Drop(column, Blue);

            Assert.AreEqual(GameState.Playing, board.CheckWin(Blue));
        }

        [Test]
        public void FullBoardWithoutFourInLine_IsADraw()
        {
            // Alternating pattern with runs of at most two in every direction
            for (int column = 0; column < board.columns; column++)
                for (int row = 0; row < board.rows; row++)
                    board.board[column, row] = (row / 2 + column) % 2 == 0 ? Blue : Red;

            Assert.IsFalse(board.FieldContainsEmptyCell());
            Assert.IsEmpty(board.GetPossibleMoves());
            Assert.AreEqual(GameState.Draw, board.CheckWin(Blue));
            Assert.AreEqual(GameState.Draw, board.CheckWin(Red));
        }

        // ---------------------------------------------------------------- look-ahead boards

        [Test]
        public void TempBoard_HasTheMove_AndLeavesTheRealBoardUntouched()
        {
            // GameBoard is a MonoBehaviour created with "new" for look-ahead copies; Unity warns about it
            LogAssert.ignoreFailingMessages = true;
            Drop(3, Blue);

            GameBoard temp = board.CreateTempBoardWithMove(3, Red);

            Assert.AreEqual(Red, temp.board[3, 4], "the new piece lands on top of the existing one");
            Assert.AreEqual(Blue, temp.board[3, 5]);
            Assert.AreEqual(0, board.board[3, 4], "the real board does not change");
        }
    }
}
