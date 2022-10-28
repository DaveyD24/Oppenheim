using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using static global::BatMathematics;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(BatMovement), typeof(BatEvents))]
public class Bat : PlayerController
{
#if UNITY_EDITOR
	[field: Header("Start Reset")]
	[field: ContextMenuItem("Set Start Transform", "SetStartTransform")]
#pragma warning disable SA1202 // Elements should be ordered by access
	[field: SerializeField] public Vector3 StageStartPosition { get; set; }

	[field: ContextMenuItem("Move to Start", "MoveToStartTransform")]
	[field: SerializeField] public Quaternion StageStartRotation { get; set; }
#pragma warning restore SA1202 // Elements should be ordered by access

#endif

	[SerializeField] private GameObject ragdol;
	[SerializeField] private GameObject baseMesh;
	private BoxCollider boxCollider;

	// Expose Protected Fields.
	public Rigidbody Physics => Rb;

	public float GroundSpeed => MovementSpeed;

	public float YawSpeed => RotationSpeed;

	// public string Food => FoodTag;
	public BatEvents Events => EventsComponent;

	BatMovement MovementComponent;
	BatEvents EventsComponent;

	protected override void Start()
	{
		base.Start();

		MovementComponent = GetComponent<BatMovement>();
		EventsComponent = GetComponent<BatEvents>();
		boxCollider = gameObject.GetComponent<BoxCollider>();
	}

	public override void ActivateInput(int playerID, PlayerInput playerInput)
	{
		if (playerID == PlayerIdSO.PlayerID)
		{
			base.ActivateInput(playerID, playerInput);
			BindMiscellaneousInputs();
		}
	}

	private void BindMiscellaneousInputs()
	{
		PlayerInput.FindAction("Move").canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleMovement(Vector2.zero);
			MovementComponent.StopGradualAcceleration();
		};

		PlayerInput.FindAction("Jump").canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleJump(0f);
		};

		PlayerInput.FindAction("Look").performed += (CallbackContext Context) =>
		{
			MovementComponent.LookBinding(ref Context);
		};

		PlayerInput.FindAction("Look").canceled += (CallbackContext Context) =>
		{
			MovementComponent.LookBinding(ref Context);

			// MovementComponent.HandleLook(ref Context);
		};
	}

	//public void AdjustEnergy(float amount)
	//{
	//	AdjustFuelValue(amount);
	//}

	public void AdjustHealth(float amount)
	{
		// Can't modify Health because it's a protected value with no set method.
	}

	public void FallDamage(/*float ImpactVelocity*/)
	{
		TakeFallDamage(/*ImpactVelocity*/);
	}

	protected override void Movement(CallbackContext Context) => MovementComponent.MovementBinding(ref Context);

	protected override void Jump(CallbackContext Context) => MovementComponent.JumpBinding(ref Context);

	protected override void PerformAbility(CallbackContext ctx) => MovementComponent.AbilityBinding();

	public override Vector3 GetGroundCheckPosition()
	{
		// Fix Global Down as a Local direction.
		Vector3 WorldToLocalDown = transform.InverseTransformDirection(-transform.up);

		// Set the origin of the Ground Check to the centre of the Bat.
		WorldToLocalDown += Rb.centerOfMass;

		return transform.position + WorldToLocalDown + groundCheckPosition;
	}

	protected override bool ShouldTakeFallDamage(Collision Collision, out float RelativeVelocity)
	{
		RelativeVelocity = Collision.relativeVelocity.magnitude;

		if (RelativeVelocity < FallDamageThreshold)
		{
			return false;
		}

		// Take damage if landing/crashing at an Angle > than 30 degrees of the surface.
		float Angle = FAngle(transform.up, Collision.contacts[0].normal);
		bool bTakeFallDamage = Angle > 30f;

		if (bTakeFallDamage)
		{
			Debug.Log($"Collision with {Collision.gameObject.name} at {Angle:F0} degrees at {RelativeVelocity:F0}m/s");
		}

		return bTakeFallDamage;
	}

	protected override void PlayFuelCollectionSound()
	{
		Events.OnMangoCollected();
	}

	public override void OnDeath()
	{
		base.OnDeath();
		MovementComponent.ForceStopAllMovement();

		baseMesh.SetActive(false);
		boxCollider.enabled = false;
		ragdol.SetActive(true);
		Rb.isKinematic = true;
		ragdol.transform.position = transform.position;
	}

	protected override void Respawn()
	{
		baseMesh.SetActive(true);
		boxCollider.enabled = true;
		ragdol.SetActive(false);
		Rb.isKinematic = false;
		MovementComponent.ForceStopAllMovement();
		base.Respawn();
	}

	protected override void OnDustParticles(Vector3 Position)
	{
		base.OnDustParticles(Position);

		Audio.PlayUnique("Crash", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd, .321f);
	}

#if UNITY_EDITOR
	private void SetStartTransform()
	{
		StageStartPosition = transform.position;
		StageStartRotation = transform.rotation;
	}

	private void MoveToStartTransform()
	{
		transform.rotation = StageStartRotation;
		transform.position = StageStartPosition;
	}
#endif
}
