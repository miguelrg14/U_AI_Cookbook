// Miguel Rodríguez Gallego
namespace AICookbook.Minimax
{
    /// <summary>
    ///     Negamax with alpha-beta pruning: same decisions as Minimax, visiting far fewer positions,
    ///     so it can look deeper in the same time.
    /// </summary>
    public class NegaMaxPlayer : AI_Player
    {
        protected override string AlgorithmName => "Negamax α-β";

        protected override SearchResult Search(int[,] board) => AdversarialSearch.Negamax(board, maxDepth, ID);
    }
}
