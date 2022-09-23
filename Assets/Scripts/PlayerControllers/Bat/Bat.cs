using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using static global::BatMathematics;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BatMovement), typeof(BatEvents))]
public class Bat : PlayerController
{
#if UNITY_EDITOR
	[Header("EDITOR ONLY")]
	// True when on a testing scene. Sets this Bat to 'Active' so
	// that it can be controlled in a scene without a SwitchManager.
	[SerializeField] private bool bIsStandalone;
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

#if UNITY_EDITOR
			if (bIsStandalone)
			{
				Activate();
			}
#endif
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

	public void AdjustEnergy(float amount)
	{
		AdjustFuelValue(amount);
	}

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
		Vector3 worldToLocalDown = transform.InverseTransformDirection(-transform.up);

		// Set the origin of the Ground Check to the centre of the Bat.
		worldToLocalDown += Rb.centerOfMass;

		return transform.position + worldToLocalDown;
	}

	protected override bool ShouldTakeFallDamage(Collision collision, out float relativeVelocity)
	{
		relativeVelocity = collision.relativeVelocity.magnitude;

		if (relativeVelocity < FallDamageThreshold)
		{
			return false;
		}

		// Take damage if landing/crashing at an Angle > than 30 degrees of the surface.
		float angle = FAngle(transform.up, collision.contacts[0].normal);
		bool bTakeFallDamage = angle > 30f;

		if (bTakeFallDamage)
		{
			Debug.Log($"Collision with {collision.gameObject.name} at {angle:F0} degrees at {relativeVelocity:F0}m/s");
		}

		return bTakeFallDamage;
	}

	public override void OnDeath()
	{
		base.OnDeath();
		baseMesh.SetActive(false);
		boxCollider.enabled = false;
		ragdol.SetActive(true);
		Rb.isKinematic = true;
		ragdol.transform.position = transform.position;
		MovementComponent.HandleMovement(Vector2.zero);
		MovementComponent.StopGradualAcceleration();
	}

	protected override void Respawn()
	{
		baseMesh.SetActive(true);
		boxCollider.enabled = true;
		ragdol.SetActive(false);
		Rb.isKinematic = false;
		base.Respawn();
	}
}
