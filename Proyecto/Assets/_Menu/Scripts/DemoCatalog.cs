// Miguel Rodríguez Gallego
namespace AICookbook.Menu
{
    /// <summary>
    ///     Data shown for each demo in the main menu and in its in-demo help panel.
    /// </summary>
    public readonly struct DemoInfo
    {
        public readonly string Number;
        public readonly string Title;
        public readonly string Technique;
        public readonly string Description;
        public readonly string Scene;
        /// <summary> Controls available in the demo, one per line </summary>
        public readonly string[] Controls;
        /// <summary> What the viewer should pay attention to </summary>
        public readonly string WhatToWatch;

        public DemoInfo(string number, string title, string technique, string description, string scene,
                        string[] controls, string whatToWatch)
        {
            Number = number;
            Title = title;
            Technique = technique;
            Description = description;
            Scene = scene;
            Controls = controls;
            WhatToWatch = whatToWatch;
        }
    }

    /// <summary>
    ///     Single place to register the demos. Scene names must match the scenes in Build Profiles.
    /// </summary>
    public static class DemoCatalog
    {
        public const string MenuScene = "MainMenu";

        const string FreeCamera = "Space: toggle free camera (WASD + mouse, Shift to go faster)";
        const string DebugView = "G: AI debug view (detection ranges, targets and current decision)";

        public static readonly DemoInfo[] Demos =
        {
            new DemoInfo("01", "Zombie Infection", "Behaviour Trees",
                "Citizens flee and zombies chase the closest target they detect. Click a citizen to infect it.",
                "BT_ZombieInfection",
                new[] { "Left click on a citizen: infect it", DebugView, FreeCamera },
                "Zombies chase the closest citizen in range and citizens run away from the closest zombie. " +
                "With nobody around, both wander. Every citizen a zombie touches becomes a zombie too."),

            new DemoInfo("02", "Predator & Prey", "Decision Trees",
                "Hunters and prey decide whether to chase, flee, wander or rest depending on stamina and distance.",
                "DT_Hunting",
                new[] { DebugView, FreeCamera },
                "The icon above each agent is the action its decision tree picked this frame. " +
                "Watch the stamina bars: an agent that runs out of stamina must rest until it is full again, " +
                "which gives the prey a chance to escape."),

            new DemoInfo("03", "Predator & Prey", "Finite State Machines",
                "The same scenario driven by state machines, with visual feedback of the current state of each agent.",
                "FSM_Hunting",
                new[] { DebugView, FreeCamera },
                "The icon above each agent is its current state. Compare it with demo 02: here behaviour only changes " +
                "through explicit transitions. Prey also watch each other, so when one escapes nearby ones panic."),

            new DemoInfo("04", "Procedural Terrain", "Procedural Generation",
                "A terrain mesh generated at runtime using Perlin noise.",
                "PCG_ProceduralGeneration",
                new[] { FreeCamera },
                "The mesh is built vertex by vertex when the scene starts: a grid whose heights come from Perlin noise, " +
                "joined into triangles. Fly close to see its shape."),

            new DemoInfo("05", "Connect Four", "Minimax · Negamax α-β · Aspiration",
                "Choose human or AI players and pit the search algorithms against each other.",
                "Minimax_ConnectFour",
                new[] { "Pick a player type for each side and start the game", "Left click on a column: drop a piece (human player)",
                        "G: AI debug view (chosen column, score, positions searched and time)" },
                "The three AIs search the same game tree and reach the same decisions; what changes is the cost. " +
                "Minimax explores every move, Negamax with alpha-beta skips branches that cannot change the result " +
                "(it searches deeper with far fewer positions) and Aspiration bets on a narrow score window. " +
                "Press G during an AI vs AI game to compare how many positions each one visits."),

            new DemoInfo("06", "Launch to Target", "Genetic Algorithms",
                "A population of shots evolves through selection, crossover and mutation until it hits the target.",
                "GA_LaunchTarget",
                new[] { "Space: shoot with the best individual found so far" },
                "Every shot is an individual: two angles and a strength. After each generation the best ones are selected, " +
                "crossed and mutated, so shots land closer and closer to the target."),
            new DemoInfo("07", "Learning a Maze", "Reinforcement Learning",
                "Q-Learning and SARSA learn by trial and error which way to go; watch the values spread from the goal.",
                "RL_GridWorld",
                new[] { "Right panel: map, algorithm, parameters and speed", "Left click on a cell: wall on/off",
                        "Right click: move the goal  ·  Shift + click: move the start" },
                "Each cell's colour is the value learned for it and the arrows are the best action found so far. " +
                "The values spread backwards from the goal episode after episode until the cyan path is the shortest one. " +
                "In 'Cliff walking', compare both algorithms: Q-Learning learns the risky path along the edge and " +
                "SARSA a safer one, because it accounts for its own exploration."),
        };

        /// <summary>
        ///     Demo registered for <paramref name="scene"/>, if any
        /// </summary>
        public static bool TryGet(string scene, out DemoInfo demo)
        {
            foreach (var d in Demos)
            {
                if (d.Scene == scene)
                {
                    demo = d;
                    return true;
                }
            }
            demo = default;
            return false;
        }
    }
}
