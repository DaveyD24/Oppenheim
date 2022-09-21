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

#if USE_MOVE_YAW && USE_LOOK_YAW
#error LOOK YAW TAKES PRECEDENCE! DEFINE USE_MOVE_YAW XOR USE_LOOK_YAW.
#endif

#define USE_DOUBLEJUMP_GLIDE

using System.Collections;
using UnityEngine;
using static global::BatMathematics;
using static UnityEngine.InputSystem.InputAction;

public class BatMovement : MonoBehaviour
{
	private Bat Bat;

	// Airborne settings while this Bat is in the air.
	// Player Controller values will be used for ground movement.

	[Header("Airborne Settings")]
	[SerializeField] float MaxTakeoffAcceleration = 50f;
	[SerializeField] AnimationCurve TakeoffAccelerationCurve;
	[SerializeField] float TimeToV1 = 1f;
	[SerializeField] float JumpHeight = 5f;
	[SerializeField] float SecondsOfPitchFlight = 1f;
	float RemainingSeconds;
	IEnumerator CurrentGradualFunc;

	[SerializeField, Min(kZeroThreshold)] float YawStrength = 2f;
	[SerializeField, Min(kZeroThreshold)] float PitchStrength = 1f;
	float YawDirection = 0f;
	float PitchDirection = 0f;

	[Header("Ground Checks")]
	[SerializeField] Vector3 GroundCheckOffset;
	[SerializeField] float GroundRayDistance = .51f;

	[Header("Cosmetics")]
	[SerializeField] TrailRenderer[] WingtipVortex;

	Vector2 Throw;
	Vector2 ThrowLook;

	// Ground Variables.
	Vector3 GroundMovement;

	// Stop the Player from Gliding more than once per Jump.
	bool bHasGlidedThisJump, bHasCancelledGlideThisJump;
	bool bHasBeenGivenSlightBoost;

	[Space]
	[SerializeField] Camera BatCamera;
	Speedometer Speedometer;

	void Start()
	{
		Bat = GetComponent<Bat>();

		bHasGlidedThisJump = false;
		bHasCancelledGlideThisJump = false;
		bHasBeenGivenSlightBoost = false;

		// BatCamera = GameObject.FindGameObjectWithTag("Bat Camera").GetComponent<Camera>();

		Speedometer.Initialise();
	}

	void FixedUpdate()
	{
		Speedometer.Record(this);

		HandleGroundMovement();
		HandleMovement(Throw);
		HandleLook(ThrowLook);
		DetermineVortex();

		if (!IsZero(YawDirection) || !IsZero(PitchDirection))
		{
			// Translate World-Velocity to Local Forward.
			Vector3 YawVelocity = RotateVector(Bat.Physics.velocity, transform.up, YawDirection);
			Vector3 CombinedVelocity = RotateVector(YawVelocity, -transform.right, PitchDirection);
			Bat.Physics.velocity = CombinedVelocity; // Combination of Pitch and Yaw.

			// Set to Zero if its close enough to Zero.
			Vector3 Velocity = Bat.Physics.velocity;
			SetZeroIfZero(ref Velocity);

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
			if (GroundMovement != Vector3.zero && !IsAirborne())
			{
				// Smoothly rotate the Bat towards where it's moving.
				Vector3 MovementVector = DirectionRelativeToTransform(BatCamera.transform, GroundMovement);
				AlignTransformToMovement(transform, MovementVector, Bat.YawSpeed, Vector3.up);
			}
		}

		SetAnimationState();
		Realign();

		Speedometer.Mark();
	}

	public void MovementBinding(ref CallbackContext Context)
	{
		if (Bat.Active)
		{
			Throw = Context.action.ReadValue<Vector2>();
		}
		else
		{
			Throw = Vector2.zero;
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

			if (!bHasBeenGivenSlightBoost)
			{
				bHasBeenGivenSlightBoost = true;

				Vector3 Forward = transform.forward;
				Forward.y = 0f;
				Forward.Normalize();
				transform.forward = Forward;

				ApplyWingForce(2f, true);

				// When jumping off a ledge, give more Pitch time because otherwise
				// the Bat is uncontrollable and death is guaranteed.
				RemainingSeconds = SecondsOfPitchFlight * 10;
			}

#if !USE_DOUBLEJUMP_GLIDE
			// Forward Gliding.
			if (!bHasGlidedThisJump && Vertical > kGlideInputSensitivity)
			{
				StartGliding();
			}
			else
#endif
			if (!bHasCancelledGlideThisJump && Vertical < -kGlideInputSensitivity)
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
			float Horizontal = Throw.x;

			// Keyboard Yaw.
			ThrowYaw(Horizontal);
#endif

			// Stop Ground Movement from taking place while Airborne.
			// Removed for Ledge/Edge bug. // GroundMovement = Vector3.zero;
		}
		else
		{
			// Grounded Controls.

			GroundMovement = new Vector3(Throw.x, 0f, Throw.y).normalized;
			GroundMovement *= Bat.GroundSpeed;

			// Stop applying gradual forward acceleration.
			StopGradualAcceleration();
			bHasBeenGivenSlightBoost = false;
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

			Bat.Events.OnAnimationStateChanged?.Invoke(EAnimationState.WingedFlight);
		}
#if USE_DOUBLEJUMP_GLIDE
		else if (!IsZero(Throw))
		{
			// Double-Jump mechanism.
			if (!bHasGlidedThisJump)
			{
				// On Double-Jump...
				StartGliding();
			}
			else
			{
				// On 2+ Jump...
				StopGradualAcceleration();
			}
		}
#endif
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
		if (!IsAirborne() || !bHasGlidedThisJump)
		{
			// Ground Movement relative to the camera.
			Vector3 CameraRelativeDirection = DirectionRelativeToTransform(BatCamera.transform, GroundMovement);
			Bat.Physics.MovePosition(Bat.Physics.position + (CameraRelativeDirection * Time.fixedDeltaTime));

			// PitchDirection = YawDirection = 0f;
			// Apparently this wasn't enough - floating point precision failed and 
			// sometimes made these 1E-11. These values NEED to be zero whilst Grounded.
			ForceZero(ref PitchDirection);
			ForceZero(ref YawDirection);
		}
	}

