// Miguel Rodríguez Gallego
using System;
using System.Collections.Generic;

namespace AICookbook.ReinforcementLearning
{
    /// <summary>
    ///     Common part of Q-Learning and SARSA: a Q table with the value of each action in each cell,
    ///     epsilon-greedy action selection and the episode loop. Subclasses only decide how the Q value
    ///     is updated after each step (<see cref="Learn"/>), which is exactly where both algorithms differ.
    ///     It can run a whole episode at once (fast training) or one step at a time (to watch the agent).
    /// </summary>
    public abstract class TabularLearner
    {
        public float Alpha = 0.1f;   // learning rate
        public float Gamma = 0.95f;  // discount factor
        public float Epsilon = 0.1f; // exploration rate
        public int MaxStepsPerEpisode = 2000;

        protected readonly GridWorld World;
        readonly Random random;
        float[,,] q;

        public abstract string Name { get; }

        public int Episodes { get; private set; }
        public int TotalSteps { get; private set; }
        public readonly List<int> StepsPerEpisode = new List<int>();
        public readonly List<float> RewardPerEpisode = new List<float>();
        public readonly List<int> FallsPerEpisode = new List<int>();

        // Current episode (step by step)
        public GridPos AgentPosition { get; private set; }
        public bool EpisodeRunning { get; private set; }
        public int CurrentEpisodeSteps { get; private set; }
        int pendingAction;
        float currentReward;
        int currentFalls;

        protected TabularLearner(GridWorld world, int seed)
        {
            World = world;
            random = new Random(seed);
            q = new float[world.Rows, world.Columns, GridWorld.ActionCount];
            AgentPosition = world.Start;
        }

        // ---------------------------------------------------------------- Q table

        public float Q(GridPos p, int action) => q[p.Row, p.Column, action];
        protected void SetQ(GridPos p, int action, float value) => q[p.Row, p.Column, action] = value;

        /// <summary> Value of the best possible action in a cell (0 if the agent can't move) </summary>
        public float MaxQ(GridPos p)
        {
            float best = float.NegativeInfinity;
            for (int action = 0; action < GridWorld.ActionCount; action++)
                if (World.CanMove(p, action) && Q(p, action) > best)
                    best = Q(p, action);
            return float.IsNegativeInfinity(best) ? 0f : best;
        }

        /// <summary> Best action in a cell; ties are broken at random so unexplored cells don't bias a direction </summary>
        public int GreedyAction(GridPos p) => BestAction(p, randomTies: true);

        /// <summary> Best action without randomness (to draw the learned policy) or -1 if it can't move </summary>
        public int PolicyAction(GridPos p) => BestAction(p, randomTies: false);

        int BestAction(GridPos p, bool randomTies)
        {
            float best = float.NegativeInfinity;
            int chosen = -1, ties = 0;
            for (int action = 0; action < GridWorld.ActionCount; action++)
            {
                if (!World.CanMove(p, action)) continue;
                float value = Q(p, action);
                if (value > best)
                {
                    best = value;
                    chosen = action;
                    ties = 1;
                }
                else if (randomTies && value == best && random.Next(++ties) == 0)
                {
                    chosen = action; // reservoir sampling: every tied action has the same chance
                }
            }
            return chosen;
        }

        /// <summary> With probability epsilon a random possible action (explore), otherwise the best one (exploit) </summary>
        protected int EpsilonGreedy(GridPos p)
        {
            if (random.NextDouble() < Epsilon)
            {
                int count = 0, chosen = -1;
                for (int action = 0; action < GridWorld.ActionCount; action++)
                    if (World.CanMove(p, action) && random.Next(++count) == 0)
                        chosen = action;
                return chosen;
            }
            return GreedyAction(p);
        }

        /// <summary>
        ///     Updates Q(state, action) after observing <paramref name="result"/> and returns the next action to take
        /// </summary>
        protected abstract int Learn(GridPos state, int action, StepResult result);

        // ---------------------------------------------------------------- episodes

        public void BeginEpisode()
        {
            AgentPosition = World.Start;
            pendingAction = EpsilonGreedy(AgentPosition);
            CurrentEpisodeSteps = 0;
            currentReward = 0f;
            currentFalls = 0;
            EpisodeRunning = true;
        }

        /// <summary> Takes one action (starting a new episode if needed) and learns from it </summary>
        public StepResult Step()
        {
            if (!EpisodeRunning)
                BeginEpisode();

            if (pendingAction < 0) // walled in: nothing to learn here
            {
                EndEpisode();
                return new StepResult { State = AgentPosition };
            }

            StepResult result = World.Step(AgentPosition, pendingAction);
            int next = Learn(AgentPosition, pendingAction, result);

            AgentPosition = result.State;
            CurrentEpisodeSteps++;
            TotalSteps++;
            currentReward += result.Reward;
            if (result.FellOffCliff) currentFalls++;

            if (result.Terminal || CurrentEpisodeSteps >= MaxStepsPerEpisode)
                EndEpisode();
            else
                pendingAction = next;

            return result;
        }

        /// <summary> Runs a whole episode and returns its number of steps </summary>
        public int RunEpisode()
        {
            BeginEpisode();
            while (EpisodeRunning)
                Step();
            return StepsPerEpisode[StepsPerEpisode.Count - 1];
        }

        void EndEpisode()
        {
            EpisodeRunning = false;
            Episodes++;
            StepsPerEpisode.Add(CurrentEpisodeSteps);
            RewardPerEpisode.Add(currentReward);
            FallsPerEpisode.Add(currentFalls);
        }

        // ---------------------------------------------------------------- learned policy

        /// <summary>
        ///     Path from the start following the best action of each cell, without exploring.
        ///     Ends at the goal, or earlier if the policy loops or doesn't know where to go yet.
        /// </summary>
        public List<GridPos> GreedyPath(int maxSteps = 500)
        {
            var path = new List<GridPos> { World.Start };
            var visited = new HashSet<GridPos> { World.Start };
            GridPos p = World.Start;

            for (int i = 0; i < maxSteps; i++)
            {
                int action = PolicyAction(p);
                if (action < 0) break;

                StepResult result = World.Step(p, action);
                p = result.State;
                path.Add(p);
                if (result.Terminal || result.FellOffCliff || !visited.Add(p)) break;
            }
            return path;
        }

        public bool GreedyPathReachesGoal(out int length)
        {
            var path = GreedyPath();
            length = path.Count - 1;
            return path[path.Count - 1] == World.Goal;
        }

        /// <summary> Forgets what was learned in a cell (e.g. it became a wall) </summary>
        public void ClearCell(GridPos p)
        {
            for (int action = 0; action < GridWorld.ActionCount; action++)
                SetQ(p, action, 0f);
        }
    }
}
