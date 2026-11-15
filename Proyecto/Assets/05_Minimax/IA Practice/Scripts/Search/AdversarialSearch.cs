// Miguel Rodríguez Gallego
namespace AICookbook.Minimax
{
    /// <summary>
    ///     Result of a search: column to play, its score for the player who searched and how many
    ///     positions were visited (to compare the cost of each algorithm).
    /// </summary>
    public struct SearchResult
    {
        public int Column;
        public int Score;
        public long Nodes;
        /// <summary> Aspiration search only: the narrow window failed and a full search was needed </summary>
        public bool ReSearched;
    }

    /// <summary>
    ///     Three ways of searching the game tree. All of them evaluate the same tree with the same
    ///     heuristic, so they agree on the score of a position; they differ in how much of the tree they visit.
    ///     <list type="bullet">
    ///         <item><see cref="Minimax"/>: explores everything, one player maximises and the other minimises</item>
    ///         <item><see cref="Negamax"/>: same idea written for both players at once (score of one = -score of the other),
    ///               plus alpha-beta pruning to skip branches that cannot change the decision</item>
    ///         <item><see cref="Aspiration"/>: Negamax with a narrow window around an expected score; if the real
    ///               score falls outside, it searches again with the full window</item>
    ///     </list>
    ///     The search works on a copy of the board array and undoes every move, so the real board is never touched.
    /// </summary>
    public static class AdversarialSearch
    {
        const int Infinity = BoardEvaluator.WinScore * 2;

        // ---------------------------------------------------------------- Minimax

        public static SearchResult Minimax(int[,] board, int depth, int player)
        {
            var result = new SearchResult { Column = -1, Score = -Infinity };
            int opponent = ConnectFourRules.Opponent(player);

            foreach (int column in ConnectFourRules.OrderedMoves(board))
            {
                int row = ConnectFourRules.Drop(board, column, player);
                int score = MinimaxValue(board, depth - 1, 1, player, opponent, ref result.Nodes);
                ConnectFourRules.Undo(board, column, row);

                if (score > result.Score)
                {
                    result.Score = score;
                    result.Column = column;
                }
            }
            result.Nodes++; // root
            return result;
        }

        /// <summary>
        ///     Value of the position for <paramref name="me"/>, with <paramref name="toMove"/> about to play
        /// </summary>
        static int MinimaxValue(int[,] board, int depth, int ply, int me, int toMove, ref long nodes)
        {
            nodes++;
            int justMoved = ConnectFourRules.Opponent(toMove);

            if (ConnectFourRules.IsWin(board, justMoved))
                return justMoved == me ? BoardEvaluator.WinScore - ply : -(BoardEvaluator.WinScore - ply);
            if (ConnectFourRules.IsFull(board))
                return 0;
            if (depth <= 0)
                return BoardEvaluator.Evaluate(board, me);

            bool maximizing = toMove == me;
            int best = maximizing ? -Infinity : Infinity;

            foreach (int column in ConnectFourRules.OrderedMoves(board))
            {
                int row = ConnectFourRules.Drop(board, column, toMove);
                int value = MinimaxValue(board, depth - 1, ply + 1, me, ConnectFourRules.Opponent(toMove), ref nodes);
                ConnectFourRules.Undo(board, column, row);

                best = maximizing ? System.Math.Max(best, value) : System.Math.Min(best, value);
            }
            return best;
        }

        // ---------------------------------------------------------------- Negamax with alpha-beta

        public static SearchResult Negamax(int[,] board, int depth, int player)
            => Negamax(board, depth, player, -Infinity, Infinity);

        /// <summary>
        ///     Negamax limited to the (alpha, beta) window. If the result is outside the window its score
        ///     is only a bound, not the exact value (that is what <see cref="Aspiration"/> checks).
        /// </summary>
        public static SearchResult Negamax(int[,] board, int depth, int player, int alpha, int beta)
        {
            var result = new SearchResult { Column = -1, Score = -Infinity };
            int opponent = ConnectFourRules.Opponent(player);

            foreach (int column in ConnectFourRules.OrderedMoves(board))
            {
                int row = ConnectFourRules.Drop(board, column, player);
                int score = -NegamaxValue(board, depth - 1, 1, -beta, -alpha, opponent, ref result.Nodes);
                ConnectFourRules.Undo(board, column, row);

                if (score > result.Score)
                {
                    result.Score = score;
                    result.Column = column;
                }
                if (score > alpha) alpha = score;
                if (alpha >= beta) break; // the opponent will never allow this position
            }
            result.Nodes++;
            return result;
        }

        /// <summary>
        ///     Value of the position for <paramref name="toMove"/> (the player about to play)
        /// </summary>
        static int NegamaxValue(int[,] board, int depth, int ply, int alpha, int beta, int toMove, ref long nodes)
        {
            nodes++;
            int justMoved = ConnectFourRules.Opponent(toMove);

            if (ConnectFourRules.IsWin(board, justMoved))
                return -(BoardEvaluator.WinScore - ply);
            if (ConnectFourRules.IsFull(board))
                return 0;
            if (depth <= 0)
                return BoardEvaluator.Evaluate(board, toMove);

            int best = -Infinity;
            foreach (int column in ConnectFourRules.OrderedMoves(board))
            {
                int row = ConnectFourRules.Drop(board, column, toMove);
                int value = -NegamaxValue(board, depth - 1, ply + 1, -beta, -alpha, justMoved, ref nodes);
                ConnectFourRules.Undo(board, column, row);

                if (value > best) best = value;
                if (best > alpha) alpha = best;
                if (alpha >= beta) break; // prune: this branch is already worse than an alternative
            }
            return best;
        }

        // ---------------------------------------------------------------- Aspiration search

        /// <summary>
        ///     Negamax with a window of ±<paramref name="window"/> around <paramref name="expectedScore"/>
        ///     (usually the score of the previous turn). A narrow window prunes more; if the real score is
        ///     outside it, the result is not reliable and the position is searched again with the full window.
        /// </summary>
        public static SearchResult Aspiration(int[,] board, int depth, int player, int expectedScore, int window)
        {
            int alpha = expectedScore - window;
            int beta = expectedScore + window;

            SearchResult result = Negamax(board, depth, player, alpha, beta);
            if (result.Score > alpha && result.Score < beta)
                return result;

            SearchResult full = Negamax(board, depth, player);
            full.Nodes += result.Nodes;
            full.ReSearched = true;
            return full;
        }
    }
}
