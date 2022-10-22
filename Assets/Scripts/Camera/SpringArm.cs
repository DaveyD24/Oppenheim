using UnityEngine;
using static UnityEngine.Extensions.XVector;
using static global::BatMathematics;
using static global::MDebug;
using EventSystem;

public class SpringArm : MonoBehaviour
{

#if UNITY_EDITOR
	[Header("Debug Options. [EDITOR ONLY]")]
	[SerializeField] bool bDrawSpringArmLine;
	[SerializeField] bool bDrawRotationalLines;
	[SerializeField] bool bDrawAdvancedCollisionLines;
	[Space(10)]
#endif

	[Header("Target Settings.")]
	[SerializeField] Transform Camera;
	public Transform Target;
	[SerializeField] Vector3 TargetOffset;

	[HideInInspector] public Camera CameraComponent;

	/// <summary>Is this SpringArm tracking an Average Position?</summary>
	[Header("ViewportSplit Controls [Read Only]")]
	[ReadOnly] public bool bIsAverageTracking;
	/// <summary>Is this Spring Arm temporary and should not initialise itself?</summary>
	[ReadOnly] public bool bIsSecondarySpringArm;

	[Header("Spring Arm Settings.")]
	public float Distance;
	/// <summary>Rotation relative to Target.</summary>
	[SerializeField] Vector3 GimbalRotation;
	/// <summary>Rotation of the View Camera.</summary>
	[SerializeField] Vector3 CameraRotation;
	public bool bInheritRotation;
	[SerializeField] bool bInheritPitch;
	[HideInInspector, SerializeField] Vector3 DefaultGimbalRotation;
	[HideInInspector, SerializeField] Vector3 DefaultCameraRotation;

	[Header("Scroll Settings")]
	[SerializeField] bool bEnableScrollToDistance;
	[SerializeField] float ScrollSensitivity;
	[SerializeField] float MaxDistance = 30;

	[Header("Orbit Settings")]
	[SerializeField] float OrbitSensitivity = 1f;
	[SerializeField] Vector2 MinMaxOrbitAngle = new Vector2(1f, 70f);

	[Header("Pan Settings")]
	[SerializeField] bool bPermanentlyChangePanTargetOffset;
	[SerializeField] float MaxPanDistance = 5f;

	Vector2 PreviousMouseDragPosition;
	Vector3 GimbalRotationInherited;
	Vector3 CameraRotationInherited;
	Vector2 PreviousMousePanPosition;
	Vector3 OriginalTargetOffset;
	float PitchDegrees;

	[Header("Inverse Settings.")]
	[SerializeField] bool bInvertX; // Inverse LR dragging Orbit Controls.
	[SerializeField] bool bInvertY; // Inverse UD dragging Orbit Controls.
	[SerializeField] bool bInvertZ; // Inverse Zoom Controls.

	[Header("Collisions")]
	[SerializeField] bool bRunCollisionChecks;
	[SerializeField] bool bUseAdvancedCollisionBehaviour;
	[SerializeField] bool bForceAdvancedBehaviour;
	[SerializeField] float AdvancedCollisionActivationDistance;
	[SerializeField] float MaxAdvancedCollisionAngle;
	[SerializeField] LayerMask OnlyCollideWith;
	bool bIsAdvancedBehaviourEffective;
	Vector3 AdvancedForwardVector;
	Vector3 AdvancedRightVector;
	bool bHasSetAdvancedVectors;

	[Header("Lag Settings")]
	[SerializeField] float PositionalLagStrength = .2f;
	[SerializeField] float RotationalLagStrength = .2f;
	Vector3 TargetPosition;
	Quaternion TargetRotation;

	[Header("Projection Settings.")]
	[SerializeField] bool bUseCustomProjection;
	[SerializeField] Transform Plane;
	[SerializeField] float NearClipDistance;
	[SerializeField] float DistanceLimit;
	Matrix4x4 DefaultProjection;

	bool bNoClip = false;

