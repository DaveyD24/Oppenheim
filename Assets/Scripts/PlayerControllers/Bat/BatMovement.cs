/* --           Pre-Processor Directives           -- */

// Show diagnostic data on the bottom left.
#define WITH_GUI_INFO

// Enables Yaw control with the Horizontal Axis of InputActions.Move.
#define MOVE_AIRBORNE
// Enables Yaw control with the Horizontal Axis of InputActions.Look.
//#define LOOK_AIRBORNE

#if !MOVE_AIRBORNE && !LOOK_AIRBORNE
#error No Airborne Input is defined!
#endif

#if MOVE_AIRBORNE && LOOK_AIRBORNE
#error LOOK_AIRBORNE takes precedence over MOVE_AIRBORNE! Define MOVE_AIRBORNE ^ LOOK_AIRBORNE.
#endif

// Enables Gliding upon double jump.
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
	[SerializeField] Vector2 MinMaxPitchAngle;
	float RemainingSeconds;
	IEnumerator CurrentGradualFunc;

	[SerializeField, Min(kZeroThreshold)] float YawDeltaAngle = 2f;
	[SerializeField, Min(kZeroThreshold)] float PitchDeltaAngle = 2f;
	float YawDelta = 0f;
	float PitchDelta = 0f;

	[Header("Ground Checks")]
	[SerializeField] Vector3 GroundCheckOffset;
	[SerializeField] float GroundRayDistance = .51f;

	[Header("Cosmetics")]
	[SerializeField] TrailRenderer[] WingtipVortex;

	Vector2 ThrowMove;
	Vector2 ThrowLook;

	// Ground Variables.
	Vector3 GroundMovement;

	// Stop the Player from Gliding more than once per Jump.
	bool bHasGlidedThisJump, bHasCancelledGlideThisJump;
	bool bHasBeenGivenSlightBoost;
	bool bHasDoubleJumped;

	Speedometer Speedometer;
	float TimeNotMovingWhilstAirborne = 0f;

	const float kAlignByGravityThreshold = 12f;

	void Start()
	{
		Bat = GetComponent<Bat>();

		bHasGlidedThisJump = false;
		bHasCancelledGlideThisJump = false;
		bHasBeenGivenSlightBoost = false;

		// BatCamera = GameObject.FindGameObjectWithTag("Bat Camera").GetComponent<Camera>();

		Speedometer.Initialise();
	}

#if UNITY_EDITOR
	void Update()
	{
		// Simulates falling flat on the Bat's face.
		if (Input.GetKeyDown(KeyCode.RightBracket))
		{
			transform.position = new Vector3(164.2399f, 9.04353f, -479.6122f);
			transform.localEulerAngles = new Vector3(64.365f, 0f, 0f);
		}
	}
