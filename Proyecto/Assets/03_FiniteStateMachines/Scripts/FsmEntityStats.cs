// Miguel Rodríguez Gallego
namespace AICookbook.FiniteStateMachines
{
    using AICookbook.Shared;

    /// <summary>
    ///     Stamina (from <see cref="EntityStats"/>) plus the flags the state machine uses
    ///     to coordinate the prey group: whether this agent is panicking or escaping.
    /// </summary>
    public class FsmEntityStats : EntityStats
    {
        public bool isPanicking;
        public bool isEscaping;
    }
}
