using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class SeesawPressurePoint : MonoBehaviour
{
	[Min(0f)] public float CurrentWeight;
	public float maxDistanceAway;

	[SerializeField] private GameObject rotationObject;
	private Dictionary<GameObject, float> objDistAwayMass = new Dictionary<GameObject, float>(); // dictionary holding each player obj and the current mass of it, based on how far from the center it is 

	/// <summary>
	/// return the total mass of the object.
	/// </summary>
	/// <returns>the total mass stored for this pressure point.</returns>
	public float GetSideMass()
	{
		float totalMass = 0;
		foreach (var item in objDistAwayMass)
		{
			totalMass += item.Value;
		}

		return totalMass;
	}

	private void OnTriggerEnter(Collider other)
	{
		GameObject obj = other.gameObject.name == "CarBody" ? other.transform.parent.gameObject : other.gameObject;

		if (obj.TryGetComponent(out Rigidbody physics))
		{
			CurrentWeight += physics.mass;
			if (objDistAwayMass.ContainsKey(obj))
            {
				objDistAwayMass.Remove(obj);
            }

			objDistAwayMass.Add(obj, 0);

			// obj.transform.SetParent(transform.parent, true);
		}
    }

	private void OnTriggerStay(Collider other)
	{
		GameObject obj = other.gameObject.name == "CarBody" ? other.transform.parent.gameObject : other.gameObject;

		if (obj.TryGetComponent(out Rigidbody physics))
		{
			if (objDistAwayMass.ContainsKey(obj))
            {
				float newMass = Mathf.Abs(rotationObject.transform.position.z - obj.transform.position.z);

				// Debug.Log(newMass);
				newMass /= maxDistanceAway;
				newMass = Mathf.Clamp(newMass, 0, 1);
				objDistAwayMass[obj] = (newMass) * physics.mass;
            }
			else
            {
				objDistAwayMass.Remove(obj);
            }
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawRay(rotationObject.transform.position, rotationObject.transform.forward * maxDistanceAway);
	}

	private void OnTriggerExit(Collider other)
	{
		GameObject obj = other.gameObject.name == "CarBody" ? other.transform.parent.gameObject : other.gameObject;

		if (obj.TryGetComponent(out Rigidbody physics))
		{
			CurrentWeight -= physics.mass;
			objDistAwayMass.Remove(obj);
			obj.transform.SetParent(null, true);
		}

		// Ensure the weight never drops below zero.
		BatMathematics.ClampMin(ref CurrentWeight, 0f);
	}
}