using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

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

		BindMiscellaneousInputs();

#if UNITY_EDITOR
		if (bIsStandalone)
			Activate();
#endif
	}

	private void BindMiscellaneousInputs()
	{
		Inputs.Player.Move.canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleMovement(Vector2.zero);
		};

		Inputs.Player.Jump.canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleJump(0f);
		};

		Inputs.Player.Look.performed += (CallbackContext Context) =>
		{
			MovementComponent.LookBinding(ref Context);
		};

		Inputs.Player.Look.canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleLook(Vector2.zero);
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
}
