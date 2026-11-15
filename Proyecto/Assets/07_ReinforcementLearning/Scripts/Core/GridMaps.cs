// Miguel Rodríguez Gallego
namespace AICookbook.ReinforcementLearning
{
    /// <summary>
    ///     A map with the learning parameters that work well on it
    /// </summary>
    public readonly struct GridMap
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string[] Layout;
        public readonly float Alpha, Gamma, Epsilon;
        public readonly float GoalReward, MoveReward, CliffReward;

        public GridMap(string name, string description, string[] layout,
                       float alpha, float gamma, float epsilon, float goalReward, float moveReward, float cliffReward)
        {
            Name = name;
            Description = description;
            Layout = layout;
            Alpha = alpha;
            Gamma = gamma;
            Epsilon = epsilon;
            GoalReward = goalReward;
            MoveReward = moveReward;
            CliffReward = cliffReward;
        }

        public GridWorld CreateWorld()
        {
            var world = GridWorld.Parse(Layout);
            world.GoalReward = GoalReward;
            world.MoveReward = MoveReward;
            world.CliffReward = CliffReward;
            return world;
        }
    }

    public static class GridMaps
    {
        public static readonly GridMap Maze = new GridMap(
            "Maze", "Find the shortest way out of a maze",
            new[]
            {
                "###############",
                "#S....#.......#",
                "#.###.#.#####.#",
                "#.#...#.#...#.#",
                "#.#.###.#.#.#.#",
                "#...#.....#...#",
                "###.#.###.###.#",
                "#.....#......G#",
                "###############",
            },
            alpha: 0.3f, gamma: 0.95f, epsilon: 0.1f, goalReward: 100f, moveReward: -1f, cliffReward: -100f);

        /// <summary>
        ///     Classic example (Sutton &amp; Barto): Q-Learning learns the shortest path along the cliff edge,
        ///     SARSA a safer one away from it, because SARSA learns the value of the exploring policy it follows.
        /// </summary>
        public static readonly GridMap Cliff = new GridMap(
            "Cliff walking", "Q-Learning walks the edge, SARSA keeps away from it",
            new[]
            {
                "............",
                "............",
                "............",
                "SCCCCCCCCCCG",
            },
            alpha: 0.5f, gamma: 1f, epsilon: 0.1f, goalReward: 0f, moveReward: -1f, cliffReward: -100f);

        /// <summary> The maze of the original university assignment (console version) </summary>
        public static readonly GridMap Practice = new GridMap(
            "Practice maze", "The maze of the original console assignment",
            new[]
            {
                "########",
                "#S...#G#",
                "##...#.#",
                "#....#.#",
                "#.##...#",
                "#...##.#",
                "########",
            },
            alpha: 0.1f, gamma: 0.9f, epsilon: 0.1f, goalReward: 100f, moveReward: -1f, cliffReward: -100f);

        public static readonly GridMap[] All = { Maze, Cliff, Practice };
    }
}
