using System;
using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// A base class for handling the common functionality across all players.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerIdObject playerIDSO;
    [SerializeField] private DefaultPlayerDataObject defaultDataSO;
    [SerializeField] private float weight;
    private float fuel;
    [SerializeField] private float bouyancy;
    [SerializeField] private string foodTag;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float health;

    public Rigidbody Rb { get; private set; }

    public float Weight { get => weight; private set => weight = value; }

    // A unique object each scene object gets assigned, being largly used to store the players id
    protected PlayerIdObject PlayerID { get => playerIDSO; private set => playerIDSO = value; }

    protected DefaultPlayerDataObject DefaultPlayerData { get => defaultDataSO; private set => defaultDataSO = value; }

    protected float Bouyancy { get => bouyancy; private set => bouyancy = value; }

    protected string FoodTag { get => foodTag; private set => foodTag = value; }

    protected float MovementSpeed { get => movementSpeed; private set => movementSpeed = value; }

    protected float RotationSpeed { get => rotationSpeed; private set => rotationSpeed = value; }

    protected float Health { get => health; private set => health = value; }

    protected float Fuel { get => fuel; set => fuel = Mathf.Clamp(value, 0, defaultDataSO.MaxFuel); }

    protected InputActions Inputs { get; private set; }

    // use Vector2 direction = ctx.ReadValue<Vector2>(); to get the values for each direction of movement
    protected abstract void Movement(InputAction.CallbackContext ctx);

    protected abstract void Jump(InputAction.CallbackContext ctx);

    protected abstract void PerformAbility(InputAction.CallbackContext ctx);

    protected void AdjustFuelValue(float amount)
    {
        Fuel += amount;
        UIEvents.OnFuelChanged(PlayerID.PlayerID, Fuel / defaultDataSO.MaxFuel);

        if (Fuel <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void Update()
    {
        AdjustFuelValue(-defaultDataSO.DecreaseFuelAmount.Evaluate(Fuel / defaultDataSO.MaxFuel) * Time.deltaTime);
    }

    protected virtual void Start()
    {
        Rb = GetComponent<Rigidbody>();
        weight = Rb.mass;
        Fuel = defaultDataSO.MaxFuel;
    }

    protected virtual void OnDeath()
    {
        Debug.Log("Player Died");
        Inputs.Player.Disable();
    }

    private void OnEnable()
    {
        // setup the inputs to use
        Inputs = new InputActions();

        Inputs.Player.Move.performed += Movement;
        Inputs.Player.Move.canceled += Movement;
        Inputs.Player.Ability.performed += PerformAbility;
        Inputs.Player.Jump.performed += Jump;
        Inputs.Player.Jump.canceled += Jump;

        Inputs.Player.Enable();

        // assign the nessesary functions to the event system
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