	void StartGliding()
	{
		StopGradualAcceleration();

		CurrentGradualFunc = GradualAcceleration();
		StartCoroutine(CurrentGradualFunc);

		RemainingSeconds = SecondsOfPitchFlight;

		Bat.Audio.Play("Whoosh", EAudioPlayOptions.FollowEmitter | EAudioPlayOptions.DestroyOnEnd);
	}

	IEnumerator GradualAcceleration()
	{
		float rTime = 1f / TimeToV1;
		float t = 0f;

		while (t <= 1f)
		{
			t += Time.fixedDeltaTime * rTime;

			ApplyWingForce(TakeoffAccelerationCurve.Evaluate(t) * MaxTakeoffAcceleration);

			yield return new WaitForFixedUpdate();
		}
	}

	void ApplyWingForce(float Force, bool bOverrides = false)
	{
		// F = ma.
		Bat.Physics.AddForce(Bat.Physics.mass * Force * transform.forward);

		bHasGlidedThisJump = !bOverrides;
	}

	public void StopGradualAcceleration()
	{
		if (CurrentGradualFunc != null)
		{
			StopCoroutine(CurrentGradualFunc);
			CurrentGradualFunc = null;
		}
	}

	/// <summary>Gives Pitch Input.</summary>
	/// <param name="Throw">Direction of Pitch; delta. + Downwards. - Upwards.</param>
	void ThrowPitch(float Throw)
	{
		float PitchThrow = PitchStrength;

		if (RemainingSeconds <= 0f)
		{
			// The Bat is too tired to Pitch upwards.
			// But still allow *some* Pitch input.
			PitchThrow *= .25f;
		}

		// Pitch.
		if (Throw < -.3f)
		{
			PitchDirection = PitchThrow;

			// Deduct time only when Pitching upwards.
			RemainingSeconds -= Time.deltaTime;
		}
		else if (Throw > .3f)
		{
			PitchDirection = -PitchThrow;
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
			/*Bat.Physics.velocity = */
			Bat.Physics.angularVelocity = Vector3.zero;

			// Stop everything else. Switch back to the Stand Idle animation.
			Bat.Events.OnAnimationStateChanged?.Invoke(EAnimationState.StandIdle);
		}
	}

	void DetermineVortex()
	{
		ShowVortices(Speedometer.MetresPerSecond > 10f);
	}

	void ShowVortices(bool bShow)
	{
		foreach (TrailRenderer Vortex in WingtipVortex)
		{
			Vortex.emitting = bShow;
		}
	}

	bool IsAirborne()
	{
		Ray GroundRay = new Ray(transform.position + GroundCheckOffset, Vector3.down);
		return !Physics.Raycast(GroundRay, GroundRayDistance);
	}

#if WITH_GUI_INFO
	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 250, 250), $"Velocity: {Bat.Physics.velocity:F1}");
		GUI.Label(new Rect(10, 25, 250, 250), $"Speed: {Bat.Physics.velocity.magnitude:F1}");
		GUI.Label(new Rect(10, 40, 250, 250), $"Airborne? {(IsAirborne() ? "Yes" : "No")}");
		GUI.Label(new Rect(10, 55, 250, 250), $"Remaining Pitch: {RemainingSeconds}");
	}
#endif
}

public struct Speedometer
{
	public Vector3 Velocity => (ThisFrame - LastFrame) / Time.deltaTime;
	public float MetresPerSecond => Velocity.magnitude;

	Vector3 LastFrame;
	Vector3 ThisFrame;

	public void Initialise()
	{
		LastFrame = ThisFrame = Vector3.zero;
	}

	/// <summary>Begins recording Speed.</summary>
	public void Record(MonoBehaviour Behaviour)
	{
		ThisFrame = Behaviour.transform.position;
	}

	/// <summary>Marks the end of a Speed Recording after <see cref="Time.deltaTime"/>.</summary>
	/// <param name="Behaviour">The Behaviour to mark a Speed relative to.</param>
	public void Mark()
	{
		LastFrame = ThisFrame;
	}
}
