using UnityEngine;

namespace AICookbook.Shared
{
	/// <summary>
	/// Runs a decision tree every frame: walks it from <see cref="rootNode"/> and executes the chosen action.
	/// </summary>
	public class DecisionTreeAgent : MonoBehaviour, IAIDebugLabel
	{
		public DecisionTreeNode rootNode;

		/// <summary>
		/// Action chosen in the last frame (shown by the AI debug view)
		/// </summary>
		public IAction CurrentAction { get; private set; }

		Transform agentRoot;

		void Awake ()
		{
			var body = GetComponentInParent<Rigidbody>();
			agentRoot = body != null ? body.transform : transform;
		}

		void OnEnable () => AIDebugView.Register(this);
		void OnDisable () => AIDebugView.Unregister(this);

		// Update is called once per frame
		void Update ()
		{
			if (rootNode == null)
			{
				return;
			}

			IAction action = rootNode.Decide();
			CurrentAction = action;

			if (action != null)
			{
				action.Act();
			}
		}

		public Vector3 LabelPosition => agentRoot.position + Vector3.up * 2.2f;

		public string DebugLabel
		{
			get
			{
				if (CurrentAction == null) return null;
				if (CurrentAction is ActionCallMethod call) return "Decision: " + call.message;
				return "Decision: " + CurrentAction.GetType().Name;
			}
		}
	}
}
