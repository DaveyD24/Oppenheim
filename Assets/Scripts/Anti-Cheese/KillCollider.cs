using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class KillCollider : MonoBehaviour
{

	void Start()
	{
		GetComponent<BoxCollider>().isTrigger = true;
	}

	void OnTriggerEnter(Collider Player)
	{
		if (Player.CompareTag("Player"))
		{
			if (Player.transform.root.TryGetComponent(out PlayerController Controller))
			{
				Controller.OnDeath();
			}
		}
	}
}
