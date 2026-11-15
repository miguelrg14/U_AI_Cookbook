// Miguel Rodríguez Gallego
namespace AICookbook.Minimax
{
    /// <summary>
    ///     Baseline opponent: plays any legal column at random
    /// </summary>
    public class RandomPlayer : AI_Player
    {
        protected override string AlgorithmName => "Random";

        protected override SearchResult Search(int[,] board) => RandomMove(board);
    }
}