	void Start()
	{
		// Secondary Spring Arms are initialised in ViewportSplit::SetSecondaryTarget().
		if (!bIsSecondarySpringArm)
		{
			DefaultGimbalRotation = GimbalRotation;
			DefaultCameraRotation = CameraRotation;

			GimbalRotationInherited = DefaultGimbalRotation;
			CameraRotationInherited = DefaultCameraRotation;

			OriginalTargetOffset = TargetOffset;

			CameraComponent = GetComponent<Camera>();
			DefaultProjection = CameraComponent.projectionMatrix;

			ViewportSplit.SetMainSpringArm(this);
		}
	}

	private void Update()
	{
		// UpdateRotationOnMouse();
		PanCameraOnMouse();

		//ScrollDistance();

		if (bNoClip)
		{
			const float kNoClipSpeed = 25f;

			if (Input.GetKey(KeyCode.W))
			{
				Camera.position += kNoClipSpeed * Time.deltaTime * Camera.forward;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				Camera.position -= kNoClipSpeed * Time.deltaTime * Camera.forward;
			}

			if (Input.GetKey(KeyCode.D))
			{
				Camera.position += kNoClipSpeed * Time.deltaTime * Camera.right;
			}
			else if (Input.GetKey(KeyCode.A))
			{
				Camera.position -= kNoClipSpeed * Time.deltaTime * Camera.right;
			}

			if (Input.GetKey(KeyCode.Q))
			{
				Camera.position -= kNoClipSpeed * Time.deltaTime * Vector3.up;
			}
			else if (Input.GetKey(KeyCode.E))
			{
				Camera.position += kNoClipSpeed * Time.deltaTime * Vector3.up;
			}
		}
	}

	private void InheritRotation(Transform playerTransform)
	{
		// only do if control is from the player the camera is tracking or camera tracking all players through average tracking
		if (Target == playerTransform || bIsAverageTracking)
		{
			bInheritRotation = !bInheritRotation;
		}
	}

	private void OnEnable()
	{
		GameEvents.OnCameraMove += UpdateRotationOnMouse;
		GameEvents.OnCameraZoom += ScrollDistance;
		GameEvents.OnCameraFollowRotation += InheritRotation;
	}

	private void OnDisable()
	{
		GameEvents.OnCameraMove -= UpdateRotationOnMouse;
		GameEvents.OnCameraZoom -= ScrollDistance;
		GameEvents.OnCameraFollowRotation += InheritRotation;
	}

	Vector3 SmoothPositionVelocity;

	void FixedUpdate()
	{
		if (bNoClip)
			return;

		Camera.SetPositionAndRotation(
			Vector3.SmoothDamp(Camera.position, TargetPosition, ref SmoothPositionVelocity, PositionalLagStrength),
			Quaternion.Slerp(Camera.rotation, TargetRotation, RotationalLagStrength)
		);

		PlaceCamera();
	}

#if false
	void OnPreCull()
	{
		//ComputeProjection();
	}
#endif

