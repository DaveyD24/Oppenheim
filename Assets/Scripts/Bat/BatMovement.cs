/* --           Pre-Processor Directives           -- */

#if UNITY_EDITOR
// Show diagnostic data on the top left.
#define WITH_GUI_INFO
#endif

// Enables Yaw control with the Horizontal Axis of InputActions.Move.
// #define USE_MOVE_YAW
// Enables Yaw control with the Horizontal Axis of InputActions.Look.
#define USE_LOOK_YAW

#if !USE_MOVE_YAW && !USE_LOOK_YAW
#error NO YAW INPUT IS DEFINED!
#endif

using UnityEngine;
using static global::BatMathematics;
using static UnityEngine.InputSystem.InputAction;

public class BatMovement : MonoBehaviour
{
	private Bat Bat;
	private Vector2 Throw;
	private Vector2 ThrowLook;
	// Airborne settings while this Bat is in the air.
	// Player Controller values will be used for ground movement.

	[Header("Airborne Settings")]
	[SerializeField] float TakeoffAcceleration = 850f;
	[SerializeField] float JumpHeight = 5f;

	[SerializeField] float SecondsOfPitchFlight = 1f;
	float RemainingSeconds;

	[SerializeField] float YawStrength = 2f;
	[SerializeField] float PitchStrength = 1f;
	float YawDirection = 0f;
	float PitchDirection = 0f;

	[Header("Ground Checks")]
	[SerializeField] Vector3 GroundCheckOffset;
	[SerializeField] float GroundRayDistance = .51f;

	// Ground Variables.
	Vector3 GroundMovement;

	// Stop the Player from Gliding more than once per Jump.
	bool bHasGlidedThisJump, bHasCancelledGlideThisJump;

	[SerializeField] Camera BatCamera;
	Speedometer Speedometer;

	void Start()
	{
		Bat = GetComponent<Bat>();

		bHasGlidedThisJump = false;
		bHasCancelledGlideThisJump = false;

		// BatCamera = GameObject.FindGameObjectWithTag("Bat Camera").GetComponent<Camera>();
		Speedometer = new Speedometer();
		Speedometer.Initialise();
	}

	void Update()
	{
		//if (YawDirection != 0f || PitchDirection != 0f)
		//{
		//	// Translate World-Velocity to Local Forward.
		//	Vector3 YawVelocity = RotateVector(Bat.Physics.velocity, transform.up, YawDirection);
		//	Vector3 CombinedVelocity = RotateVector(YawVelocity, -transform.right, PitchDirection);
		//	Bat.Physics.velocity = CombinedVelocity; // Combination of Pitch and Yaw.

		//	// Set to Zero if its close enough to Zero.
		//	Vector3 Velocity = Bat.Physics.velocity;
		//	ForceZeroIfZero(ref Velocity);

		//	Bat.Physics.velocity = Velocity;

		//	// Not Zero and must be facing in the same general direction.
		//	if (Velocity != Vector3.zero && Vector3.Dot(transform.forward, Velocity) > .5f)
		//	{
		//		// Use transform.up or Vector3.up?
		//		Quaternion RotationNow = transform.rotation;
		//		Quaternion TargetRot = Quaternion.LookRotation(Velocity, Vector3.up);
		//		transform.rotation = Quaternion.RotateTowards(RotationNow, TargetRot, Bat.YawSpeed);
		//	}
		//}
		//else
		//{
		//	if (GroundMovement != Vector3.zero)
		//	{
		//		// Smoothly rotate the Bat towards where it's moving.
		//		Vector3 MovementVector = DirectionRelativeToTransform(BatCamera.transform, GroundMovement);
		//		AlignTransformToMovement(transform, MovementVector, Bat.YawSpeed, Vector3.up);
		//	}
		//}

		//SetAnimationState();
		//Realign();
	}

