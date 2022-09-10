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
    private float fuel;

    protected Vector3 startPosition;
    protected Quaternion startRotation;

    [Header("Inherited from Player Controller")]
    [SerializeField] protected GameObject Oppenheim;

    public Rigidbody Rb { get; private set; }

    public float Weight { get; private set; }

    // A unique object each scene object gets assigned, being largly used to store the players id
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }

    [field: SerializeField] protected DefaultPlayerDataObject DefaultPlayerData { get; private set; }

    [field: SerializeField] protected float Bouyancy { get; private set; }

    [field: SerializeField] protected float MovementSpeed { get; private set; }

    [field: SerializeField] protected float RotationSpeed { get; private set; }

    [field: SerializeField] protected float Health { get; private set; }

    // Not a Property: Use GetGroundCheckPosition() instead.
    [field: SerializeField] protected Vector3 GroundCheckPosition;

    [field: SerializeField] protected float GroundCheckRadius { get; private set; }

    protected float CurrentFuel { get => fuel; set => fuel = Mathf.Clamp(value, 0, DefaultPlayerData.MaxFuel); }

    SwitchManager switchManager;
    float FollowSpeed = 0.001f;

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
        if (Rb.transform.position.y < 2.5f)
        {
            GameEvents.Die();
        }

        if (Vector3.Distance(this.gameObject.transform.position, switchManager.GetActivePlayer().transform.position) > 3.0f)
        {
            isFarEnoughAway = true;
        }
        else
        {
            isFarEnoughAway = false;
        }

        if (!active && isFarEnoughAway)
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
        Debug.Log("Player Died");

        Inputs.Player.Disable();
    }

    // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/manual/Actions.html see here for further details on the input types
    protected virtual void OnEnable()
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

    protected virtual void OnDisable()
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
        transform.rotation = startRotation;
    }

    private void MaxFuel(int playerId)
    {
        if (playerId == PlayerIdSO.PlayerID)
        {
            AdjustFuelValue(DefaultPlayerData.MaxFuel);
        }
    }

    public virtual bool IsGrounded()
    {
        Collider[] groundOverlap = Physics.OverlapSphere(GetGroundCheckPosition(), GroundCheckRadius);

        // A 'Valid Contact' is not this PlayerController.
        int numberOfValidContacts = groundOverlap.Count(
            Collider => Collider.gameObject.transform.root != transform
        );

        // If we are the Monkey, don't count any Clingable surface as a Valid Contact.
        if (this is MonkeyController)
	{
            numberOfValidContacts -= groundOverlap.Count(
                Collider => Collider.gameObject.CompareTag("Clingable")
            );
	}

        return numberOfValidContacts > 0;
    }

    public Vector3 GetGroundCheckPosition()
    {
        return transform.position + GroundCheckPosition;
    }

    [HideInInspector] public bool active = false;
    public bool isFarEnoughAway = false;
    [SerializeField] Canvas canvas;

    public void Activate()
    {
        active = true;
        FindObjectOfType<Camera>().GetComponent<CameraFollow>().target = this.gameObject.transform;
        GetComponentInChildren<SpriteRenderer>().enabled = true;
        //canvas.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        active = false;
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        //canvas.gameObject.SetActive(false);
    }

    public bool IsActive()
    {
        return active;
    }

    private void EnableInstructions()
    {
        Oppenheim.SetActive(true);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(GetGroundCheckPosition(), GroundCheckRadius);
    }
}