	void PlaceCamera()
	{
		// Where the Spring Arm will point towards.
		Vector3 ArmDirection = Vector3.one;
		Vector3 FinalPosition;
		Quaternion FinalRotation = Quaternion.Euler(CameraRotation);

		if (!bInheritRotation || bIsAverageTracking)
		{
			float VerticalOrbit = GimbalRotation.x;
			float HorizontalOrbit = -GimbalRotation.y;

			VerticalOrbit *= Mathf.Deg2Rad;
			HorizontalOrbit *= Mathf.Deg2Rad;

			// Convert Angles to Vectors.
			Vector3 Ground = new Vector3(Mathf.Sin(VerticalOrbit), 0, Mathf.Cos(VerticalOrbit)); // XZ.
			Vector3 Up = new Vector3(0, Mathf.Sin(HorizontalOrbit), Mathf.Cos(HorizontalOrbit)); // XYZ.

			// Ground's XZ and Up's Y will be used to define the direction of the Spring Arm.
			ArmDirection = new Vector3(Ground.x, Up.y, Ground.z).normalized;
#if UNITY_EDITOR
			if (bDrawRotationalLines)
			{
				Debug.DrawLine(Target.position, Target.position + -Ground * Distance, Color.red);
				Debug.DrawLine(Target.position, Target.position + -Up * Distance, Color.green);
				Debug.DrawLine(Target.position, Target.position + -ArmDirection * Distance, Color.yellow);
			}
#endif
		}
		else
		{
			if (bInheritPitch && Physics.Raycast(Target.position + Vector3.up, Vector3.down, out RaycastHit Ground, 5f, OnlyCollideWith))
			{
				PitchDegrees = 90f-V2PYR(Ground.normal).x;
			}
			else
			{
				PitchDegrees = 0f;
			}

			// Rotates the Spring Arm around to face the Target's forward vector.
			// Ignores the Target's Y-Axis, replacing it with the Yaw rotation,
			// relative to the Target, after inheritance.
			ArmDirection = Target.forward;
			ArmDirection.y = Mathf.Sin((-GimbalRotationInherited.y - PitchDegrees) * Mathf.Deg2Rad);
			ArmDirection.Normalize();

			FinalRotation = GetInheritedRotation();
		}

		// If the Spring Arm will collider with something:
		if (bRunCollisionChecks && RunCollisionsCheck(ref ArmDirection))
			return;

		// Make the Position and Rotation for Lag.
		FinalPosition = TargetPos() - (Distance * ArmDirection);

		SetPositionAndRotation(FinalPosition, FinalRotation);
	}

	bool RunCollisionsCheck(ref Vector3 Direction)
	{
		if (bUseCustomProjection)
			return false;

		Vector3 TP = TargetPos();
		Ray FOV = new Ray(TP, -Direction);
		bool bViewToTargetBlocked = Physics.Raycast(FOV, out RaycastHit Hit, Distance, OnlyCollideWith);

		Quaternion Rotation = !bInheritRotation || bIsAverageTracking
					? Quaternion.Euler(CameraRotation)
					: GetInheritedRotation();

		if (bViewToTargetBlocked)
		{
			// Run Collision Checks NORMALLY if:
			// The Ground Normal Pitch Degrees is greater than 10 degrees. OR                                  // If greater than 10 degrees, use Advanced.
			// We're not Forcing Advanced Collision Behaviour AND EITHER ANY OF:                               // If Forcing Advanced Collision, use Advanced.
			// We're not even using the Advanced Collision Behaviour OR                                        // If we are using Advanced Collision, continue evaluating...
			// We're not Inheriting Rotation OR                                                                // If we are using Advanced Collision, we *must* also be Inheriting Rotation, continue evaluating if so...
			// The distance between the wall and the Target is greater than the Advanced Distance Threshold.   // If the wall is 'close enough' to the Target, finally use Advanced Collision.
			if (!IsZero(PitchDegrees, 10f) || (!bForceAdvancedBehaviour && !bUseAdvancedCollisionBehaviour ||
				!bInheritRotation || Hit.distance > AdvancedCollisionActivationDistance))
			{
				Vector3 Point = Hit.point - FOV.direction;
				SetPositionAndRotation(Point, Rotation);
			}
			// Fail-safe checks. Here we sort of redefine what Force Advanced Behaviour means...
			// If the above fails, but we're still forcing the use of Advanced, we can still do it regardless if we're Inheriting Rotations.
			// Otherwise, if we're not using Force, the Advanced Behaviour must use Inherit Rotation.
			else if (bForceAdvancedBehaviour || bInheritRotation)
			{
				RunAdvancedCollision(TP, Hit, Rotation, -Direction);
			}
			// Otherwise, skip the frame.
		}

		// bIsAdvancedBehaviourEffective = bViewToTargetBlocked;

		if (!bIsAdvancedBehaviourEffective)
			bHasSetAdvancedVectors = false;

		return bViewToTargetBlocked;
	}