	void FixedUpdate()
	{
		HandleMovement(Throw);
		HandleLook(ThrowLook);

		Speedometer.Record();

		HandleGroundMovement();

		Speedometer.Mark(this);

		if (YawDirection != 0f || PitchDirection != 0f)
		{
			// Translate World-Velocity to Local Forward.
			Vector3 YawVelocity = RotateVector(Bat.Physics.velocity, transform.up, YawDirection);
			Vector3 CombinedVelocity = RotateVector(YawVelocity, -transform.right, PitchDirection);
			Bat.Physics.velocity = CombinedVelocity; // Combination of Pitch and Yaw.

			// Set to Zero if its close enough to Zero.
			Vector3 Velocity = Bat.Physics.velocity;
			ForceZeroIfZero(ref Velocity);

			Bat.Physics.velocity = Velocity;

			// Not Zero and must be facing in the same general direction.
			if (Velocity != Vector3.zero && Vector3.Dot(transform.forward, Velocity) > .5f)
			{
				// Use transform.up or Vector3.up?
				Quaternion RotationNow = transform.rotation;
				Quaternion TargetRot = Quaternion.LookRotation(Velocity, Vector3.up);
				transform.rotation = Quaternion.RotateTowards(RotationNow, TargetRot, Bat.YawSpeed);
			}
		}
		else
		{
			if (GroundMovement != Vector3.zero)
			{
				// Smoothly rotate the Bat towards where it's moving.
				Vector3 MovementVector = DirectionRelativeToTransform(BatCamera.transform, GroundMovement);
				AlignTransformToMovement(transform, MovementVector, Bat.YawSpeed, Vector3.up);
			}
		}

		SetAnimationState();
		Realign();
	}

	public void MovementBinding(ref CallbackContext Context)
	{
		if (Bat.Active)
		{
			Throw = Context.action.ReadValue<Vector2>();
			// HandleMovement(Throw);
		}
		else
		{
			Throw = Vector2.zero;
			//HandleMovement(Vector2.zero);
		}
	}

	public void JumpBinding(ref CallbackContext Context)
	{
		if (Bat.Active)
		{
			float Throw = Context.action.ReadValue<float>();
			HandleJump(Throw);
		}
		else
		{
			HandleJump(0f);
		}
	}

	public void LookBinding(ref CallbackContext Context)
	{
		if (Bat.Active)
		{
			ThrowLook = Context.action.ReadValue<Vector2>();
		}
		else
		{
			ThrowLook = Vector2.zero;
			// HandleLook(Vector2.zero);
		}
	}

	public void AbilityBinding() { }

	public void HandleMovement(Vector2 Throw)
	{
		if (IsAirborne())
		{
			// Airborne Motion and Controls.

			// Gliding Sensitivity for Gamepad Compatibility.
			const float kGlideInputSensitivity = .5f;

			float Vertical = Throw.y;
			float Horizontal = Throw.x;

			// Forward Gliding.
			if (!bHasGlidedThisJump && Vertical > kGlideInputSensitivity)
			{
				StartGliding();
			}
			else if (!bHasCancelledGlideThisJump && Vertical < -kGlideInputSensitivity)
			{
				// If the Player Cancels their Glide, decrease velocity but do not affect Gravity.
				Vector3 Velocity = Bat.Physics.velocity;
				Velocity.x *= .25f;
				Velocity.z *= .25f;

				Bat.Physics.velocity = Velocity;
				bHasCancelledGlideThisJump = true;
			}
			else if (!bHasCancelledGlideThisJump)
			{
				// Provide Lift while the Player has not cancelled their Glide.
				float Lift = ComputeLift(Bat.Physics);
				Bat.Physics.AddForce(transform.up * Lift);
			}

#if USE_MOVE_YAW
			// Keyboard Yaw.
			ThrowYaw(Horizontal);
#endif

			// Stop Ground Movement from taking place while Airborne.
			GroundMovement = Vector3.zero;
		}
		else
		{
			// Grounded Controls.

			GroundMovement = new Vector3(Throw.x, 0f, Throw.y).normalized;
			GroundMovement *= Bat.GroundSpeed;

			LockCursor(false);
		}
	}

	public void HandleJump(float Throw)
	{
		if (!IsAirborne())
		{
			// Grounded.

			YawDirection = PitchDirection = 0f;

			bHasGlidedThisJump = false;
			bHasCancelledGlideThisJump = false;

			if (Throw > .01f)
			{
				Bat.Physics.velocity += ComputeJumpVelocity(transform.up, JumpHeight);
			}

			// Fact - Every time the Bat jumps, it has to take off from the Ground.
			RemainingSeconds = SecondsOfPitchFlight;
		}
	}

	public void HandleLook(Vector2 Throw)
	{
		Throw.Normalize();
		float Azimuth = Throw.x;
		float Inclination = Throw.y;

		if (IsAirborne())
		{
			ThrowPitch(Inclination);

#if USE_LOOK_YAW
			// Look Yaw.
			ThrowYaw(Azimuth);
#endif
		}
		else
		{
			// Camera Look.

			/*
			 * Should be handled with a Spring Arm or similar component.
			 */
		}
	}

