using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BatMovement), typeof(BatEvents))]
public class Bat : PlayerController
{
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
	}

	private void BindMiscellaneousInputs()
	{
		player.FindAction("Move").canceled += (CallbackContext Context) =>
		{
			MovementComponent.HandleMovement(Vector2.zero);
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
}
