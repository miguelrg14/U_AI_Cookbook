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

        public static readonly DemoInfo[] Demos =
        {
            new DemoInfo("01", "Zombie Infection", "Behaviour Trees",
                "Citizens flee and zombies chase the closest target they detect. Click a citizen to infect it.",
                "BT_ZombieInfection",
                new[] { "Left click on a citizen: infect it", FreeCamera },
                "Zombies chase the closest citizen in range and citizens run away from the closest zombie. " +
                "With nobody around, both wander. Every citizen a zombie touches becomes a zombie too."),

            new DemoInfo("02", "Predator & Prey", "Decision Trees",
                "Hunters and prey decide whether to chase, flee, wander or rest depending on stamina and distance.",
                "DT_Hunting",
                new[] { FreeCamera },
                "The icon above each agent is the action its decision tree picked this frame. " +
                "Watch the stamina bars: an agent that runs out of stamina must rest until it is full again, " +
                "which gives the prey a chance to escape."),

            new DemoInfo("03", "Predator & Prey", "Finite State Machines",
                "The same scenario driven by state machines, with visual feedback of the current state of each agent.",
                "FSM_Hunting",
                new[] { FreeCamera },
                "The icon above each agent is its current state. Compare it with demo 02: here behaviour only changes " +
                "through explicit transitions. Prey also watch each other, so when one escapes nearby ones panic."),

            new DemoInfo("04", "Procedural Terrain", "Procedural Generation",
                "A terrain mesh generated at runtime using Perlin noise.",
                "PCG_ProceduralGeneration",
                new[] { FreeCamera },
                "The mesh is built vertex by vertex when the scene starts: a grid whose heights come from Perlin noise, " +
                "joined into triangles. Fly close to see its shape."),

            new DemoInfo("05", "Connect Four", "Minimax · Negamax · Aspiration",
                "Choose human or AI players and pit the search algorithms against each other.",
                "Minimax_ConnectFour",
                new[] { "Pick a player type for each side and start the game", "Left click on a column: drop a piece (human player)" },
                "Each AI explores future moves and scores the boards it reaches. Minimax and Negamax search the same tree; " +
                "Aspiration search starts with a narrow alpha-beta window to prune more branches. Try AI vs AI."),

            new DemoInfo("06", "Launch to Target", "Genetic Algorithms",
                "A population of shots evolves through selection, crossover and mutation until it hits the target.",
                "GA_LaunchTarget",
                new[] { "Space: shoot with the best individual found so far" },
                "Every shot is an individual: two angles and a strength. After each generation the best ones are selected, " +
                "crossed and mutated, so shots land closer and closer to the target."),
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