	private void RunAdvancedCollision(Vector3 TP, RaycastHit Hit, Quaternion Rotation, Vector3 FallbackDirection)
	{
		if (!bHasSetAdvancedVectors)
		{
			AdvancedForwardVector = Target.forward;
			AdvancedRightVector = Target.right;

			bHasSetAdvancedVectors = true;
		}

		bIsAdvancedBehaviourEffective = true;

		if (bDrawAdvancedCollisionLines)
		{
			DrawArrow(transform.position, AdvancedForwardVector, Color.blue);
			DrawArrow(transform.position, AdvancedRightVector, Color.red);
		}

		// Dynamically adjust the angle to be inversely proportional to the distance with the Wall and the Max Spring Arm Distance.
		float DeltaAngle = 90f * ((Distance - Hit.distance) / Distance);
		ClampMax(ref DeltaAngle, MaxAdvancedCollisionAngle);

		// Make and Rotate L and R Vectors Rotated by the above Angle.
		Vector3 TargetDirectionToCamera = (Hit.point - TP).FNormalised();
		Vector3 Right = RotateVector(TargetDirectionToCamera, Vector3.up, DeltaAngle);
		Vector3 Left = RotateVector(TargetDirectionToCamera, Vector3.up, -DeltaAngle);

		Ray RR = new Ray(TP, Right);
		Ray RL = new Ray(TP, Left);

		// If the above Rays don't hit anything, set default positions for L and R at Distance units away from Target.
		Vector3 PointRight = TP + Right * Distance;
		Vector3 PointLeft = TP + Left * Distance;

		if (bDrawAdvancedCollisionLines)
			DrawArrow(TP, TargetDirectionToCamera * Hit.distance, Color.yellow);

		bool bRHit = Physics.Raycast(RR, out RaycastHit RHit, Distance, OnlyCollideWith);
		bool bLHit = Physics.Raycast(RL, out RaycastHit LHit, Distance, OnlyCollideWith);

		if (bRHit)
		{
			PointRight = RHit.point;

			if (bDrawAdvancedCollisionLines)
				DrawArrow(TP, Right * RHit.distance, Color.red);
		}

		if (bLHit)
		{
			PointLeft = LHit.point;

			if (bDrawAdvancedCollisionLines)
				DrawArrow(TP, Left * LHit.distance, Color.green);
		}

		// Look at TargetPos() while still maintaining the inherited Pitch rotation.
		Camera.LookAt(TP);
		Camera.localEulerAngles = new Vector3(CameraRotationInherited.x, Camera.localEulerAngles.y, Camera.localEulerAngles.z);

		// Choose the Ray furthest away from Target.
		SetPositionAndRotation(
			SqrDist(TP, PointRight) < SqrDist(TP, PointLeft)
				? PointLeft
				: PointRight,
			Rotation
		);
	}

	void SetPositionAndRotation(Vector3 FinalPosition, Quaternion FinalRotation)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			Camera.position = FinalPosition;
			Camera.rotation = FinalRotation;
			return;
		}
