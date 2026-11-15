// Miguel Rodríguez Gallego
namespace AICookbook.ReinforcementLearning
{
    /// <summary>
    ///     SARSA (on-policy): learns the value of the action it will really take next, exploration included.
    ///     Its name comes from the tuple it uses: State, Action, Reward, next State, next Action.
    ///     Q(s,a) ← Q(s,a) + α · [r + γ · Q(s',a') − Q(s,a)]
    /// </summary>
    public class Sarsa : TabularLearner
    {
        public Sarsa(GridWorld world, int seed = 0) : base(world, seed) { }

        public override string Name => "SARSA";

        protected override int Learn(GridPos state, int action, StepResult result)
        {
            // The next action is chosen first, because its value is part of the update
            int nextAction = result.Terminal ? -1 : EpsilonGreedy(result.State);

            float target = result.Reward;
            if (!result.Terminal && nextAction >= 0)
                target += Gamma * Q(result.State, nextAction);

            SetQ(state, action, Q(state, action) + Alpha * (target - Q(state, action)));
            return nextAction;
        }
    }
}
