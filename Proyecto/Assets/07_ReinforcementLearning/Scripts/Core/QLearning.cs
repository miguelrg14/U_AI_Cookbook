// Miguel Rodríguez Gallego
namespace AICookbook.ReinforcementLearning
{
    /// <summary>
    ///     Q-Learning (off-policy): learns the value of acting optimally from the next state,
    ///     whatever the agent actually does next.
    ///     Q(s,a) ← Q(s,a) + α · [r + γ · max_a' Q(s',a') − Q(s,a)]
    /// </summary>
    public class QLearning : TabularLearner
    {
        public QLearning(GridWorld world, int seed = 0) : base(world, seed) { }

        public override string Name => "Q-Learning";

        protected override int Learn(GridPos state, int action, StepResult result)
        {
            float target = result.Reward;
            if (!result.Terminal)
                target += Gamma * MaxQ(result.State);

            SetQ(state, action, Q(state, action) + Alpha * (target - Q(state, action)));

            return result.Terminal ? -1 : EpsilonGreedy(result.State);
        }
    }
}