#endif

		if (TargetPosition != FinalPosition || TargetRotation != FinalRotation)
		{
			TargetPosition = FinalPosition;
			TargetRotation = FinalRotation;
		}
	}

	Vector3 TargetPos() => Target.position + TargetOffset * Target.up.y;

	Quaternion GetInheritedRotation()
	{
		return Quaternion.Euler(new Vector3(CameraRotationInherited.x + PitchDegrees *.5f, CameraRotationInherited.y + GetInheritedAxis(Target.localEulerAngles.y)));
	}

	float GetInheritedAxis(float AxisAngle)
	{
		float TargetAxis = AxisAngle;
		if (TargetAxis < 0f)
			TargetAxis = 360f + TargetAxis; // TargetAxis is negative.
		return TargetAxis;
	}

	private void ScrollDistance(Transform playerTransform, float scrollValue)
	{
		if (bEnableScrollToDistance && (playerTransform == Target || bIsAverageTracking))
		{
			Distance += scrollValue * (bInvertZ ? -1f : 1f) * -ScrollSensitivity;

			Distance = Mathf.Clamp(Distance, 1, MaxDistance);
		}
	}

	void UpdateRotationOnMouse(Transform playerTransform, Vector3 inputAmount, bool bCamFinished = false)
	{
		// only perform the action if the input is comming from the player this camera is following
		if (playerTransform == Target || bIsAverageTracking)
		{
			if (!bCamFinished)
			{
				float DeltaX = inputAmount.x * OrbitSensitivity * Time.deltaTime;
				float DeltaY = inputAmount.y * OrbitSensitivity * Time.deltaTime;

				DetermineInverse(ref DeltaX, ref DeltaY);

				if (!bInheritRotation || bIsAverageTracking)
				{
					GimbalRotation.x += DeltaX;
					CameraRotation.y += DeltaX;

					if (GimbalRotation.y - DeltaY < MinMaxOrbitAngle.y && GimbalRotation.y - DeltaY >= MinMaxOrbitAngle.x)
					{
						GimbalRotation.y -= DeltaY;
						CameraRotation.x -= DeltaY;
					}
				}
				else
				{
					CameraRotationInherited.x -= DeltaY;
					CameraRotationInherited.y += DeltaX;

					if (GimbalRotationInherited.y - DeltaY < MinMaxOrbitAngle.y && GimbalRotationInherited.y - DeltaY >= MinMaxOrbitAngle.x)
					{
						GimbalRotationInherited.y -= DeltaY;
					}
				}
			}
			else
			{
				GimbalRotationInherited = DefaultGimbalRotation;
				CameraRotationInherited = DefaultCameraRotation;
			}
		}
	}

	void DetermineInverse(ref float DeltaX, ref float DeltaY)
	{
		if (bInvertX)
			Inverse(ref DeltaX);
		if (bInvertY)
			Inverse(ref DeltaY);

		static void Inverse(ref float F) => F *= -1f;
	}

	private void PanCameraOnMouse()
	{
		Vector3 MousePosition = Input.mousePosition;

		if (Input.GetMouseButton(2))
		{
			float DeltaX = (MousePosition.x - PreviousMousePanPosition.x) * OrbitSensitivity * OrbitSensitivity * Time.deltaTime;
			float DeltaY = (MousePosition.y - PreviousMousePanPosition.y) * OrbitSensitivity * OrbitSensitivity * Time.deltaTime;

			// Ensure 'Right' and 'Up' is relative to the Camera.
			TargetOffset -= DeltaX * Time.deltaTime * Camera.right + DeltaY * Time.deltaTime * Camera.up;
			TargetOffset = Vector3.ClampMagnitude(TargetOffset, MaxPanDistance);
		}
		else
		{
			if (!bPermanentlyChangePanTargetOffset)
				TargetOffset = Vector3.Lerp(TargetOffset, OriginalTargetOffset, .2f);
		}

		PreviousMousePanPosition = MousePosition;
	}

	public void GetForwardRight(out Vector3 Forward, out Vector3 Right)
	{
		Forward = transform.forward;
		Right = transform.right;

#if false // Kept for Options.
		bool bCheck = bIsAdvancedBehaviourEffective && bInheritRotation;

		Forward = bCheck
			? transform.forward
			: transform.forward;

		Right = bCheck
			? transform.right
			: transform.right;
#endif
	}

	[Exec("Toggles No Clip on the first Spring Arm.")]
	public void NoClia()
	{
		bNoClip = !bNoClip;

		if (!bNoClip)
			TargetOffset = OriginalTargetOffset;
	}

	[Exec("Teleports this Spring Arm's Target to the current position.")]
	public void TP_Pos()
	{
		if (bNoClip)
			NoClia();

		Target.position = transform.position;
	}

