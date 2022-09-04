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
    private float fuel;

    private Vector3 startPosition;
    private Quaternion startRot;

    [SerializeField] private GameObject Oppenheim;

    public Rigidbody Rb { get; private set; }

    public float Weight { get; private set; }

    // A unique object each scene object gets assigned, being largly used to store the players id
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }

    [field: SerializeField] protected DefaultPlayerDataObject DefaultPlayerData { get; private set; }

    [field: SerializeField] protected float Bouyancy { get; private set; }

    [field: SerializeField] protected float MovementSpeed { get; private set; }

    [field: SerializeField] protected float RotationSpeed { get; private set; }

    [field: SerializeField] protected float Health { get; private set; }

    protected float CurrentFuel { get => fuel; set => fuel = Mathf.Clamp(value, 0, DefaultPlayerData.MaxFuel); }

    protected InputActions Inputs { get; private set; }

    // use Vector2 direction = ctx.ReadValue<Vector2>(); to get the values for each direction of movement
    protected abstract void Movement(InputAction.CallbackContext ctx);

    protected abstract void Jump(InputAction.CallbackContext ctx);

    protected abstract void PerformAbility(InputAction.CallbackContext ctx);

    protected void AdjustFuelValue(float amount)
    {
        CurrentFuel += amount;
        //UIEvents.OnFuelChanged(PlayerIdSO.PlayerID, CurrentFuel / DefaultPlayerData.MaxFuel);

        if (CurrentFuel <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void Update()
    {
        if (transform.position.y < 2)
        {
            GameEvents.Die();
        }

        //AdjustFuelValue(-DefaultPlayerData.DecreaseFuelAmount.Evaluate(CurrentFuel / DefaultPlayerData.MaxFuel) * Time.deltaTime);
    }

    protected virtual void Start()
    {
        Rb = GetComponent<Rigidbody>();
        Weight = Rb.mass;
        fuel = DefaultPlayerData.MaxFuel;

        startPosition = transform.position;
        startRot = transform.rotation;
    }

    protected virtual void OnDeath()
    {
        Debug.Log("Player Died");

        Inputs.Player.Disable();
    }

    // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Actions.html see here for further details on the input types
    private void OnEnable()
    {
        // setup the inputs to use
        Inputs = new InputActions();

        Inputs.Player.Move.performed += Movement;
        Inputs.Player.Move.canceled += Movement;
        Inputs.Player.Ability.performed += PerformAbility;
        // Inputs.Player.Ability.canceled += PerformAbility;

        Inputs.Player.Jump.performed += Jump;
        // Inputs.Player.Jump.canceled += Jump;

        Inputs.Player.Enable();

        // assign the nessesary functions to the event system
        GameEvents.OnCollectFuel += MaxFuel;
        UIEvents.OnShowIntructions += EnableInstructions;
        GameEvents.OnDie += Respawn;
    }

    private void OnDisable()
    {
        Inputs = new InputActions();

        Inputs.Player.Move.performed -= Movement;
        Inputs.Player.Ability.performed -= PerformAbility;
        Inputs.Player.Jump.performed -= Jump;

        Inputs.Player.Disable();

        UIEvents.OnShowIntructions -= EnableInstructions;
        GameEvents.OnCollectFuel -= MaxFuel;
        GameEvents.OnDie -= Respawn;
    }

    private void AddBouyancy()
    {
        // apply bouyancy while in water
    }

    private void Respawn()
    {
        // respawning code...
        Rb.velocity = Vector3.zero;
        Rb.transform.position = startPosition;
        transform.rotation = startRot;
    }

    private void MaxFuel(int playerId)
    {
        if (playerId == PlayerIdSO.PlayerID)
        {
            AdjustFuelValue(DefaultPlayerData.MaxFuel);
        }
    }

    [HideInInspector] public bool active = false;
    [SerializeField] Canvas canvas;

    public void Activate()
    {
        active = true;
        canvas.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        active = false;
        canvas.gameObject.SetActive(false);
    }

    public bool isActive()
    {
        return active;
    }

    private void EnableInstructions()
    {
        Oppenheim.SetActive(true);
    }
}