	void HandleGroundMovement()
	{
		if (!IsAirborne())
		{
			// Ground Movement relative to the camera.
			Vector3 CameraRelativeDirection = DirectionRelativeToTransform(BatCamera.transform, GroundMovement);
			Bat.Physics.MovePosition(Bat.Physics.position + (CameraRelativeDirection * Time.fixedDeltaTime));
		}
	}

	void StartGliding()
	{
		// F = ma.
		Bat.Physics.AddForce(Bat.Physics.mass * TakeoffAcceleration * transform.forward);

		bHasGlidedThisJump = true;

		LockCursor(true);
	}

	/// <summary>Gives Pitch Input.</summary>
	/// <param name="Throw">Direction of Pitch; delta. + Downwards. - Upwards.</param>
	void ThrowPitch(float Throw)
	{
		// The Bat is too tired to Pitch upwards.
		if (RemainingSeconds <= 0f)
		{
			PitchDirection = 0f;
			return;
		}

		// Pitch.
		if (Throw < -.3f)
		{
			PitchDirection = PitchStrength;

			// Deduct time only when Pitching upwards.
			RemainingSeconds -= Time.deltaTime;
		}
		else if (Throw > .3f)
		{
			PitchDirection = -PitchStrength;
		}
		else
		{
			Bat.Physics.angularVelocity = Vector3.zero;
			PitchDirection = 0f;
		}
	}

	/// <summary>Gives Yaw Input.</summary>
	/// <param name="Throw">Direction of Yaw; delta. + Right. - Left.</param>
	void ThrowYaw(float Throw)
	{
		// Yaw.
		if (Throw < -.3f)
		{
			YawDirection = -YawStrength;
		}
		else if (Throw > .3f)
		{
			YawDirection = YawStrength;
		}
		else
		{
			Bat.Physics.angularVelocity = Vector3.zero;
			YawDirection = 0f;
		}
	}

	void SetAnimationState()
	{
		if (!IsAirborne())
		{
			// If we're not moving, we're in the Stand Idle State.
			Bat.Events.OnAnimationStateChanged?.Invoke(!IsZero(Speedometer.Velocity)
				? EAnimationState.Walking
				: EAnimationState.StandIdle);
		}
		else
		{
			// If the Bat's Animation is Walking, change to WingedFlight.
			// Otherwise, the Bat should already be in the Air; change to Gliding.
			Bat.Events.OnAnimationStateChanged?.Invoke(Bat.Events.GetCurrentAnimState() < EAnimationState.WingedFlight
				? EAnimationState.WingedFlight
				: EAnimationState.Gliding
			);
		}
	}

	void Realign()
	{
		const float kUpSensitivity = .4f;
		float Dot = Vector3.Dot(transform.up, Vector3.up);

		// If the Bat is not 'upright' and the Bat is on the Ground.
		if ((Dot <= kUpSensitivity) && Bat.IsGrounded())
		{
			transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;

			// Stop moving.
			Bat.Physics.velocity = Bat.Physics.angularVelocity = Vector3.zero;
		}
	}

	static void LockCursor(bool bShouldLock)
	{
		if (bShouldLock)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = bShouldLock;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	bool IsAirborne()
	{
		Ray GroundRay = new Ray(transform.position + GroundCheckOffset, -transform.up);
		return !Physics.Raycast(GroundRay, GroundRayDistance);
	}

#if WITH_GUI_INFO
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 250, 250), $"Velocity: {Bat.Physics.velocity:F1}");
		GUI.Label(new Rect(10, 25, 250, 250), $"Speed: {Bat.Physics.velocity.magnitude:F1}");
		GUI.Label(new Rect(10, 40, 250, 250), $"Airborne? {(IsAirborne() ? "Yes" : "No")}");
	}
#endif
}

public struct Speedometer
{
	public float MetresPerSecond => (ThisFrame - LastFrame).magnitude / Time.deltaTime;
	public Vector3 Velocity => (ThisFrame - LastFrame) / Time.deltaTime;

	Vector3 LastFrame;
	Vector3 ThisFrame;

	public void Initialise()
	{
		LastFrame = ThisFrame = Vector3.zero;
	}

	/// <summary>Begins recording Speed.</summary>
	public void Record()
	{
		LastFrame = ThisFrame;
	}

	/// <summary>Marks the end of a Speed Recording after <see cref="Time.deltaTime"/>.</summary>
	/// <param name="Behaviour">The Behaviour to mark a Speed relative to.</param>
	public void Mark(MonoBehaviour Behaviour)
	{
		ThisFrame = Behaviour.transform.position;
	}
}
