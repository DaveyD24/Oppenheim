using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SeesawPressurePoint : MonoBehaviour
{
	[Min(0f)] public float CurrentWeight;

	void OnTriggerEnter(Collider Other)
	{
		if (Other.TryGetComponent(out Rigidbody Physics))
		{
			CurrentWeight += Physics.mass;
		}
	}

	void OnTriggerExit(Collider Other)
	{
		if (Other.TryGetComponent(out Rigidbody Physics))
		{
			CurrentWeight -= Physics.mass;
		}

		// Ensure the weight never drops below zero.
		BatMathematics.ClampMin(ref CurrentWeight, 0f);
	}

	/// <summary>Automatic <see langword="implicit"/> conversion to the Weight of this Pressure Point.</summary>
	public static implicit operator float(SeesawPressurePoint SPP) => SPP.CurrentWeight;
}