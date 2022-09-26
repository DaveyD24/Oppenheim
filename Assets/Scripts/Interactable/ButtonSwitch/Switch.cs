using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Switch : Interactable
{
	public UnityEvent SwitchedOn;
	public UnityEvent SwitchedOff;

	// Read-only - Can only be modified with OnTriggerEnter.
	// Use the Inspector Button 'Toggle Switch' during play instead.
	[ReadOnly] public bool bIsOn = false;

	[SerializeField] Switch[] ReliantSwitches;

	Action<Switch> ReliantListeners;

	void Start()
	{
		// Attach the main Listener to the Switches that this Switch relies on.
		foreach (Switch Reliant in ReliantSwitches)
		{
			Reliant.ReliantListeners += OnAnyReliantListener;
		}
	}

	public virtual void OnTriggerEnter(Collider Entered)
	{
		// Turn this Switch ON/OFF.
		ToggleSwitch();

		// Fires the ON/OFF Broadcasts.
		CheckBroadcast(Entered);
	}

	// Trigger this Switch regardless of any reliant Switches.
	public virtual void ToggleSwitch()
	{
		bIsOn = !bIsOn;

		if (bIsOn)
		{
			SwitchedOn?.Invoke();
		}
		else
		{
			SwitchedOff?.Invoke();
		}

		ReliantListeners?.Invoke(this);
	}

	/// <summary>Activates or Deactivates this Switch.</summary>
	/// <remarks>Only Activates when all the Broadcaster's <see cref="ReliantSwitches"/> are <see cref="bIsOn"/>.</remarks>
	/// <param name="Broadcaster">
	/// The GameObject that entered and triggered this Broadcast,
	/// <br></br><b>
	/// OR
	/// </b><br></br>
	/// The last Switch to turn ON before this Broadcast.
	/// </param>
	void CheckBroadcast(Collider Broadcaster)
	{
		if (bIsOn && AreAllReliantSwitchesOn())
		{
			// Turn the Switch ON.
			BroadcastActive(Broadcaster);
		}
		else
		{
			// Turn the Switch OFF.
			BroadcastDeactive(Broadcaster);
		}
	}

	// Also fire ON/OFF Broadcasts if a reliant Switch is triggered.
	void OnAnyReliantListener(Switch Broadcast)
	{
		CheckBroadcast(Broadcast.Collider);
	}

	/// <returns><see langword="true"></see> if all <see cref="ReliantSwitches"/> are <see cref="bIsOn"/>.</returns>
	bool AreAllReliantSwitchesOn()
	{
		if (ReliantSwitches.Length == 0)
			return true;

		return ReliantSwitches.All(Switch => Switch.bIsOn);
	}
}
