using UnityEngine;

/// <summary>
/// Purpose-built mathematics function library for the Bat's behaviours and functions.
/// <br></br>
/// Includes implementations for Kinematics, Fast Approximations, Aerodynamics,
/// and Vector Operations.
/// </summary>
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

		return FSqrt(UU);
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
		{
			return Vector;
		}

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

	/// <summary>Converts a world direction to be relative to the Reference's forward.</summary>
	public static Vector3 DirectionRelativeToTransform(Transform Reference, Vector3 Direction, bool bIgnoreYAxis = true)
	{
		Vector3 ReferenceForward = Reference.forward;
		Vector3 ReferenceRight = Reference.right;

		if (bIgnoreYAxis)
		{
			ReferenceForward.y = ReferenceRight.y = 0f;
		}

		ReferenceForward.Normalize();
		ReferenceRight.Normalize();

		float LeftRight = Direction.x;
		float ForwardBackward = Direction.z;

		Vector3 RelativeMovementVector = (ReferenceForward * ForwardBackward) + (ReferenceRight * LeftRight);

		return RelativeMovementVector;
	}

	public static void AlignTransformToMovement(Transform Transform, Vector3 MovementVector, float RotationSpeed, Vector3 UpAxis)
	{
		Quaternion RotationNow = Transform.rotation;
		Quaternion TargetRotation = Quaternion.LookRotation(MovementVector, UpAxis);
		Transform.rotation = Quaternion.RotateTowards(RotationNow, TargetRotation, RotationSpeed);
	}

	public const float kZeroThreshold = .01f;

	/// <summary>True if F is close enough to zero.</summary>
	public static bool IsZero(float F, float Threshold = kZeroThreshold)
	{
		return Mathf.Abs(F) <= Threshold;
	}

	/// <summary>True if V is close enough to zero.</summary>
	/// <remarks>'Close enough' is define in <see cref="IsZero(float)"/>.</remarks>
	public static bool IsZero(Vector3 V, float Threshold = kZeroThreshold)
	{
		return IsZero(V.x, Threshold) && IsZero(V.y, Threshold) && IsZero(V.z, Threshold);
	}

	/// <summary>Sets V to Vector3.zero if it's close enough to zero.</summary>
	/// <remarks>'Close enough' is define in <see cref="IsZero(float)"/>.</remarks>
	public static void SetZeroIfZero(ref Vector3 V, bool bUseForce = false, float Threshold = kZeroThreshold)
	{
		// Vector3's == operator is accurate to: 9.99999944 E-11 (0.0000000000999999944)
		// This is too accurate; define our own threshold.
		if (IsZero(V, Threshold))
		{
			if (!bUseForce)
			{
				V = Vector3.zero;
			}
			else
			{
				ForceZero(ref V.x);
				ForceZero(ref V.y);
				ForceZero(ref V.z);
			}
		}
	}

	public static void ForceZero(ref Vector3 V)
	{
		ForceZero(ref V.x);
		ForceZero(ref V.y);
		ForceZero(ref V.z);
	}

	/// <summary>Uses bitwise operations to force a float to be absolute zero.</summary>
	/// <param name="F">Reference to the float that needs to be zero.</param>
	public static unsafe void ForceZero(ref float F)
	{
		// Fix pointer to point to the address float F.
		fixed (float* pF = &F)
		{
			int I = *(int*)&pF; // Lossless conversion of float F bits to int I bits.
			I &= 0x0;           // Bitwise & 0 always equals 0.
			F = *(float*)&I;    // Treat the bits of I as a float and give it back to F.

#if UNITY_EDITOR
			/*
			 * For future reference, this function was made because PitchDelta and YawDelta
			 * was not Zero where it needed to be. These two floats are used in BatMovement.FixedUpdate()
			 * and is needed for aligning the Bat's velocity to where it is facing.
			 * 
			 * There are checks (PitchDelta != 0f || YawDelta != 0f): only if this check passes,
			 * can we align velocities - it also means the Bat is Airborne.
			 * 
			 * Problem is: When these checks pass whilst the Bat is clearly Grounded (IsGrounded() == true)
			 * and not Airborne (IsAirborne() == false) the Bat would not orient itself to where it's going
			 * *on the Ground*. This looked weird, and this function literally forces the two floats to be
			 * exactly Zero... well in theory anyway; if you're reading this, it means it, too, failed.
			 */

			if ((*(int*)(&pF) & 0x7FFFFFFF) > 0x7F800000)
			{
				Debug.LogError("Tell Michael he's dumb! F = NaN");
			}

			if (*(int*)(&pF) == 0x7F800000)
			{
				Debug.LogError("Tell Michael he's dumb! F = Infinity");
			}

			if (*(int*)(&pF) == unchecked((int)0xFF800000))
			{
				Debug.LogError("Tell Michael he's dumb! F = -Infinity");
			}
#endif
		}
	}

	public static void ClampMin(ref float F, float Min)
	{
		if (F < Min)
			F = Min;
	}

	public static void ClampMax(ref float F, float Max)
	{
		if (F > Max)
			F = Max;
	}

	/// <summary>Checks whether <paramref name="V"/> contains <see cref="float.NaN"/>.</summary>
	/// <remarks>Used in Antipede.</remarks>
	/// <returns><see langword="true"/> if at least one vector component is <see cref="float.NaN"/>.</returns>
	public static bool DiagnosticCheckNaN(Vector3 V)
	{
		return DiagnosticCheckNaN(V.x) || DiagnosticCheckNaN(V.y) || DiagnosticCheckNaN(V.z);
	}

	/// <summary>Checks whether <paramref name="F"/> is <see cref="float.NaN"/>.</summary>
	/// <remarks>Used in Antipede.</remarks>
	/// <returns><see langword="true"/> if <paramref name="F"/> is <see cref="float.NaN"/>.</returns
	public static bool DiagnosticCheckNaN(float F)
	{
		return float.IsNaN(F);
	}


	#region Fast Approximation Functions

	/// <summary>1 / sqrt(N).</summary>
	/// <remarks>Modified from: <see href="https://github.com/id-Software/Quake-III-Arena/blob/dbe4ddb10315479fc00086f08e25d968b4b43c49/code/game/q_math.c#L552"/></remarks>
	/// <param name="N">1 / sqrt(x) where x is N.</param>
	/// <param name="AdditionalIterations">The number of additional Newton Iterations to perform.</param>
	/// <returns>An approximation for calculating: 1 / sqrt(N).</returns>
	public static unsafe float FInverseSqrt(float N, int AdditionalIterations = 1)
	{
		int F = *(int*)&N;
		F = 0x5F3759DF - (F >> 1);
		float X = *(float*)&F;

		float RSqrt = X * (1.5f - .5f * N * X * X);
		for (int i = 0; i < AdditionalIterations; ++i)
			RSqrt *= (1.5f - .5f * N * RSqrt * RSqrt);
		return RSqrt;
	}

	/// <summary>Faster version of <see cref="Mathf.Sqrt(float)"/>.</summary>
	/// <param name="F"></param>
	/// <param name="Iterations">The number of Newton Iterations to perform.</param>
	/// <returns>An approximation for the Square Root of F.</returns>
	public static float FSqrt(float F, int Iterations = 1) => FInverseSqrt(Mathf.Max(F, Vector3.kEpsilon), Iterations) * F;

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

	/// <summary>Faster version of <see cref="Mathf.Asin(float)"/>.</summary>
	/// <param name="Angle">The angle to get the inverse Sine of.</param>
	/// <returns>Inverse Sine of Angle.</returns>
	public static float FArcSine(float Angle)
	{
		bool bIsPositive = Angle >= 0f;
		float Abs = Mathf.Abs(Angle);

		float OneMinusFAbs = 1f - Abs;
		ClampMin(ref OneMinusFAbs, 0f);

		float Root = FSqrt(OneMinusFAbs);

		const float kASinHalfPI = 1.5707963050f;

		float Approximation = ((((((-0.0012624911f * Abs + 0.0066700901f) * Abs - 0.0170881256f) * Abs + 0.0308918810f) * Abs - 0.0501743046f) * Abs + 0.0889789874f) * Abs - 0.2145988016f) * Abs + kASinHalfPI;
		Approximation *= Root;

		return bIsPositive ? kASinHalfPI - Approximation : Approximation - kASinHalfPI;
	}

	/// <summary>Faster version of <see cref="Vector3.Angle(Vector3, Vector3)"/>.</summary>
	/// <param name="L">The Vector in which the angular difference is measured.</param>
	/// <param name="R">The Vector in which the angular difference is measured.</param>
	/// <returns>The Angle between L and R in degrees.</returns>
	public static float FAngle(Vector3 L, Vector3 R)
	{
		float ZeroOrEpsilon = FSqrt(L.sqrMagnitude * R.sqrMagnitude);
		if (IsZero(ZeroOrEpsilon))
		{
			return 0f;
		}

		float Radians = Mathf.Clamp(Vector3.Dot(L, R), -1f, 1f);
		return 90f - FArcSine(Radians) * Mathf.Rad2Deg;
	}

	#endregion
}
