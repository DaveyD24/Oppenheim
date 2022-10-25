using UnityEngine;

/// The naming of this class is to avoid <see cref="UnityEngine.UI.Button"/>
/// and <see cref="Button"/> and definitely did not draw inspiration from UE.
public class AButton : Interactable
{
	[SerializeField, Min(0f)] float RequiredMassToActivate;
	[SerializeField, Tooltip("True if this button should NOT deactivate once activated.")] bool bIsPersistent;

	bool bIsOn = false;
	IOData IO;

	private void OnTriggerEnter(Collider Entered)
	{
		// as the car's gameobject which has a rigidbody also does not have any colliders, need to specifically handle this case
		float Mass = Entered.gameObject.name == "CarBody" ? IO.Enter(Entered.transform.parent.gameObject).Mass : IO.Enter(Entered.gameObject).Mass;

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
