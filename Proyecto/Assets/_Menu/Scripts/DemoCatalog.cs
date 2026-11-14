// Miguel Rodríguez Gallego
namespace AICookbook.Menu
{
    /// <summary>
    ///     Data shown for each demo in the main menu.
    /// </summary>
    public readonly struct DemoInfo
    {
        public readonly string Number;
        public readonly string Title;
        public readonly string Technique;
        public readonly string Description;
        public readonly string Scene;

        public DemoInfo(string number, string title, string technique, string description, string scene)
        {
            Number = number;
            Title = title;
            Technique = technique;
            Description = description;
            Scene = scene;
        }
    }

    /// <summary>
    ///     Single place to register the demos. Scene names must match the scenes in Build Profiles.
    /// </summary>
    public static class DemoCatalog
    {
        public const string MenuScene = "MainMenu";

        public static readonly DemoInfo[] Demos =
        {
            new DemoInfo("01", "Zombie Infection", "Behaviour Trees",
                "Citizens flee and zombies chase the closest target they detect. Click a citizen to infect it; Space toggles the free camera.",
                "BT_ZombieInfection"),

            new DemoInfo("02", "Predator & Prey", "Decision Trees",
                "Hunters and prey decide whether to chase, flee, wander or rest depending on stamina and distance.",
                "DT_Hunting"),

            new DemoInfo("03", "Predator & Prey", "Finite State Machines",
                "The same scenario driven by state machines, with visual feedback of the current state of each agent.",
                "FSM_Hunting"),

            new DemoInfo("04", "Procedural Terrain", "Procedural Generation",
                "A terrain mesh generated at runtime using Perlin noise.",
                "PCG_ProceduralGeneration"),

            new DemoInfo("05", "Connect Four", "Minimax · Negamax · Aspiration",
                "Choose human or AI players and pit the search algorithms against each other.",
                "Minimax_ConnectFour"),

            new DemoInfo("06", "Launch to Target", "Genetic Algorithms",
                "A population of shots evolves through selection, crossover and mutation until it hits the target.",
                "GA_LaunchTarget"),
        };
    }
}
