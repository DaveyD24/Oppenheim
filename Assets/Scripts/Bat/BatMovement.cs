#if UNITY_EDITOR
#define WITH_GUI_INFO
#endif
//#define USE_KEYBOARD_YAW
//#define USE_ANY_YAW

using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class BatMovement : PlayerController
{
	[Space(15)]

	// Airborne settings while this Bat is in the air.
	// Player Controller values will be used for ground movement.

	[Header("Airborne Settings")]
	[SerializeField] float TakeoffAcceleration = 850f;
	[SerializeField] float JumpHeight = 5f;

	[SerializeField] float YawStrength = 3f;
	[SerializeField] float PitchStrength = 1f;
	float YawDirection = 0f;
	float PitchDirection = 0f;

	Rigidbody BatPhysics;
	// Stop the Player from Gliding more than once per Jump.
	bool bHasGlidedThisJump, bHasCancelledGlideThisJump;

	protected override void Start()
	{
		base.Start();

		BatPhysics = GetComponent<Rigidbody>();

		bHasGlidedThisJump = false;
		bHasCancelledGlideThisJump = false;

		Inputs.Player.Move.canceled += (CallbackContext Context) =>
		{
			HandleMovement(Vector2.zero);
		};

		Inputs.Player.Jump.canceled += (CallbackContext Context) =>
		{
			HandleJump(0f);
		};

		Inputs.Player.Look.performed += (CallbackContext Context) =>
		{
			Vector2 Throw = Context.action.ReadValue<Vector2>();
			HandleLook(Throw);
		};

		Inputs.Player.Look.canceled += (CallbackContext Context) =>
		{
			HandleLook(Vector2.zero);
			YawDirection = PitchDirection = 0f;
		};
	}

	void Update()
	{
		// Translate World-Velocity to Local Forward.
		if (YawDirection != 0f)
		{
			Vector3 YawVelocity = RotateVector(BatPhysics.velocity, transform.up, YawDirection);
			BatPhysics.velocity = YawVelocity;
		}

		if (PitchDirection != 0f)
		{
			Vector3 PitchVelocity = RotateVector(BatPhysics.velocity, -transform.right, PitchDirection);
			BatPhysics.velocity = PitchVelocity;
		}

		// Stop dead input affecting rotations.
		if (IsZero(YawDirection + PitchDirection))
		{
			BatPhysics.angularVelocity = Vector3.zero;
		}

		// Set to Zero if its close enough to Zero.
		Vector3 Velocity = BatPhysics.velocity;
		ForceZeroIfZero(ref Velocity);

		if (YawDirection != 0f || PitchDirection != 0f)
		{
			BatPhysics.velocity = Velocity;

			// Not Zero and must be facing in the same general direction.
			if (Velocity != Vector3.zero && Vector3.Dot(transform.forward, Velocity) > .5f)
			{
				// Use transform.up or Vector3.up?
				transform.rotation = Quaternion.LookRotation(Velocity, Vector3.up);
			}
		}
	}

	void OnTriggerEnter(Collider Other)
	{
		if (Other.CompareTag(FoodTag))
		{
			Debug.Log("Mango Collected!");
		}
	}

	protected override void Movement(CallbackContext Context)
	{
		Vector2 Throw = Context.action.ReadValue<Vector2>();
		HandleMovement(Throw);
	}

	protected override void Jump(CallbackContext Context)
	{
		float Throw = Context.action.ReadValue<float>();
		HandleJump(Throw);
	}

	protected override void PerformAbility(CallbackContext ctx) { }

	void HandleMovement(Vector2 Throw)
	{
		float Vertical = Throw.y;
		float Horizontal = Throw.x;

		if (IsAirborne())
		{
			// Airborne Motion and Controls.

			// Gliding Sensitivity for Gamepad Compatibility.
			const float kGlideInputSensitivity = .5f;

			// Forward Gliding.
			if (!bHasGlidedThisJump && Vertical > kGlideInputSensitivity)
			{
				// F = ma.
				BatPhysics.AddForce(BatPhysics.mass * TakeoffAcceleration * transform.forward);

				bHasGlidedThisJump = true;
			}
			else if (!bHasCancelledGlideThisJump && Vertical == 0f)
			{
				// If the Player Cancels their Glide, decrease velocity but do not affect Gravity.
				Vector3 Velocity = BatPhysics.velocity;
				Velocity.x *= .5f;
				Velocity.z *= .5f;

				BatPhysics.velocity = Velocity;
				bHasCancelledGlideThisJump = true;
			}
			else if (!bHasCancelledGlideThisJump)
			{
				// Provide Lift while the Player has not cancelled their Glide.
				float Lift = ComputeLift();
				BatPhysics.AddForce(transform.up * Lift);
			}

#if USE_KEYBOARD_YAW || USE_ANY_YAW
			// Keyboard Yaw.
			if (Horizontal < -.3f)
			{
				YawDirection = -YawStrength;
			}
			else if (Horizontal > .3f)
			{
				YawDirection = YawStrength;
			}
			else
			{
				BatPhysics.angularVelocity = Vector3.zero;
				YawDirection = 0f;
			}
#endif
		}
	}

	void HandleJump(float Throw)
	{
		if (!IsAirborne())
		{
			// Grounded.

			bHasGlidedThisJump = false;
			bHasCancelledGlideThisJump = false;

			if (Throw > .01f)
			{
				BatPhysics.velocity += ComputeJumpVelocity();
			}
		}
	}

	void HandleLook(Vector2 Throw)
	{
		Throw.Normalize();
		float Azimuth = Throw.x;
		float Inclination = Throw.y;

		if (IsAirborne())
		{
			// Pitch.
			if (Inclination < -.3f)
			{
				PitchDirection = PitchStrength;
			}
			else if (Inclination > .3f)
			{
				PitchDirection = -PitchStrength;
			}
			else
			{
				BatPhysics.angularVelocity = Vector3.zero;
				PitchDirection = 0f;
			}

#if !USE_KEYBOARD_YAW || USE_ANY_YAW
			// Yaw.
			if (Azimuth < -.3f)
			{
				YawDirection = -YawStrength;
			}
			else if (Azimuth > .3f)
			{
				YawDirection = YawStrength;
			}
			else
			{
				BatPhysics.angularVelocity = Vector3.zero;
				YawDirection = 0f;
			}
#endif
		}
		else
		{
			// Camera Look.
		}
	}

	bool IsAirborne()
	{
		Ray GroundRay = new Ray(transform.position, -transform.up);
		return !Physics.Raycast(GroundRay, .51f);
	}

#if WITH_GUI_INFO
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 250, 250), $"Velocity: {BatPhysics.velocity:F1}");
		GUI.Label(new Rect(10, 25, 250, 250), $"Speed: {BatPhysics.velocity.magnitude:F1}");
		GUI.Label(new Rect(10, 40, 250, 250), $"Airborne? {(IsAirborne() ? "Yes" : "No")}");
	}
