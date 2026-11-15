// Miguel Rodríguez Gallego
using System.Collections.Generic;

namespace AICookbook.Minimax
{
    /// <summary>
    ///     Connect Four rules on a plain board array, independent of Unity objects so the search
    ///     algorithms can run (and be tested) without a scene.
    ///     The board is board[column, row], row 0 at the top: pieces fall towards the last row.
    ///     Cells are 0 (empty), 1 or 2 (player id).
    /// </summary>
    public static class ConnectFourRules
    {
        public const int Empty = 0;
        public const int Connect = 4;

        public static int Opponent(int player) => 3 - player;
        public static int Columns(int[,] board) => board.GetLength(0);
        public static int Rows(int[,] board) => board.GetLength(1);

        public static bool IsValidMove(int[,] board, int column)
            => column >= 0 && column < Columns(board) && board[column, 0] == Empty;

        /// <summary>
        ///     Drops a piece and returns the row where it landed, or -1 if the column is full
        /// </summary>
        public static int Drop(int[,] board, int column, int player)
        {
            for (int row = Rows(board) - 1; row >= 0; row--)
            {
                if (board[column, row] == Empty)
                {
                    board[column, row] = player;
                    return row;
                }
            }
            return -1;
        }

        public static void Undo(int[,] board, int column, int row) => board[column, row] = Empty;

        /// <summary> Playable columns from left to right </summary>
        public static List<int> ValidMoves(int[,] board)
        {
            var moves = new List<int>(Columns(board));
            for (int column = 0; column < Columns(board); column++)
                if (IsValidMove(board, column))
                    moves.Add(column);
            return moves;
        }

        /// <summary>
        ///     Playable columns from the centre outwards. Central moves are usually the best ones,
        ///     so searching them first lets alpha-beta prune much more.
        /// </summary>
        public static List<int> OrderedMoves(int[,] board)
        {
            int columns = Columns(board);
            int center = columns / 2;
            var moves = new List<int>(columns);
            for (int offset = 0; offset <= center; offset++)
            {
                int left = center - offset, right = center + offset;
                if (IsValidMove(board, left)) moves.Add(left);
                if (offset != 0 && right < columns && IsValidMove(board, right)) moves.Add(right);
            }
            return moves;
        }

        public static bool IsFull(int[,] board)
        {
            for (int column = 0; column < Columns(board); column++)
                if (board[column, 0] == Empty)
                    return false;
            return true;
        }

        public static bool IsEmpty(int[,] board)
        {
            int bottom = Rows(board) - 1;
            for (int column = 0; column < Columns(board); column++)
                if (board[column, bottom] != Empty)
                    return false;
            return true;
        }

        /// <summary>
        ///     True if <paramref name="player"/> has four in line: vertical, horizontal or diagonal
        /// </summary>
        public static bool IsWin(int[,] board, int player)
        {
            int columns = Columns(board), rows = Rows(board);
            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    if (board[column, row] != player) continue;

                    if (Line(board, player, column, row, 0, 1)) return true;   // vertical
                    if (Line(board, player, column, row, 1, 0)) return true;   // horizontal
                    if (Line(board, player, column, row, 1, 1)) return true;   // diagonal going down
                    if (Line(board, player, column, row, 1, -1)) return true;  // diagonal going up
                }
            }
            return false;
        }

        static bool Line(int[,] board, int player, int column, int row, int stepColumn, int stepRow)
        {
            int lastColumn = column + stepColumn * (Connect - 1);
            int lastRow = row + stepRow * (Connect - 1);
            if (lastColumn < 0 || lastColumn >= Columns(board) || lastRow < 0 || lastRow >= Rows(board))
                return false;

            for (int i = 1; i < Connect; i++)
                if (board[column + stepColumn * i, row + stepRow * i] != player)
                    return false;
            return true;
        }
    }
}
