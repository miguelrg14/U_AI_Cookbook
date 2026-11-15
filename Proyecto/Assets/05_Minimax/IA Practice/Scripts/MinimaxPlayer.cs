// Miguel Rodríguez Gallego
namespace AICookbook.Minimax
{
    /// <summary>
    ///     Plain Minimax: explores every move up to <see cref="AI_Player.maxDepth"/>, without pruning.
    ///     It is the reference the other two algorithms are compared with.
    /// </summary>
    public class MinimaxPlayer : AI_Player
    {
        protected override string AlgorithmName => "Minimax";

        protected override SearchResult Search(int[,] board) => AdversarialSearch.Minimax(board, maxDepth, ID);
    }
}
