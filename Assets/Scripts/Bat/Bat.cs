using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(BatMovement), typeof(BatEvents))]
public class Bat : PlayerController
{
	// Expose Protected Fields.
	public Rigidbody Physics => Rb;
	public float GroundSpeed => MovementSpeed;
	public float YawSpeed => RotationSpeed;
	//public string Food => FoodTag;
	public BatEvents Events => EventsComponent;

	BatMovement MovementComponent;
	BatEvents EventsComponent;

	protected override void Start()
	{
		base.Start();

		MovementComponent = GetComponent<BatMovement>();
		EventsComponent = GetComponent<BatEvents>();

		BindMiscellaneousInputs();
	}

	void BindMiscellaneousInputs()
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

	protected override void Movement(CallbackContext Context) => MovementComponent.MovementBinding(ref Context);

	protected override void Jump(CallbackContext Context) => MovementComponent.JumpBinding(ref Context);

	protected override void PerformAbility(CallbackContext ctx) => MovementComponent.AbilityBinding();

	public void AdjustEnergy(float Amount)
	{
		AdjustFuelValue(Amount);
	}

	public void AdjustHealth(float Amount)
	{
		// Can't modify Health because it's a protected value with no set method.
	}

}
