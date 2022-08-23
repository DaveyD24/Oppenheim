using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A base class for handling the common functionality across all players.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] private float weight;
    [SerializeField] private float fuel;
    [SerializeField] private float bouyancy;
    [SerializeField] private string foodTag;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float health;

    protected float Weight { get => weight; private set => weight = value; }

    protected float Bouyancy { get => bouyancy; private set => bouyancy = value; }

    protected string FoodTag { get => foodTag; private set => foodTag = value; }

    protected float MovementSpeed { get => movementSpeed; private set => movementSpeed = value; }

    protected float RotationSpeed { get => rotationSpeed; private set => rotationSpeed = value; }

    protected float Health { get => health; private set => health = value; }

    protected float Guel { get => fuel; private set => fuel = value; }

    protected Rigidbody Rb { get; private set; }

    protected InputActions Inputs { get; private set; }

    // use Vector2 direction = ctx.ReadValue<Vector2>(); to get the values for each direction of movement
    protected abstract void Movement(InputAction.CallbackContext ctx);

    protected abstract void Jump(InputAction.CallbackContext ctx);

    protected abstract void PerformAbility(InputAction.CallbackContext ctx);

    protected void AdjustFuelValue(float amount)
    {
        fuel += amount;

        // also update the ui for it
    }

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
        weight = Rb.mass;
    }

    private void OnEnable()
    {
        Inputs = new InputActions();

        Inputs.Player.Move.performed += Movement;
        Inputs.Player.Ability.performed += PerformAbility;
        Inputs.Player.Jump.performed += Jump;

        Inputs.Player.Enable();
    }

    private void OnDisable()
    {
        Inputs = new InputActions();

        Inputs.Player.Move.performed -= Movement;
        Inputs.Player.Ability.performed -= PerformAbility;
        Inputs.Player.Jump.performed -= Jump;

        Inputs.Player.Disable();
    }

    private void AddBouyancy()
    {
        // apply bouyancy while in water
    }

    private void Respawn()
    {
        // respawning code...
    }
}
