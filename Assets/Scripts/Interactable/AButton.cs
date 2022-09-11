using UnityEngine;

/// The naming of this class is to avoid <see cref="UnityEngine.UI.Button"/>.
public class AButton : Interactable
{
	[SerializeField, Min(0f)] float RequiredMassToActivate;
	[SerializeField, Tooltip("True if this button should NOT deactivate once activated.")] bool bIsPersistent;

	IOData IO;

	void OnTriggerEnter(Collider Entered)
	{
		float Mass = IO.Enter(Entered.gameObject).Mass;
		
		if (Mass >= RequiredMassToActivate)
		{
			BroadcastActive(Entered);
		}
	}

	void OnTriggerExit(Collider Exited)
	{
		IO.Exit(Exited.gameObject);

		if (!bIsPersistent)
		{
			BroadcastDeactive(Exited);
		}
	}
}