#endif

	/// 
	/// MATH FUNCTIONS
	/// 

	Vector3 ComputeJumpVelocity()
	{
		return transform.up * ComputeJumpScalar();
	}

	/// <summary>Calculates the force required to reach JumpHeight.</summary>
	float ComputeJumpScalar()
	{
		/*
		 * V^2 = U^2 + 2AS  (-2AS)
		 * U^2 = V^2 - 2AS  (Rearranged for U)
		 * 
		 * JumpVelocity ^ 2 = FinalVelocity ^ 2 - 2 * Gravity * JumpHeight
		 * JumpVelocity ^ 2 = 0 - 2 * Gravity * JumpHeight
		 * JumpVelocity = Sqrt(-2 * Gravity * JumpHeight)
		 */

		float UU = -2f * Physics.gravity.y * JumpHeight;

		return Mathf.Sqrt(UU);
	}

	// Lift Constants.
	const float kLiftCoefficient = .03f; // Cl (NASA says this value is experimental, but keep within Epsilon and .05)
	const float kWingArea = .577f; // A
	const float kAirDensity = 1f; // ρ (Rho)
	const float kClDAOver2 = kLiftCoefficient * kWingArea * .5f * kAirDensity;

	/// <summary>Calculates the generation of Lift according to a number of variables.</summary>
	/// <returns>The force of Lift in m/s.</returns>
	float ComputeLift()
	{
		// https://www.grc.nasa.gov/WWW/K-12/airplane/lifteq.html
		float Speed = BatPhysics.velocity.magnitude;

		return kClDAOver2 * Speed * Speed;
	}

	/// <summary>Rotates a Vector about an Axis by Angle degrees.</summary>
	Vector3 RotateVector(Vector3 Vector, Vector3 Axis, float Angle)
	{
		// 3D Version of: https://matthew-brett.github.io/teaching/rotation_2d.html

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
	void SinCos(out float Sine, out float Cosine, float Angle)
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

	void Abs(ref Vector3 V)
	{
		V.x = Mathf.Abs(V.x);
		V.y = Mathf.Abs(V.y);
		V.z = Mathf.Abs(V.z);
	}

	/// <summary>True if F is close enough to zero.</summary>
	bool IsZero(float F)
	{
		return F <= .01f;
	}

	/// <summary>True if V is close enough to zero.</summary>
	/// <remarks>'Close enough' is define in <see cref="IsZero(float)"/>.</remarks>
	bool IsZero(Vector3 V)
	{
		Abs(ref V);
		return IsZero(V.x) && IsZero(V.y) && IsZero(V.z);
	}

	/// <summary>Sets V to Vector3.zero if it's close enough to zero.</summary>
	/// <remarks>'Close enough' is define in <see cref="IsZero(float)"/>.</remarks>
	void ForceZeroIfZero(ref Vector3 V)
	{
		// Vector3's == operator is accurate to: 9.99999944 E-11 (0.0000000000999999944)
		// This is too accurate, define our own threshold.
		if (IsZero(V))
			V = Vector3.zero;
	}
}
