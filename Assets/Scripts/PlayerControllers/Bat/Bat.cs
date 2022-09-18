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
	[SerializeField] bool bIsStandalone;
#endif

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
	}

	public override void ActivateInput(PlayerInput playerInput)
	{
		base.ActivateInput(playerInput);
		BindMiscellaneousInputs();

#if UNITY_EDITOR
		if (bIsStandalone)
			Activate();
#endif
	}

	private void BindMiscellaneousInputs()
	{
		player.FindAction("Move").canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleMovement(Vector2.zero);
			MovementComponent.StopGradualAcceleration();
		};

		player.FindAction("Jump").canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleJump(0f);
		};

		player.FindAction("Look").performed += (CallbackContext Context) =>
		{
			MovementComponent.LookBinding(ref Context);
		};

		player.FindAction("Look").canceled += (CallbackContext Context) =>
		{
			MovementComponent.LookBinding(ref Context);
			//MovementComponent.HandleLook(ref Context);
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
		Vector3 WorldToLocalDown = transform.InverseTransformDirection(-transform.up);

		// Set the origin of the Ground Check to the centre of the Bat.
		WorldToLocalDown += Rb.centerOfMass;

		return transform.position + WorldToLocalDown;
	}

	protected override bool ShouldTakeFallDamage(Collision collision, out float relativeVelocity)
	{
		relativeVelocity = collision.relativeVelocity.magnitude;

		if (relativeVelocity < FallDamageThreshold)
			return false;

		// Take damage if landing/crashing at an Angle > than 30 degrees of the surface.
		float Angle = FAngle(transform.up, collision.contacts[0].normal);
		bool bTakeFallDamage = Angle > 30f;

		if (bTakeFallDamage)
			Debug.Log($"At Angle: {Angle}");

		return bTakeFallDamage;
	}
}
