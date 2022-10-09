using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SeesawPressurePoint : MonoBehaviour
{
	[Min(0f)] public float CurrentWeight;

	private void OnTriggerEnter(Collider other)
	{
		GameObject obj = other.gameObject.name == "CarBody" ? other.transform.parent.gameObject : other.gameObject;

		if (obj.TryGetComponent(out Rigidbody physics))
        {
            CurrentWeight += physics.mass;
        }
    }

	private void OnTriggerExit(Collider other)
	{
		GameObject obj = other.gameObject.name == "CarBody" ? other.transform.parent.gameObject : other.gameObject;

		if (obj.TryGetComponent(out Rigidbody physics))
		{
			CurrentWeight -= physics.mass;
		}

		// Ensure the weight never drops below zero.
		BatMathematics.ClampMin(ref CurrentWeight, 0f);
	}

    /// <summary>Automatic <see langword="implicit"/> conversion to the Weight of this Pressure Point.</summary>
	public static implicit operator float(SeesawPressurePoint sPP) => sPP.CurrentWeight;
}