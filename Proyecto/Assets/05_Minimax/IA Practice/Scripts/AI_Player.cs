// Miguel Rodríguez Gallego
using System.Diagnostics;
using UnityEngine;
using AICookbook.Shared;
using Debug = UnityEngine.Debug;

namespace AICookbook.Minimax
{
    /// <summary>
    ///     Base of the AI players: takes a snapshot of the board, runs the search of the subclass,
    ///     plays the chosen column and records its cost (time and visited positions).
    ///     With the AI debug view (G) it shows the last decision above the board.
    /// </summary>
    public abstract class AI_Player : Player, IAIDebugLabel
    {
        [Tooltip("How many moves ahead the AI looks (its own and the opponent's)")]
        [Min(1)] public int maxDepth = 4;

        [Tooltip("The first move of the game is random, so AI vs AI games are not always the same")]
        [SerializeField] bool randomFirstMove = true;

        public SearchResult LastSearch { get; private set; }
        public long LastMilliseconds { get; private set; }

        protected abstract string AlgorithmName { get; }

        /// <summary> Decides the column to play on a copy of the board </summary>
        protected abstract SearchResult Search(int[,] board);

        public override bool MakeMove(GameBoard board)
        {
            int[,] snapshot = (int[,])board.board.Clone();

            var timer = Stopwatch.StartNew();
            SearchResult result = randomFirstMove && ConnectFourRules.IsEmpty(snapshot)
                ? RandomMove(snapshot)
                : Search(snapshot);
            timer.Stop();

            if (!board.IsValidMove(result.Column))
            {
                Debug.LogError($"{AlgorithmName} chose an invalid column: {result.Column}");
                return false;
            }

            board.MakeMove(result.Column, ID);
            LastSearch = result;
            LastMilliseconds = timer.ElapsedMilliseconds;
            RecordStats(timer.ElapsedTicks, result.Nodes);
            return true;
        }

        protected static SearchResult RandomMove(int[,] board)
        {
            var moves = ConnectFourRules.ValidMoves(board);
            return new SearchResult { Column = moves.Count > 0 ? moves[Random.Range(0, moves.Count)] : -1 };
        }

        /// <summary> Data for the results table of the DocumentationManager </summary>
        void RecordStats(long ticks, long nodes)
        {
            if (ID == 1)
            {
                DocumentationManager.Player1Documentation.timesList?.Add(ticks);
                DocumentationManager.Player1Documentation.depth += (int)nodes;
            }
            else if (ID == 2)
            {
                DocumentationManager.Player2Documentation.timesList?.Add(ticks);
                DocumentationManager.Player2Documentation.depth += (int)nodes;
            }
        }

        // ---------------------------------------------------------------- AI debug view

        protected virtual void OnEnable() => AIDebugView.Register(this);
        protected virtual void OnDisable() => AIDebugView.Unregister(this);

        // Above the board (columns 0..6 on X, rows from 0 down to -5 on Y)
        public Vector3 LabelPosition => new Vector3(3f, ID == 1 ? 2.3f : 1.4f, 0f);

        public string DebugLabel
        {
            get
            {
                if (LastSearch.Nodes == 0) return null;
                string research = LastSearch.ReSearched ? " (re-searched)" : "";
                return $"P{ID} {AlgorithmName} · depth {maxDepth}: column {LastSearch.Column + 1}, " +
                       $"score {LastSearch.Score}, {LastSearch.Nodes:N0} positions{research}, {LastMilliseconds} ms";
            }
        }
    }
}
