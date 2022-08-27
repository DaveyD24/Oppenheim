using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(BatMovement), typeof(BatEvents))]
public class Bat : PlayerController
{
	// Expose Protected Fields.
	public Rigidbody Physics { get => Rb; }
	public float GroundSpeed { get => MovementSpeed; }
	public float YawSpeed { get => RotationSpeed; }
	public new string FoodTag { get => FoodTag; }

	BatMovement MovementComponent;

	protected override void Start()
	{
		base.Start();

		MovementComponent = GetComponent<BatMovement>();

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
}
