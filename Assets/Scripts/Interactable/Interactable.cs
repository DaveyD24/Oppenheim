using UnityEngine;
using UnityEngine.Events;

/// <summary>The base class for any <see cref="AButton"/> or <see cref="Switch"/>.</summary>
[RequireComponent(typeof(BoxCollider))]
public class Interactable : MonoBehaviour
{
	/// <summary>Event which fires when something Enters the Interactable Collider.</summary>
	public UnityEvent<Interactable, GameObject> OnActivated;
	/// <summary>Event which fires when something Exits the Interactable Collider.</summary>
	public UnityEvent<Interactable, GameObject> OnDeactivated;

	protected BoxCollider Collider
	{
		get
		{
			if (!BoxCollider)
				BoxCollider = GetComponent<BoxCollider>();
			return BoxCollider;
		}
		private set
		{
			BoxCollider = value;
		}
	}

	BoxCollider BoxCollider;

	public void BroadcastActive(Collider Sender)
	{
		OnActivated?.Invoke(this, Sender ? Sender.gameObject : null);
	}

	public void BroadcastDeactive(Collider Sender)
	{
		OnDeactivated?.Invoke(this, Sender ? Sender.gameObject : null);
	}
}

public struct IOData
{
	public int TotalNumberOfEntries;
	public int Count;
	public float Mass;

	public IOData Enter(GameObject Entered)
	{
		if (Entered.TryGetComponent(out Rigidbody R))
		{
			Mass += R.mass;
			++Count;

			++TotalNumberOfEntries;
		}

		return this;
	}

	public IOData Exit(GameObject Entered)
	{
		if (Entered.TryGetComponent(out Rigidbody R))
		{
			Mass -= R.mass;
			--Count;
		}

		return this;
	}
}
