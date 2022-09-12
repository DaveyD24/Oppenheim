using UnityEngine;

public static class BatMathematics
{
	/// 
	/// MATH FUNCTIONS
	/// 

	public static Vector3 ComputeJumpVelocity(Vector3 Up, float DesiredHeight)
	{
		return Up * ComputeJumpScalar(DesiredHeight);
	}

	/// <summary>Calculates the force required to reach DesiredHeight.</summary>
	public static float ComputeJumpScalar(float DesiredHeight)
	{
		/*
		 * V^2 = U^2 + 2AS  (-2AS)
		 * U^2 = V^2 - 2AS  (Rearranged for U)
		 * 
		 * JumpVelocity ^ 2 = FinalVelocity ^ 2 - 2 * Gravity * DesiredHeight
		 * JumpVelocity ^ 2 = 0 - 2 * Gravity * DesiredHeight
		 * JumpVelocity = Sqrt(-2 * Gravity * DesiredHeight)
		 */

		float UU = -2f * Physics.gravity.y * DesiredHeight;

		return Mathf.Sqrt(UU);
	}

	// Lift Constants.
	const float kLiftCoefficient = .03f; // Cl (NASA says this value is experimental, but keep within Epsilon and .05)
	const float kWingArea = .577f; // A
	const float kAirDensity = 1f; // ? (Rho)
	const float kClDAOver2 = kLiftCoefficient * kWingArea * .5f * kAirDensity;

	/// <summary>Calculates the generation of Lift according to a number of variables.</summary>
	/// <returns>The force of Lift in m/s.</returns>
	public static float ComputeLift(Rigidbody Physics)
	{
		// https://www.grc.nasa.gov/WWW/K-12/airplane/lifteq.html
		float Speed = Physics.velocity.magnitude;

		return kClDAOver2 * Speed * Speed;
	}

	/// <summary>Rotates a Vector about an Axis by Angle degrees.</summary>
	/// <remarks>
	/// Because you can't really define 0-degrees with Vector angles,
	/// <paramref name="Angle"/> should be passed in as a delta angle instead
	/// of a raw angle-in-degrees.
	/// <br></br><br></br>
	/// <b>While looking towards -Axis:</b><br></br>
	/// + Angle is CW. - Angle is CCW.
	/// </remarks>
	public static Vector3 RotateVector(Vector3 Vector, Vector3 Axis, float Angle)
	{
		if (IsZero(Angle))
			return Vector;

		SinCos(out float S, out float C, Angle * Mathf.Deg2Rad);

		float XX = Axis.x * Axis.x;
		float YY = Axis.y * Axis.y;
		float ZZ = Axis.z * Axis.z;

		float XY = Axis.x * Axis.y;
		float YZ = Axis.y * Axis.z;
		float ZX = Axis.z * Axis.x;

		float XS = Axis.x * S;
		float YS = Axis.y * S;
		float ZS = Axis.z * S;

		float OMC = 1f - C;

		return new Vector3(
			(OMC * XX + C) * Vector.x + (OMC * XY - ZS) * Vector.y + (OMC * ZX + YS) * Vector.z,
			(OMC * XY + ZS) * Vector.x + (OMC * YY + C) * Vector.y + (OMC * YZ - XS) * Vector.z,
			(OMC * ZX - YS) * Vector.x + (OMC * YZ + XS) * Vector.y + (OMC * ZZ + C) * Vector.z
		);
	}

	/// <summary>Computes the Sine and Cosine of a given Angle.</summary>
	public static void SinCos(out float Sine, out float Cosine, float Angle)
	{
		const float kInversePI = 1f / Mathf.PI;
		const float kHalfPI = Mathf.PI * .5f;

		float Quotient = kInversePI * .5f * Angle;

		Quotient = (int)(Quotient + (Angle >= 0f ? .5f : -.5f));

		float A = Angle - 2f * Mathf.PI * Quotient;

		// Map A to [-PI / 2, PI / 2] with Sin(A) = Sin(Value).
		float Sign;
		if (A > kHalfPI)
		{
			A = Mathf.PI - A;
			Sign = -1f;
		}
		else if (A < -kHalfPI)
		{
			A = -Mathf.PI - A;
			Sign = -1f;
		}
		else
		{
			Sign = +1f;
		}

		float A2 = A * A;

		// Fast Sine Cosine Approximations.
		// 11-degree minimax Sine. https://publik-void.github.io/sin-cos-approximations/#_sin_rel_error_minimized_degree_11
		// 10-degree minimax Cosine. https://publik-void.github.io/sin-cos-approximations/#_cos_abs_error_minimized_degree_10

		Sine = (((((-2.3889859e-08f * A2 + 2.7525562e-06f) * A2 - 0.00019840874f) * A2 + 0.0083333310f) * A2 - 0.16666667f) * A2 + 1.0f) * A;

		Cosine = Sign * ((((-2.6051615e-07f * A2 + 2.4760495e-05f) * A2 - 0.0013888378f) * A2 + 0.041666638f) * A2 - 0.5f) * A2 + 1.0f;
	}

	/// <summary>Converts a world direction to be relative to the Reference's forward.</summary>
	public static Vector3 DirectionRelativeToTransform(Transform Reference, Vector3 Direction, bool bIgnoreYAxis = true)
	{
		Vector3 ReferenceForward = Reference.forward;
		Vector3 ReferenceRight = Reference.right;

		if (bIgnoreYAxis)
			ReferenceForward.y = ReferenceRight.y = 0f;

		ReferenceForward.Normalize();
		ReferenceRight.Normalize();

		float LeftRight = Direction.x;
		float ForwardBackward = Direction.z;

		Vector3 RelativeMovementVector = ReferenceForward * ForwardBackward + ReferenceRight * LeftRight;

		return RelativeMovementVector;
	}

	public static void AlignTransformToMovement(Transform Transform, Vector3 MovementVector, float RotationSpeed, Vector3 UpAxis)
	{
		Quaternion RotationNow = Transform.rotation;
		Quaternion TargetRotation = Quaternion.LookRotation(MovementVector, UpAxis);
		Transform.rotation = Quaternion.RotateTowards(RotationNow, TargetRotation, RotationSpeed);
	}

	/// <summary>True if F is close enough to zero.</summary>
	public static bool IsZero(float F)
	{
		return Mathf.Abs(F) <= .01f;
	}

	/// <summary>True if V is close enough to zero.</summary>
	/// <remarks>'Close enough' is define in <see cref="IsZero(float)"/>.</remarks>
	public static bool IsZero(Vector3 V)
	{
		return IsZero(V.x) && IsZero(V.y) && IsZero(V.z);
	}

	/// <summary>Sets V to Vector3.zero if it's close enough to zero.</summary>
	/// <remarks>'Close enough' is define in <see cref="IsZero(float)"/>.</remarks>
	public static void ForceZeroIfZero(ref Vector3 V)
	{
		// Vector3's == operator is accurate to: 9.99999944 E-11 (0.0000000000999999944)
		// This is too accurate; define our own threshold.
		if (IsZero(V))
			V = Vector3.zero;
	}
}
