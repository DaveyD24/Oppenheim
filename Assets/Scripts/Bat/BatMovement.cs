
/* --           Pre-Processor Directives           -- */

#if UNITY_EDITOR
// Show diagnostic data on the top left.
#define WITH_GUI_INFO
#endif

// Enables Yaw control with the Horizontal Axis of InputActions.Move.
#define USE_MOVE_YAW
// Enables Yaw control with the Horizontal Axis of InputActions.Look.
// #define USE_LOOK_YAW

using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using static global::BatMathematics;

public class BatMovement : MonoBehaviour
{
	Bat Bat;

	// Airborne settings while this Bat is in the air.
	// Player Controller values will be used for ground movement.

	[Header("Airborne Settings")]
	[SerializeField] float TakeoffAcceleration = 850f;
	[SerializeField] float JumpHeight = 5f;

	[SerializeField] float YawStrength = 2f;
	[SerializeField] float PitchStrength = 1f;
	float YawDirection = 0f;
	float PitchDirection = 0f;

	// Ground Variables.
	Vector3 GroundMovement;

	// Stop the Player from Gliding more than once per Jump.
	bool bHasGlidedThisJump, bHasCancelledGlideThisJump;

	Camera BatCamera;

	void Start()
	{
		Bat = GetComponent<Bat>();

		bHasGlidedThisJump = false;
		bHasCancelledGlideThisJump = false;

		BatCamera = GameObject.FindGameObjectWithTag("Bat Camera").GetComponent<Camera>();
	}

	void Update()
	{
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
				Quaternion RotationNow = transform.rotation;
				Vector3 MovementVector = DirectionRelativeToCamera(BatCamera.transform, GroundMovement);
				Quaternion TargetRot = Quaternion.LookRotation(MovementVector, Vector3.up);
				transform.rotation = Quaternion.RotateTowards(RotationNow, TargetRot, Bat.YawSpeed);
			}
		}
	}

	void FixedUpdate()
	{
		HandleGroundMovement();
	}

	public void MovementBinding(ref CallbackContext Context)
	{
		Vector2 Throw = Context.action.ReadValue<Vector2>();
		HandleMovement(Throw);
	}

	public void JumpBinding(ref CallbackContext Context)
	{
		float Throw = Context.action.ReadValue<float>();
		HandleJump(Throw);
	}

	public void LookBinding(ref CallbackContext Context)
	{
		Vector2 Throw = Context.action.ReadValue<Vector2>();
		HandleLook(Throw);
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
				Bat.Physics.angularVelocity = Vector3.zero;
				YawDirection = 0f;
			}
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
		}
	}

	public void HandleLook(Vector2 Throw)
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
				Bat.Physics.angularVelocity = Vector3.zero;
				PitchDirection = 0f;
			}

#if USE_LOOK_YAW
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
				Bat.Physics.angularVelocity = Vector3.zero;
				YawDirection = 0f;
			}
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
		// Ground Movement relative to the camera.
		Vector3 CameraRelativeDirection = DirectionRelativeToCamera(BatCamera.transform, GroundMovement);
		Bat.Physics.MovePosition(Bat.Physics.position + (CameraRelativeDirection * Time.fixedDeltaTime));
	}

	void StartGliding()
	{
		// F = ma.
		Bat.Physics.AddForce(Bat.Physics.mass * TakeoffAcceleration * transform.forward);

		bHasGlidedThisJump = true;

		LockCursor(true);
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
		Ray GroundRay = new Ray(transform.position, -transform.up);
		return !Physics.Raycast(GroundRay, .51f);
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
