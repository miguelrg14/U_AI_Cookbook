// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Anything that can tell the <see cref="AIDebugView"/> what an agent is deciding right now
    ///     (current action of a decision tree, current state of a state machine...).
    ///     Register it with <see cref="AIDebugView.Register(IAIDebugLabel)"/> in OnEnable.
    /// </summary>
    public interface IAIDebugLabel
    {
        /// <summary> World position where the label is drawn (usually above the agent) </summary>
        Vector3 LabelPosition { get; }

        /// <summary> Text to show, or null to hide the label </summary>
        string DebugLabel { get; }
    }
}
