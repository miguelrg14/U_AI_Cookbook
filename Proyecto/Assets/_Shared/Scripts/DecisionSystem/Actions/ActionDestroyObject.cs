using UnityEngine;
using System.Collections;

namespace AICookbook.Shared
{

	/// <summary>
	/// Destroys an object
	/// </summary>
	public class ActionDestroyObject : IAction
	{
		public GameObject objectToDestroy;


		public override void Act ()
		{
			if (objectToDestroy == null)	return;

			Destroy(objectToDestroy);
		}
	}
}
