using UnityEngine;

/// The naming of this class is to avoid <see cref="UnityEngine.UI.Button"/>
/// and <see cref="Button"/> and definitely did not draw inspiration from UE.
public class AButton : Interactable
{
	[SerializeField, Min(0f)] float RequiredMassToActivate;
	[SerializeField, Tooltip("True if this button should NOT deactivate once activated.")] bool bIsPersistent;

	bool bIsOn = false;
	IOData IO;

	void OnTriggerEnter(Collider Entered)
	{
		float Mass = IO.Enter(Entered.gameObject).Mass;
		
		if (!bIsOn && Mass >= RequiredMassToActivate)
		{
			BroadcastActive(Entered);
			bIsOn = true;
		}
	}

	void OnTriggerExit(Collider Exited)
	{
		IO.Exit(Exited.gameObject);

		if (bIsOn && !bIsPersistent)
		{
			BroadcastDeactive(Exited);
			bIsOn = false;
		}
	}
}
