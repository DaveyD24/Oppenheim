using System;
using UnityEngine;

public class SpringArm : MonoBehaviour
{

#if UNITY_EDITOR
	[Header("Debug Options. [EDITOR ONLY]")]
	[SerializeField] bool bDrawRotationalLines;
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

	[Header("Inverse Settings.")]
	[SerializeField] bool bInvertX; // Inverse LR dragging Orbit Controls.
	[SerializeField] bool bInvertY; // Inverse UD dragging Orbit Controls.
	[SerializeField] bool bInvertZ; // Inverse Zoom Controls.

	[Header("Collisions")]
	[SerializeField] bool bRunCollisionChecks;
	[SerializeField] LayerMask OnlyCollideWith;

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

	void Update()
	{
		UpdateRotationOnMouse();
		PanCameraOnMouse();

		if (Input.GetKeyDown(KeyCode.C))
			bInheritRotation = !bInheritRotation;

		ScrollDistance();

		if (bNoClip)
		{
			if (Input.GetKey(KeyCode.W))
			{
				Camera.position += Camera.forward * 15 * Time.deltaTime;
			}
			else if (Input.GetKey(KeyCode.S))
			{
				Camera.position -= Camera.forward * 15 * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.D))
			{
				Camera.position += Camera.right * 15 * Time.deltaTime;
			}
			else if (Input.GetKey(KeyCode.A))
			{
				Camera.position -= Camera.right * 15 * Time.deltaTime;
			}

			if (Input.GetKey(KeyCode.Q))
			{
				Camera.position += Vector3.up * 15 * Time.deltaTime;
			}
			else if (Input.GetKey(KeyCode.E))
			{
				Camera.position -= Vector3.up * 15 * Time.deltaTime;
			}
		}
	}

	void FixedUpdate()
	{
		if (bNoClip)
			return;

		Camera.position = Vector3.Lerp(Camera.position, TargetPosition, PositionalLagStrength);
		Camera.rotation = Quaternion.Slerp(Camera.rotation, TargetRotation, RotationalLagStrength);

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
			// Rotates the Spring Arm around to face the Target's forward vector.
			// Ignores the Target's Y-Axis, replacing it with the Yaw rotation,
			// relative to the Target, after inheritance.
			ArmDirection = Target.forward;
			ArmDirection.y = Mathf.Sin(-GimbalRotationInherited.y * Mathf.Deg2Rad);
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

		if (bViewToTargetBlocked)
		{
			Vector3 Point = Hit.point - FOV.direction;
			SetPositionAndRotation(Point, !bInheritRotation || bIsAverageTracking
				? Quaternion.Euler(CameraRotation)
				: GetInheritedRotation());
		}

		return bViewToTargetBlocked;
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
		return Quaternion.Euler(new Vector3(CameraRotationInherited.x, CameraRotationInherited.y + GetInheritedAxis(Target.localEulerAngles.y)));
	}

	float GetInheritedAxis(float AxisAngle)
	{
		float TargetAxis = AxisAngle;
		if (TargetAxis < 0f)
			TargetAxis = 360f + TargetAxis; // TargetAxis is negative.
		return TargetAxis;
	}

	void ScrollDistance()
	{
		if (bEnableScrollToDistance)
		{
			Distance += Input.mouseScrollDelta.y * (bInvertZ ? -1f : 1f) * -ScrollSensitivity;

			Distance = Mathf.Clamp(Distance, 1, MaxDistance);
		}
	}

	void UpdateRotationOnMouse()
	{
		Vector3 MousePosition = Input.mousePosition;

		if (Input.GetMouseButton(1))
		{
			float DeltaX = (MousePosition.x - PreviousMouseDragPosition.x) * OrbitSensitivity * Time.deltaTime;
			float DeltaY = (MousePosition.y - PreviousMouseDragPosition.y) * OrbitSensitivity * Time.deltaTime;

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

		PreviousMouseDragPosition = MousePosition;
	}

	void DetermineInverse(ref float DeltaX, ref float DeltaY)
	{
		if (bInvertX)
			Inverse(ref DeltaX);
		if (bInvertY)
			Inverse(ref DeltaY);

		static void Inverse(ref float F) => F *= -1f;
	}


	void PanCameraOnMouse()
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

	[Exec("Toggles No Clip on the first Spring Arm.")]
	public void NoClia()
	{
		bNoClip = !bNoClip;

		if (!bNoClip)
			TargetOffset = OriginalTargetOffset;
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
	}

	void OnDrawGizmosSelected()
	{
		if (Camera && Target)
			Debug.DrawLine(TargetPos(), Camera.position, Color.red);
	}
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