#if false
	void ComputeProjection()
	{
		if (bUseCustomProjection && Distance > 3)
		{
			Plane = Camera;

			if (Physics.Linecast(Target.position, Camera.position, out RaycastHit Intercept, OnlyCollideWith))
			{
				NearClipDistance = Intercept.distance;
			}
			else
			{
				CameraComponent.projectionMatrix = DefaultProjection;
				return;
			}

			int Dot = Math.Sign(Vector3.Dot(Plane.forward, (Target.position - Camera.position).normalized));
			Vector3 CameraWorldPosition = CameraComponent.worldToCameraMatrix.MultiplyPoint(Target.position);
			Vector3 CameraNormal = CameraComponent.worldToCameraMatrix.MultiplyVector((Target.position - Camera.position).normalized) * 1;

			float CameraDistance = -Vector3.Dot(CameraWorldPosition, CameraNormal) + NearClipDistance;

			// If the Camera is too close to the Target, don't use oblique projection.
			if (Mathf.Abs(CameraDistance) > DistanceLimit)
			{
				Vector4 ClipPlaneCameraSpace = new Vector4(CameraNormal.x, CameraNormal.y, CameraNormal.z, CameraDistance);

				CameraComponent.projectionMatrix = CameraComponent.CalculateObliqueMatrix(ClipPlaneCameraSpace);
			}
			else
			{
				CameraComponent.projectionMatrix = DefaultProjection;
			}
		}
		else
		{
			CameraComponent.projectionMatrix = DefaultProjection;
		}
	}
#endif

#region Settings

	public SpringArmSettings GetSettings()
	{
		SpringArmSettings RetVal;

#if UNITY_EDITOR
		RetVal.bDrawRotationalLines = bDrawRotationalLines;
#endif

		RetVal.TPosition = transform.position;
		RetVal.TRotation = transform.rotation;

		RetVal.FOV = CameraComponent.fieldOfView;

		// RetVal.Camera = Camera; // Needs to be the transform of the new Spring Arm.
		// RetVal.Target = Target. // Needs to be different for each Spring Arm.
		RetVal.TargetOffset = TargetOffset;

		// RetVal.CameraComponent = CameraComponent; // Needs to be different for each Spring Arm.
		RetVal.Distance = Distance;
		RetVal.GimbalRotation = GimbalRotation;
		RetVal.CameraRotation = CameraRotation;
		RetVal.bInheritRotation = bInheritRotation;

		RetVal.bEnableScrollToDistance = bEnableScrollToDistance;
		RetVal.ScrollSensitivity = ScrollSensitivity;
		RetVal.DefaultGimbalRotation = DefaultGimbalRotation;
		RetVal.DefaultCameraRotation = DefaultCameraRotation;
		RetVal.OrbitSensitivity = OrbitSensitivity;
		RetVal.PreviousMouseDragPosition = PreviousMouseDragPosition;
		RetVal.GimbalRotationInherited = GimbalRotationInherited;
		RetVal.CameraRotationInherited = CameraRotationInherited;
		RetVal.PreviousMousePanPosition = PreviousMousePanPosition;
		RetVal.OriginalTargetOffset = OriginalTargetOffset;

		RetVal.bInvertX = bInvertX;
		RetVal.bInvertY = bInvertY;
		RetVal.bInvertZ = bInvertZ;

		RetVal.bRunCollisionChecks = bRunCollisionChecks;
		RetVal.OnlyCollideWith = OnlyCollideWith;

		RetVal.PositionalLagStrength = PositionalLagStrength;
		RetVal.RotationalLagStrength = RotationalLagStrength;
		RetVal.TargetPosition = TargetPosition;
		RetVal.TargetRotation = TargetRotation;

		RetVal.bUseCustomProjection = bUseCustomProjection;
		// RetVal.Plane = Plane; // Needs to be different for each Spring Arm.
		RetVal.NearClipDistance = NearClipDistance;
		RetVal.DistanceLimit = DistanceLimit;
		RetVal.DefaultProjection = DefaultProjection;

		return RetVal;
	}

	public void SetSettings(SpringArmSettings Settings, Transform Target, Camera Component)
	{
#if UNITY_EDITOR
		bDrawRotationalLines = Settings.bDrawRotationalLines;
#endif

		transform.position = Settings.TPosition;
		transform.rotation = Settings.TRotation;

		Component.fieldOfView = Settings.FOV;

		Camera = transform;
		this.Target = Target;
		TargetOffset = Settings.TargetOffset;

		CameraComponent = Component;
		Distance = Settings.Distance;
		GimbalRotation = Settings.GimbalRotation;
		CameraRotation = Settings.CameraRotation;
		bInheritRotation = Settings.bInheritRotation;

		bEnableScrollToDistance = Settings.bEnableScrollToDistance;
		ScrollSensitivity = Settings.ScrollSensitivity;
		DefaultGimbalRotation = Settings.DefaultGimbalRotation;
		DefaultCameraRotation = Settings.DefaultCameraRotation;
		OrbitSensitivity = Settings.OrbitSensitivity;
		PreviousMouseDragPosition = Settings.PreviousMouseDragPosition;
		GimbalRotationInherited = Settings.GimbalRotationInherited;
		CameraRotationInherited = Settings.CameraRotationInherited;
		PreviousMousePanPosition = Settings.PreviousMousePanPosition;
		OriginalTargetOffset = Settings.OriginalTargetOffset;

		bInvertX = Settings.bInvertX;
		bInvertY = Settings.bInvertY;
		bInvertZ = Settings.bInvertZ;

		bRunCollisionChecks = Settings.bRunCollisionChecks;
		OnlyCollideWith = Settings.OnlyCollideWith;

		PositionalLagStrength = Settings.PositionalLagStrength;
		RotationalLagStrength = Settings.RotationalLagStrength;
		TargetPosition = Settings.TargetPosition;
		TargetRotation = Settings.TargetRotation;

		bUseCustomProjection = Settings.bUseCustomProjection;
		NearClipDistance = Settings.NearClipDistance;
		DistanceLimit = Settings.DistanceLimit;
		DefaultProjection = Settings.DefaultProjection;
	}

