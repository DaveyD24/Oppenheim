using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatEvents : MonoBehaviour
{
	Bat Bat;

	void Start()
	{
		Bat = GetComponent<Bat>();
	}

	void OnTriggerEnter(Collider Other)
	{
		if (Other.CompareTag(Bat.Food))
		{
			OnMangoCollected();
		}
	}

	void OnMangoCollected()
	{
		Debug.Log("Mango Collected!");

		Bat.AdjustEnergy(10f);
		Bat.AdjustHealth(10f);
	}
}
