// Miguel Rodríguez Gallego
using System;
using System.Collections.Generic;

namespace AICookbook.ReinforcementLearning
{
    public enum Cell { Free, Wall, Cliff }

    /// <summary> Position in the grid: row 0 is the top row </summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int Row;
        public readonly int Column;

        public GridPos(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool Equals(GridPos other) => Row == other.Row && Column == other.Column;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => Row * 397 ^ Column;
        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);
        public override string ToString() => $"({Row}, {Column})";
    }

    /// <summary> What happened after taking an action </summary>
    public struct StepResult
    {
        public GridPos State;
        public float Reward;
        public bool Terminal;
        public bool FellOffCliff;
    }

    /// <summary>
    ///     The environment the agent learns in: a grid of free cells, walls and cliffs with a start and a goal.
    ///     <list type="bullet">
    ///         <item>Moving into a wall or outside the grid is not possible</item>
    ///         <item>Every move costs <see cref="MoveReward"/></item>
    ///         <item>Falling off a cliff costs <see cref="CliffReward"/> and sends the agent back to the start</item>
    ///         <item>Reaching the goal gives <see cref="GoalReward"/> and ends the episode</item>
    ///     </list>
    /// </summary>
    public class GridWorld
    {
        public const int ActionCount = 4;
        public static readonly string[] ActionNames = { "Up", "Right", "Down", "Left" };
        static readonly int[] RowStep = { -1, 0, 1, 0 };
        static readonly int[] ColumnStep = { 0, 1, 0, -1 };

        readonly Cell[,] cells;

        public int Rows { get; }
        public int Columns { get; }
        public GridPos Start { get; set; }
        public GridPos Goal { get; set; }

        public float GoalReward = 100f;
        public float MoveReward = -1f;
        public float CliffReward = -100f;

        public GridWorld(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            cells = new Cell[rows, columns];
        }

        /// <summary>
        ///     Builds a grid from text rows: '#' wall, '.' free, 'C' cliff, 'S' start, 'G' goal
        /// </summary>
        public static GridWorld Parse(params string[] lines)
        {
            var world = new GridWorld(lines.Length, lines[0].Length);
            for (int row = 0; row < lines.Length; row++)
            {
                for (int column = 0; column < lines[row].Length; column++)
                {
                    char c = lines[row][column];
                    world.cells[row, column] = c == '#' ? Cell.Wall : c == 'C' ? Cell.Cliff : Cell.Free;
                    if (c == 'S') world.Start = new GridPos(row, column);
                    if (c == 'G') world.Goal = new GridPos(row, column);
                }
            }
            return world;
        }

        public Cell this[GridPos position]
        {
            get => cells[position.Row, position.Column];
            set => cells[position.Row, position.Column] = value;
        }

        public bool InBounds(GridPos p) => p.Row >= 0 && p.Row < Rows && p.Column >= 0 && p.Column < Columns;
        public bool IsWalkable(GridPos p) => InBounds(p) && this[p] != Cell.Wall;

        public static GridPos Neighbour(GridPos p, int action) => new GridPos(p.Row + RowStep[action], p.Column + ColumnStep[action]);

        /// <summary> Actions that don't bump into a wall or the border </summary>
        public bool CanMove(GridPos p, int action) => IsWalkable(Neighbour(p, action));

        public StepResult Step(GridPos state, int action)
        {
            GridPos next = Neighbour(state, action);
            if (!IsWalkable(next))
                next = state;

            if (next == Goal)
                return new StepResult { State = next, Reward = GoalReward, Terminal = true };

            if (this[next] == Cell.Cliff)
                return new StepResult { State = Start, Reward = CliffReward, FellOffCliff = true };

            return new StepResult { State = next, Reward = MoveReward };
        }

        /// <summary> Length of the shortest safe path from start to goal (BFS), or -1 if there is none </summary>
        public int ShortestPathLength()
        {
            var distance = new Dictionary<GridPos, int> { [Start] = 0 };
            var queue = new Queue<GridPos>();
            queue.Enqueue(Start);
            while (queue.Count > 0)
            {
                GridPos p = queue.Dequeue();
                if (p == Goal) return distance[p];
                for (int action = 0; action < ActionCount; action++)
                {
                    GridPos n = Neighbour(p, action);
                    if (!IsWalkable(n) || this[n] == Cell.Cliff || distance.ContainsKey(n)) continue;
                    distance[n] = distance[p] + 1;
                    queue.Enqueue(n);
                }
            }
            return -1;
        }
    }
}
