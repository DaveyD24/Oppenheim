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
		if (Other.CompareTag(Bat.FoodTag))
		{
			Debug.Log("Mango Collected!");
		}
	}

	void OnValidate()
	{
		
	}
}
