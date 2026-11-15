// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.Minimax
{
    /// <summary>
    ///     Aspiration search: Negamax with a narrow window around the score expected for this turn
    ///     (the one found on the previous turn). Usually prunes more than a full-window search;
    ///     when the guess is wrong it has to search again, which the debug view shows as "re-searched".
    /// </summary>
    public class AspirationPlayer : AI_Player
    {
        [Tooltip("Half width of the window around the expected score")]
        [Min(1)] [SerializeField] int windowRange = 25;

        bool hasPreviousScore;
        int previousScore;

        protected override string AlgorithmName => "Aspiration";

        protected override SearchResult Search(int[,] board)
        {
            int expected = hasPreviousScore ? previousScore : BoardEvaluator.Evaluate(board, ID);
            SearchResult result = AdversarialSearch.Aspiration(board, maxDepth, ID, expected, windowRange);

            previousScore = result.Score;
            hasPreviousScore = true;
            return result;
        }
    }
}