#endif

	void FixedUpdate()
	{
		if (!Bat.TrackingCamera)
		{
			return;
		}

		Speedometer.Record(this);

		// If the Bat isn't really moving, but is still Airborne, something's wrong.
		if (IsZero(Speedometer.Velocity, .5f) && IsAirborne())
		{
			TimeNotMovingWhilstAirborne += Time.deltaTime;

			// After 1 second of not moving whilst IsAirborne():
			if (TimeNotMovingWhilstAirborne >= 1f)
			{
				// Flip the Bat upright, and enable Jumping.
				Bat.Physics.velocity = ComputeJumpVelocity(Vector3.up, 1f);
				bHasGlidedThisJump = false;
				bHasDoubleJumped = false;

				ForceRealign();

				TimeNotMovingWhilstAirborne = 0f;
			}
		}
		else
		{
			TimeNotMovingWhilstAirborne = 0f;
		}

		HandleGroundMovement();
		HandleMovement(ThrowMove);
		HandleLook(ThrowLook);
		DetermineVortex();

		// If the Bat is moving faster than the Gravity Threshold, rotate the Bat downwards during a free fall.
		bool bIsAffectedGravityPastThreshold = Speedometer.MetresPerSecond > kAlignByGravityThreshold;
		if (!IsZero(YawDelta) || !IsZero(PitchDelta) || bIsAffectedGravityPastThreshold)
		{
			// Translate World-Velocity to Local Forward.
			Vector3 YawVelocity = RotateVector(Bat.Physics.velocity, transform.up, YawDelta);
			Vector3 CombinedVelocity = RotateVector(YawVelocity, -transform.right, PitchDelta);
			Bat.Physics.velocity = CombinedVelocity; // Combination of Pitch and Yaw.

			// Set to Zero if its close enough to Zero.
			Vector3 Velocity = Bat.Physics.velocity;
			SetZeroIfZero(ref Velocity);

			Bat.Physics.velocity = Velocity;

			// Not Zero and must be facing in the same general direction.
			if (Velocity != Vector3.zero)
			{
				// If the Bat has fallen off an edge without Glide input.
				if (bHasBeenGivenSlightBoost && !bHasGlidedThisJump && !bIsAffectedGravityPastThreshold)
				{
					// Ignore and do not face towards the effect of gravity.
					Velocity.y = 0f;
				}

				// Use transform.up or Vector3.up?
				Quaternion RotationNow = transform.rotation;
				Quaternion TargetRot = Quaternion.LookRotation(Velocity, Vector3.up);
				transform.rotation = Quaternion.RotateTowards(RotationNow, TargetRot, Bat.YawSpeed);
			}
		}
		else if (GroundMovement != Vector3.zero && !IsAirborne())
		{
			// Smoothly rotate the Bat towards where it's moving.
			Vector3 MovementVector = DirectionRelativeToTransform(Bat.TrackingCamera.transform, GroundMovement);
			AlignTransformToMovement(transform, MovementVector, Bat.YawSpeed, Vector3.up);
		}

		SetAnimationState();
		Realign();

		Speedometer.Mark();
	}

	void OnCollisionEnter(Collision Collision)
	{
		if (bHasGlidedThisJump && IsAirborne())
		{
			if (Collision.relativeVelocity.magnitude > 10f)
			{
				ForceStopAllMovement();
			}
		}
	}

	public void MovementBinding(ref CallbackContext Context)
	{
		if (Bat.Active)
		{
			ThrowMove = Context.action.ReadValue<Vector2>();
		}
		else
		{
			ThrowMove = Vector2.zero;
		}
	}

	public void JumpBinding(ref CallbackContext Context)
	{
		if (Bat.Active)
		{
			HandleJump(Context.action.ReadValue<float>());
		}
		else
		{
			HandleJump(0f);
		}
	}

	public void LookBinding(ref CallbackContext Context)
	{
		/**
		--      This does nothing if we're not using LOOK_AIRBORNE.     --
		--               Can always be changed, if needed.              --
		**/
#if LOOK_AIRBORNE
		if (Bat.Active)
		{
			ThrowLook = Context.action.ReadValue<Vector2>();
		}
		else
		{
			ThrowLook = Vector2.zero;
		}
#endif
	}

	public void AbilityBinding() { }

	public void HandleMovement(Vector2 Throw)
	{
		if (IsAirborne())
		{
			// Airborne Motion and Controls.

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

#if !MOVE_AIRBORNE
			// Gliding Sensitivity for Gamepad Compatibility.
			const float kGlideInputSensitivity = .5f;
#if !USE_DOUBLEJUMP_GLIDE

			// Forward Gliding.
			if (!bHasGlidedThisJump && Vertical > kGlideInputSensitivity)
			{
				StartGliding();
			}
			else
#endif // !USE_DOUBLEJUMP_GLIDE
			if (!bHasCancelledGlideThisJump && Vertical < -kGlideInputSensitivity)
			{
				ApplyAirbrakes();
			}
			else
#endif // !MOVE_AIRBORNE
			// Generate Lift.
			if (!bHasCancelledGlideThisJump)
			{
				// Provide Lift while the Player has not cancelled their Glide.
				float Lift = ComputeLift(Bat.Physics);
				Bat.Physics.AddForce(transform.up * Lift);
			}

#if MOVE_AIRBORNE
			float Horizontal = Throw.x;

			if (bHasGlidedThisJump || bHasBeenGivenSlightBoost)
			{
				// Move Pitch.
				ThrowPitch(Vertical);

				// Move Yaw.
				ThrowYaw(Horizontal);
			}

#endif // MOVE_AIRBORNE

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

			Bat.Physics.angularVelocity = Vector3.zero;
		}
	}

	public void HandleJump(float Throw)
	{
		if (!IsAirborne())
		{
			// Grounded.

			YawDelta = PitchDelta = 0f;

			bHasGlidedThisJump = false;
			bHasCancelledGlideThisJump = false;

			if (Throw > .01f)
			{
				Bat.Physics.velocity += ComputeJumpVelocity(Vector3.up, JumpHeight);
			}

			// Fact - Every time the Bat jumps, it has to take off from the Ground.
			RemainingSeconds = SecondsOfPitchFlight;

			Bat.Events.OnAnimationStateChanged?.Invoke(EAnimationState.WingedFlight);

			bHasDoubleJumped = false;
		}
#if USE_DOUBLEJUMP_GLIDE
		else if (!IsZero(Throw))
		{
			// Double-Jump mechanism.
			if (!bHasGlidedThisJump)
			{
				// On Double-Jump...
				StartGliding();

				bHasDoubleJumped = true;
			}
			else
			{
				// On 2+ Jump...
				StopGradualAcceleration();
#if MOVE_AIRBORNE
				ApplyAirbrakes();
#endif
			}
		}
#endif
	}

	public void HandleLook(Vector2 Throw)
	{
		/**
		--      This does nothing if we're not using LOOK_AIRBORNE.     --
		--               Can always be changed, if needed.              --
		**/

#if LOOK_AIRBORNE
		Throw.Normalize();
		float Azimuth = Throw.x;
		float Inclination = Throw.y;

		if (IsAirborne())
		{
			// Look Pitch.
			ThrowPitch(Inclination);

			// Look Yaw.
			ThrowYaw(Azimuth);
		}
		else
		{
			// Camera Look.

			/*
			 * Should be handled with a Spring Arm or similar component.
			 */
		}
#endif
	}

	private void HandleGroundMovement()
	{
		if (!IsAirborne() || !bHasGlidedThisJump)
		{
			// Ground Movement relative to the camera.
			Vector3 cameraRelativeDirection = DirectionRelativeToTransform(Bat.TrackingCamera.transform, GroundMovement);
			Bat.Physics.MovePosition(Bat.Physics.position + (cameraRelativeDirection * Time.fixedDeltaTime));

			// PitchDelta = YawDelta = 0f;
			// Apparently this wasn't enough - floating point precision failed and 
			// sometimes made these 1E-11. These values NEED to be zero whilst Grounded.
			ForceZero(ref PitchDelta);
			ForceZero(ref YawDelta);
		}
	}

	private void StartGliding()
	{
		StopGradualAcceleration();

		CurrentGradualFunc = GradualAcceleration();
		StartCoroutine(CurrentGradualFunc);

		RemainingSeconds = SecondsOfPitchFlight;

		Bat.Audio.Play("Whoosh", EAudioPlayOptions.FollowEmitter | EAudioPlayOptions.DestroyOnEnd);
	}

	private IEnumerator GradualAcceleration()
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

	private void ApplyWingForce(float force, bool bOverrides = false)
	{
		// F = ma.
		Bat.Physics.AddForce(Bat.Physics.mass * force * transform.forward);

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

	private void ApplyAirbrakes()
	{
		// If the Player Cancels their Glide, decrease velocity but do not affect Gravity.
		Vector3 velocity = Bat.Physics.velocity;
		velocity.x *= .25f;
		velocity.z *= .25f;

		Bat.Physics.velocity = velocity;
		bHasCancelledGlideThisJump = true;
	}

	/// <summary>Gives Pitch Input.</summary>
	/// <param name="Throw">Direction of Pitch; delta. + Downwards. - Upwards.</param>
	private void ThrowPitch(float Throw)
	{
#if MOVE_AIRBORNE && USE_DOUBLEJUMP_GLIDE
		if (!bHasDoubleJumped)
		{
			return;
		}
#else
		if (!bHasGlidedThisJump)
		{
			return;
		}
#endif

		if (!IsFacingVelocity())
		{
			StopPitchInput();
			return;
		}

		float PitchAngle = transform.rotation.eulerAngles.x;
		float AngleNoWinding = PitchAngle > 180f ? PitchAngle - 360 : PitchAngle;

		float Incline = Mathf.Sign(Throw);

		// Upwards Pitch is negative, but positive in the inspector.
		// Downwards Pitch is positive, but negative in the inspector.
		// It just makes more sense to have the lower Pitch Angle limit
		// to be relative to transform.forward. (- is down. + is up).
		float Min = MinMaxPitchAngle[0] * -1f;
		float Max = -MinMaxPitchAngle[1];

		if (Incline < 0f && AngleNoWinding < Max)
		{
			StopPitchInput();
			return;
		}
		else if (Incline > 0f && AngleNoWinding > Min)
		{
			StopPitchInput();
			return;
		}

		float PitchThrow = PitchDeltaAngle;

		if (RemainingSeconds <= 0f)
		{
			// The Bat is too tired to Pitch upwards.
			// But still allow *some* Pitch input.
			PitchThrow *= .25f;
		}

		// Pitch Upwards.
		if (Throw < -.3f)
		{
			PitchDelta = PitchThrow;

			// Deduct time only when Pitching upwards.
			RemainingSeconds -= Time.deltaTime;
		}
		// Pitch Downwards.
		else if (Throw > .3f)
		{
			PitchDelta = -PitchThrow;
		}
		else
		{
			StopPitchInput();
		}

		void StopPitchInput()
		{
			Bat.Physics.angularVelocity = Vector3.zero;
			PitchDelta = 0f;
		}
	}

	/// <summary>Gives Yaw Input.</summary>
	/// <param name="Throw">Direction of Yaw; delta. + Right. - Left.</param>
	private void ThrowYaw(float Throw)
	{
#if MOVE_AIRBORNE && USE_DOUBLEJUMP_GLIDE
		if (!bHasDoubleJumped)
		{
			return;
		}
#else
		if (!bHasGlidedThisJump)
		{
			return;
		}
#endif

		if (!IsFacingVelocity())
		{
			Bat.Physics.angularVelocity = Vector3.zero;
			YawDelta = 0f;
			return;
		}

		// Yaw.
		if (Throw < -.3f)
		{
			YawDelta = -YawDeltaAngle;
		}
		else if (Throw > .3f)
		{
			YawDelta = YawDeltaAngle;
		}
		else
		{
			Bat.Physics.angularVelocity = Vector3.zero;
			YawDelta = 0f;
		}
	}

	private void SetAnimationState()
	{
		if (!IsAirborne())
		{
			// If we're not moving, we're in the Stand Idle State.
			Bat.Events.OnAnimationStateChanged?.Invoke(!IsZero(Speedometer.Velocity, .5f)
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
		if ((Dot <= kUpSensitivity) && !IsAirborne())
		{
			ForceRealign();
		}
	}

	void ForceRealign()
	{
		transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;

		// Stop moving.
		/*Bat.Physics.velocity = */
		Bat.Physics.angularVelocity = Vector3.zero;

		// Stop everything else. Switch back to the Stand Idle animation.
		Bat.Events.OnAnimationStateChanged?.Invoke(EAnimationState.StandIdle);
	}

	/// <summary>Forces all movement components to Zero.</summary>
	public void ForceStopAllMovement()
	{
		ForceZero(ref PitchDelta);
		ForceZero(ref YawDelta);
		ForceZero(ref GroundMovement);
		ForceZero(ref ThrowMove.x);
		ForceZero(ref ThrowMove.y);
		ForceZero(ref ThrowLook.x);
		ForceZero(ref ThrowLook.y);

		StopGradualAcceleration();
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

	bool IsFacingVelocity()
	{
		Vector3 Direction = Speedometer.DirectionOfTravel;
		Vector3 Forward = transform.forward;

		return Vector3.Dot(Forward, Direction) > .1f;
	}

#if WITH_GUI_INFO
	void OnGUI()
	{
		if (Input.GetKey(KeyCode.Backslash))
		{
			int sh = Screen.height - 50;
			GUI.Label(new Rect(10, sh - 10, 250, 250), $"Velocity: {Bat.Physics.velocity:F1}");
			GUI.Label(new Rect(10, sh - 25, 250, 250), $"Speed: {Bat.Physics.velocity.magnitude:F1}");
			GUI.Label(new Rect(10, sh - 40, 250, 250), $"Airborne? {(IsAirborne() ? "Yes" : "No")}");
			GUI.Label(new Rect(10, sh - 55, 250, 250), $"Remaining Pitch: {RemainingSeconds}");
			GUI.Label(new Rect(10, sh - 70, 250, 250), $"Slight Boost? {bHasBeenGivenSlightBoost}");
		}
	}
#endif
}

public struct Speedometer
{
	public Vector3 Velocity => (ThisFrame - LastFrame) / Time.deltaTime;
	public float MetresPerSecond => Velocity.magnitude;
	public Vector3 DirectionOfTravel => Velocity.normalized;

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
