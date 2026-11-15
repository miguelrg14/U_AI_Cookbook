// Miguel Rodríguez Gallego
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AICookbook.Minimax.Tests
{
    /// <summary>
    ///     Behaviour of the search algorithms on plain boards (no scene needed):
    ///     they take wins, block losses, agree with each other and alpha-beta really prunes.
    /// </summary>
    public class SearchTests
    {
        const int P1 = 1, P2 = 2;
        const int Columns = 7, Rows = 6;

        delegate SearchResult Algorithm(int[,] board, int depth, int player);

        static readonly Dictionary<string, Algorithm> Algorithms = new Dictionary<string, Algorithm>
        {
            ["Minimax"] = AdversarialSearch.Minimax,
            ["Negamax"] = AdversarialSearch.Negamax,
            ["Aspiration"] = (board, depth, player) =>
                AdversarialSearch.Aspiration(board, depth, player, BoardEvaluator.Evaluate(board, player), 25),
        };

        static IEnumerable<string> AlgorithmNames => Algorithms.Keys;

        static int[,] EmptyBoard() => new int[Columns, Rows];

        static int[,] Board(params (int column, int player)[] moves)
        {
            var board = EmptyBoard();
            foreach (var (column, player) in moves)
                ConnectFourRules.Drop(board, column, player);
            return board;
        }

        /// <summary> Random legal game of <paramref name="moves"/> moves, or null if someone won on the way </summary>
        static int[,] RandomPosition(int seed, int moves, out int toMove)
        {
            var random = new Random(seed);
            var board = EmptyBoard();
            toMove = P1;
            for (int i = 0; i < moves; i++)
            {
                var valid = ConnectFourRules.ValidMoves(board);
                ConnectFourRules.Drop(board, valid[random.Next(valid.Count)], toMove);
                if (ConnectFourRules.IsWin(board, toMove)) return null;
                toMove = ConnectFourRules.Opponent(toMove);
            }
            return board;
        }

        // ---------------------------------------------------------------- tactics

        [Test]
        public void TakesAnImmediateWin([ValueSource(nameof(AlgorithmNames))] string algorithm, [Values(1, 2, 3, 4)] int depth)
        {
            // P1 has three in the bottom row (columns 1-3) and plays: column 0 or 4 wins
            var board = Board((1, P1), (1, P2), (2, P1), (2, P2), (3, P1), (6, P2));

            SearchResult result = Algorithms[algorithm](board, depth, P1);

            Assert.That(result.Column, Is.EqualTo(0).Or.EqualTo(4));
            Assert.AreEqual(BoardEvaluator.WinScore - 1, result.Score, "a win next move is the best possible score");
        }

        [Test]
        public void BlocksTheOpponentsWin([ValueSource(nameof(AlgorithmNames))] string algorithm, [Values(2, 3, 4)] int depth)
        {
            // P2 has three stacked in column 0: P1 must play column 0
            var board = Board((3, P1), (0, P2), (6, P1), (0, P2), (0, P2));

            SearchResult result = Algorithms[algorithm](board, depth, P1);

            Assert.AreEqual(0, result.Column);
        }

        [Test]
        public void PrefersTheFastestWin()
        {
            // Win now in column 4 (bottom row 1-3); other moves could also win later
            var board = Board((1, P1), (1, P2), (2, P1), (2, P2), (3, P1), (6, P2));

            SearchResult shallow = AdversarialSearch.Negamax(board, 1, P1);
            SearchResult deep = AdversarialSearch.Negamax(board, 5, P1);

            Assert.AreEqual(shallow.Score, deep.Score, "looking deeper must not delay a win available now");
        }

        // ---------------------------------------------------------------- the algorithms agree

        [Test]
        public void MinimaxAndNegamax_GiveTheSameDecision([Range(0, 19)] int seed, [Values(1, 2, 3, 4)] int depth)
        {
            var board = RandomPosition(seed, 4 + seed % 10, out int toMove);
            if (board == null) Assert.Ignore("Random game already finished");

            SearchResult minimax = AdversarialSearch.Minimax(board, depth, toMove);
            SearchResult negamax = AdversarialSearch.Negamax(board, depth, toMove);

            Assert.AreEqual(minimax.Score, negamax.Score, "score");
            Assert.AreEqual(minimax.Column, negamax.Column, "column");
        }

        [Test]
        public void Aspiration_GivesTheSameDecisionAsNegamax_WhateverTheWindow([Range(0, 9)] int seed, [Values(1, 5, 25, 1000)] int window)
        {
            var board = RandomPosition(seed, 6 + seed, out int toMove);
            if (board == null) Assert.Ignore("Random game already finished");
            const int depth = 4;

            SearchResult negamax = AdversarialSearch.Negamax(board, depth, toMove);
            SearchResult aspiration = AdversarialSearch.Aspiration(board, depth, toMove, BoardEvaluator.Evaluate(board, toMove), window);

            Assert.AreEqual(negamax.Score, aspiration.Score, "score");
            Assert.AreEqual(negamax.Column, aspiration.Column, "column");
        }

        // ---------------------------------------------------------------- cost

        [Test]
        public void AlphaBeta_VisitsFarFewerPositionsThanMinimax()
        {
            var board = Board((3, P1), (3, P2));

            SearchResult minimax = AdversarialSearch.Minimax(board, 5, P1);
            SearchResult negamax = AdversarialSearch.Negamax(board, 5, P1);

            Assert.Less(negamax.Nodes * 5, minimax.Nodes, $"Negamax {negamax.Nodes} vs Minimax {minimax.Nodes} positions");
        }

        [Test]
        public void Search_DoesNotModifyTheBoard([ValueSource(nameof(AlgorithmNames))] string algorithm)
        {
            var board = Board((3, P1), (2, P2), (3, P1));
            var copy = (int[,])board.Clone();

            Algorithms[algorithm](board, 4, P2);

            CollectionAssert.AreEqual(copy, board);
        }

        // ---------------------------------------------------------------- evaluation and rules

        [Test]
        public void Evaluation_IsSymmetricBetweenPlayers([Range(0, 9)] int seed)
        {
            var board = RandomPosition(seed, 10, out _);
            if (board == null) Assert.Ignore("Random game already finished");

            Assert.AreEqual(BoardEvaluator.Evaluate(board, P1), -BoardEvaluator.Evaluate(board, P2));
        }

        [Test]
        public void OrderedMoves_StartFromTheCentre()
        {
            CollectionAssert.AreEqual(new[] { 3, 2, 4, 1, 5, 0, 6 }, ConnectFourRules.OrderedMoves(EmptyBoard()));
        }
    }
}
