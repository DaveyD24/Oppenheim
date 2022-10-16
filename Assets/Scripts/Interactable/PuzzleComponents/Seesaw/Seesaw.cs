using UnityEngine;
using static global::BatMathematics;

public class Seesaw : MonoBehaviour
{
	[Header("Ends")]
	[SerializeField] SeesawPressurePoint In;
	[SerializeField] SeesawPressurePoint Out;

	[Header("Weight")]
	[SerializeField, Min(1f), Tooltip("The Mass in which this Seesaw is fully tilted.")] float MaxWeight;
	[SerializeField, ReadOnly] float InWeight, OutWeight;

	[Header("Pitch Angle")]
	[SerializeField, Tooltip("The starting Pitch of this Seesaw.")] float DefaultPitch;
	[SerializeField, Tooltip("The Pitch of this Seesaw when fully tilted.")] float MaxPitch;
#if UNITY_EDITOR
	// This is to debug the Pitch angle.
	[SerializeField, ReadOnly] float CurrentPitch;
#endif

	[SerializeField, Tooltip("How many degrees should this Seesaw be able to rotate?")] float MaxRotationDeltaAngle;

	float InRatio;
	float OutRatio;
	float SmoothDampVelocity;
	//[SerializeField] private float timeReachTarget = 0.05f; // Doesn't work.

	void FixedUpdate()
	{
		InWeight = -In; // Inbound Weight should 'drop' the Seesaw.
		OutWeight = Out;

		// NaN Checks.
		ClampMax(ref InWeight, MaxWeight);
		ClampMax(ref OutWeight, MaxWeight);

		InRatio = InWeight / MaxWeight;
		OutRatio = OutWeight / MaxWeight;

		float Average = (InRatio + OutRatio) * .5f;

		// Rotate.
		float Pitch = Mathf.Lerp(DefaultPitch, MaxPitch, Spring(Average));
		Pitch = WrapAngle(Pitch);

		// More NaN Checks.
		if (!DiagnosticCheckNaN(Pitch))
		{
			float SmoothedPitch = Mathf.SmoothDampAngle(transform.localEulerAngles.x, Pitch, ref SmoothDampVelocity, .1f, MaxRotationDeltaAngle);
			transform.localEulerAngles = new Vector3(SmoothedPitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
		}
#if UNITY_EDITOR
		CurrentPitch = Pitch;
#endif
	}

	// From Fallen Order.
	float Spring(float T)
	{
		return Mathf.Sin(T * Mathf.PI * (.2f + 2.5f * T * T * T)) * Mathf.Pow(1f - T, 2.2f) + T * (1f + 1.2f * (1f - T));
	}

	void OnValidate()
	{
		if (MaxPitch < -90f || MaxPitch > 90f)
		{
			Debug.LogError("<color=#FF4444>Please keep it between -90 and +90 degrees to stop it from shaking.</color>");
			MaxPitch = 90f * Mathf.Sign(MaxPitch);
		}

		DefaultPitch = WrapAngle(DefaultPitch);
		MaxPitch = WrapAngle(MaxPitch);
	}

	float WrapAngle(float Angle)
	{
		if (Angle > 180f)
			return Angle - 360;
		return Angle;
	}

	//void OnGUI()
	//{
	//	int sh = Screen.height - 50;
	//	GUI.Label(new Rect(10, sh - 10, 250, 250), $"Pitch: {transform.localEulerAngles.x:F2}");
	//}
}
