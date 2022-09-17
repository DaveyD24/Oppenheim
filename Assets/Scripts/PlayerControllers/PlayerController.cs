using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float fuel;
    private bool isFarEnoughAway = false;

    // input handleing things
    public InputActionMap player;

    public static IEnumerator DeathWaitTimer { get; private set; }

    [HideInInspector] public bool Active { get; set; } = false;

    public Rigidbody Rb { get; private set; }

    public float Weight { get; private set; }

    // Not a Property: Use GetGroundCheckPosition() instead.
    [field: SerializeField] protected Vector3 groundCheckPosition;

    [field: Header("Inherited from Player Controller")]

    // A unique object each scene object gets assigned, being largly used to store the players id
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }

    [field: SerializeField] protected DefaultPlayerDataObject DefaultPlayerData { get; private set; }

    [field: SerializeField] protected float Bouyancy { get; private set; }

    [field: SerializeField] protected float MovementSpeed { get; private set; }

    [field: SerializeField] protected float RotationSpeed { get; private set; }

    [field: SerializeField] protected float Health { get; private set; }

    [field: SerializeField] protected float GroundCheckRadius { get; private set; }

    protected float CurrentFuel { get => fuel; set => fuel = Mathf.Clamp(value, 0, DefaultPlayerData.MaxFuel); }

    SwitchManager switchManager;
    float FollowSpeed = 0.001f;

    protected InputActionAsset Inputs { get; private set; }

    public virtual bool IsGrounded()
    {
        Collider[] groundOverlap = Physics.OverlapSphere(GetGroundCheckPosition(), GroundCheckRadius);

        // A 'Valid Contact' is not this PlayerController.
        int numberOfValidContacts = groundOverlap.Count(
            collider => collider.gameObject.transform.root != transform
        );

        // If we are the Monkey, don't count any Clingable surface as a Valid Contact.
        if (this is MonkeyController)
        {
            numberOfValidContacts -= groundOverlap.Count(
                collider1 => collider1.gameObject.CompareTag("Clingable"));
        }

        return numberOfValidContacts > 0;
    }

    public Vector3 GetGroundCheckPosition()
    {
        return transform.position + groundCheckPosition;
    }

    public void Activate()
    {
        Active = true;
        UIEvents.CanvasStateChanged(PlayerIdSO.PlayerID, true);
    }

    public void Deactivate()
    {
        Active = false;
        UIEvents.CanvasStateChanged(PlayerIdSO.PlayerID, false);
    }

    public bool IsActive()
    {
        return Active;
    }

    // use Vector2 direction = ctx.ReadValue<Vector2>(); to get the values for each direction of movement
    protected abstract void Movement(InputAction.CallbackContext ctx);

    protected abstract void Jump(InputAction.CallbackContext ctx);

    protected abstract void PerformAbility(InputAction.CallbackContext ctx);

    protected void AdjustFuelValue(float amount)
    {
        CurrentFuel += amount;

        // UIEvents.OnFuelChanged(PlayerIdSO.PlayerID, CurrentFuel / DefaultPlayerData.MaxFuel);
        if (CurrentFuel <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void Update()
    {
        if (Rb.transform.position.y < 2.5f)
        {
            OnDeath();
        }

        // AdjustFuelValue(-DefaultPlayerData.DecreaseFuelAmount.Evaluate(CurrentFuel / DefaultPlayerData.MaxFuel) * Time.deltaTime);
        if (Vector3.Distance(this.gameObject.transform.position, switchManager.GetActivePlayer().transform.position) > 3.0f)
        {
            isFarEnoughAway = true;
        }
        else
        {
            isFarEnoughAway = false;
        }

        if (!Active && isFarEnoughAway)
        {
            Vector3 desiredPosition = switchManager.GetActivePlayer().transform.position;
            Vector3 smoothedPosition = Vector3.Lerp(this.transform.position, desiredPosition, FollowSpeed);
            Vector3 flattenedPosition = new Vector3(smoothedPosition.x, this.transform.position.y, smoothedPosition.z);
            this.transform.position = flattenedPosition;
            this.transform.LookAt(switchManager.GetActivePlayer().transform);
        }

        //AdjustFuelValue(-DefaultPlayerData.DecreaseFuelAmount.Evaluate(CurrentFuel / DefaultPlayerData.MaxFuel) * Time.deltaTime);
    }

    protected virtual void Start()
    {
        Rb = GetComponent<Rigidbody>();
        switchManager = FindObjectOfType<SwitchManager>();
        Weight = Rb.mass;
        fuel = DefaultPlayerData.MaxFuel;

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    protected virtual void OnDeath()
    {
        player.Disable();
        if (DeathWaitTimer == null)
        {
            Debug.Log("Player Died");

            DeathWaitTimer = DeathWait();
            StartCoroutine(DeathWaitTimer);
        }
    }

    // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Actions.html see here for further details on the input types
    protected virtual void OnEnable()
    {
        // assign the nessesary functions to the event system
        GameEvents.OnCollectFuel += MaxFuel;
        GameEvents.OnDie += Respawn;
    }

    protected virtual void OnDisable()
    {
        if (Inputs != null)
        {
            player.FindAction("Move").performed -= Movement;
            player.FindAction("Move").canceled -= Movement;
            player.FindAction("Ability").performed -= PerformAbility;

            // Inputs.Player.Ability.canceled += PerformAbility;
            player.FindAction("Jump").performed -= Jump;

            player.Disable();
        }

        GameEvents.OnCollectFuel -= MaxFuel;
        GameEvents.OnDie -= Respawn;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(GetGroundCheckPosition(), GroundCheckRadius);
    }

    public virtual void ActivateInput(PlayerInput playerInput)
    {
        // setup the inputs to use
        Inputs = playerInput.actions;

        player = Inputs.FindActionMap("Player");

        player.FindAction("Move").performed += Movement;
        player.FindAction("Move").canceled += Movement;
        player.FindAction("Ability").performed += PerformAbility;

        // Inputs.Player.Ability.canceled += PerformAbility;
        player.FindAction("Jump").performed += Jump;

        // Inputs.Player.Jump.canceled += Jump;
        player.Enable();

        Active = true;
    }

    private void AddBouyancy()
    {
        // apply bouyancy while in water
    }

    protected virtual void Respawn()
    {
        // respawning code...
        Rb.velocity = Vector3.zero;
        Rb.transform.position = startPosition;
        transform.rotation = startRotation;
        if (player != null)
        {
            player.Enable();
        }
    }

    private void MaxFuel(int playerId)
    {
        if (playerId == PlayerIdSO.PlayerID)
        {
            AdjustFuelValue(DefaultPlayerData.MaxFuel);
        }
    }

    private IEnumerator DeathWait()
    {
        Debug.Log("Deathingwqertgyhferwrtf");
        yield return new WaitForSeconds(5);
        GameEvents.Die();
        DeathWaitTimer = null;
    }
}