#endregion

#if UNITY_EDITOR
	void OnValidate()
	{
		if (Camera && Target)
			PlaceCamera();

		GimbalRotation.y = Mathf.Clamp(GimbalRotation.y, -90f, 90f);

		PositionalLagStrength = Mathf.Clamp(PositionalLagStrength, Vector3.kEpsilon, 1f);
		RotationalLagStrength = Mathf.Clamp(RotationalLagStrength, Vector3.kEpsilon, 1f);

		AdvancedCollisionActivationDistance = Mathf.Clamp(AdvancedCollisionActivationDistance, 1, MaxDistance);
	}

	void OnDrawGizmosSelected()
	{
		if (bDrawSpringArmLine && Camera && Target)
			Debug.DrawLine(TargetPos(), Camera.position, Color.red);
	}

	//void OnGUI()
	//{
	//	GUI.Label(new Rect(100, 50, 250, 250), $"Is Advanced Collision On? {bIsAdvancedBehaviourEffective}");
	//	GUI.Label(new Rect(100, 75, 250, 250), $"Adv Forward: {AdvancedForwardVector:F2}");
	//	GUI.Label(new Rect(100, 100, 250, 250), $"Adv Right: {AdvancedRightVector:F2}");
	//}
#endif

}

public struct SpringArmSettings
{
#if UNITY_EDITOR
	public bool bDrawRotationalLines;
#endif

	public Vector3 TPosition;
	public Quaternion TRotation;
	public float FOV;

	public Vector3 TargetOffset;

	public float Distance;
	public Vector3 GimbalRotation;
	public Vector3 CameraRotation;
	public bool bInheritRotation;

	public bool bEnableScrollToDistance;
	public float ScrollSensitivity;
	public Vector3 DefaultGimbalRotation;
	public Vector3 DefaultCameraRotation;
	public float OrbitSensitivity;
	public Vector2 PreviousMouseDragPosition;
	public Vector3 GimbalRotationInherited;
	public Vector3 CameraRotationInherited;
	public Vector2 PreviousMousePanPosition;
	public Vector3 OriginalTargetOffset;

	public bool bInvertX; // Inverse LR dragging Orbit Controls.
	public bool bInvertY; // Inverse UD dragging Orbit Controls.
	public bool bInvertZ; // Inverse Zoom Controls.

	public bool bRunCollisionChecks;
	public LayerMask OnlyCollideWith;

	public float PositionalLagStrength;
	public float RotationalLagStrength;
	public Vector3 TargetPosition;
	public Quaternion TargetRotation;

	public bool bUseCustomProjection;
	public float NearClipDistance;
	public float DistanceLimit;
	public Matrix4x4 DefaultProjection;
}