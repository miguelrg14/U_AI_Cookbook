// Miguel Rodríguez Gallego
namespace AICookbook.Minimax
{
    /// <summary>
    ///     Heuristic score of a non-final position, used when the search reaches its depth limit.
    ///     Every group of four cells in line (a "window") that is still winnable is worth points to the
    ///     player who has pieces in it; centre pieces get a bonus because they belong to more windows.
    ///     The score is symmetric: Evaluate(board, p) == -Evaluate(board, opponent), which Negamax needs.
    /// </summary>
    public static class BoardEvaluator
    {
        /// <summary> Value of a win. The search subtracts the ply so faster wins are preferred </summary>
        public const int WinScore = 1_000_000;

        public const int ThreeInWindow = 5;
        public const int TwoInWindow = 2;
        public const int CenterPiece = 3;

        public static int Evaluate(int[,] board, int player)
        {
            int opponent = ConnectFourRules.Opponent(player);
            int columns = ConnectFourRules.Columns(board), rows = ConnectFourRules.Rows(board);
            int score = 0;

            int center = columns / 2;
            for (int row = 0; row < rows; row++)
            {
                if (board[center, row] == player) score += CenterPiece;
                else if (board[center, row] == opponent) score -= CenterPiece;
            }

            for (int column = 0; column < columns; column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    score += Window(board, player, opponent, column, row, 0, 1);
                    score += Window(board, player, opponent, column, row, 1, 0);
                    score += Window(board, player, opponent, column, row, 1, 1);
                    score += Window(board, player, opponent, column, row, 1, -1);
                }
            }
            return score;
        }

        /// <summary>
        ///     Points of the window starting at (column, row) in the given direction:
        ///     positive if only <paramref name="player"/> has pieces in it, negative if only the opponent does
        /// </summary>
        static int Window(int[,] board, int player, int opponent, int column, int row, int stepColumn, int stepRow)
        {
            int n = ConnectFourRules.Connect;
            int lastColumn = column + stepColumn * (n - 1), lastRow = row + stepRow * (n - 1);
            if (lastColumn < 0 || lastColumn >= ConnectFourRules.Columns(board) ||
                lastRow < 0 || lastRow >= ConnectFourRules.Rows(board))
                return 0;

            int mine = 0, theirs = 0;
            for (int i = 0; i < n; i++)
            {
                int cell = board[column + stepColumn * i, row + stepRow * i];
                if (cell == player) mine++;
                else if (cell == opponent) theirs++;
            }

            if (mine > 0 && theirs > 0) return 0; // blocked for both
            if (mine > 0) return Points(mine);
            if (theirs > 0) return -Points(theirs);
            return 0;
        }

        static int Points(int pieces) => pieces switch
        {
            3 => ThreeInWindow,
            2 => TwoInWindow,
            _ => 0 // four in line is a win, handled by the search as a final position
        };
    }
}
