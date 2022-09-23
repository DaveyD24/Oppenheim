using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EventSystem;
using Unity;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// A base class for handling the common functionality across all players.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(AudioController))]
public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject controlObj;
    [SerializeField] private Slider fuelSlider;
    private bool bControlsHidden = false;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float fuel;
    private bool isFarEnoughAway = false;
    private SpriteRenderer activeIndicator;
    private float beforeCollideSpeed;

    private PlayerInput pInput;

    // input handleing things
    public static IEnumerator DeathWaitTimer { get; private set; }

    public InputActionMap PlayerInput { get; private set; }

    [HideInInspector] public bool Active { get; set; } = false;

    public Rigidbody Rb { get; private set; }

    public AudioController Audio { get; private set; }

    public float Weight { get; private set; }

    [field: Header("Inherited from Player Controller")]

    // A unique object each scene object gets assigned, being largly used to store the players id
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }

    [field: SerializeField] protected DefaultPlayerDataObject DefaultPlayerData { get; private set; }

    [field: SerializeField] protected float Bouyancy { get; private set; }

    [field: SerializeField] protected float MovementSpeed { get; private set; }

    [field: SerializeField] protected float RotationSpeed { get; private set; }

    [field: SerializeField] protected float Health { get; private set; }

    [field: SerializeField, Min(25f)] protected float FallDamageThreshold { get; private set; }

    // Not a Property, is Private: Use GetGroundCheckPosition() instead.
    [field: SerializeField] Vector3 groundCheckPosition;

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

    public virtual Vector3 GetGroundCheckPosition()
    {
        return transform.position + groundCheckPosition;
    }

    public void Activate()
    {
        Active = true;
        UIEvents.CanvasStateChanged(PlayerIdSO.PlayerID, true);
        if (!bControlsHidden)
        {
            controlObj.SetActive(true);
        }
    }

    public void Deactivate()
    {
        Active = false;
        UIEvents.CanvasStateChanged(PlayerIdSO.PlayerID, false);
        if (controlObj != null)
        {
            controlObj.SetActive(false);
        }
    }

    public bool IsActive()
    {
        return Active;
    }

    /// <summary>
    /// Setup the player to use the supplied controllers input.
    /// </summary>
    /// <param name="playerID">The id of the player which is to be activated.</param>
    /// <param name="playerInput">The input method, which is tied to the controller it is using.</param>
    public virtual void ActivateInput(int playerID, PlayerInput playerInput)
    {
        if (playerID == PlayerIdSO.PlayerID)
        {
            activeIndicator.enabled = true;

            // setup the inputs to use
            pInput = playerInput;
            Inputs = playerInput.actions;

            PlayerInput = Inputs.FindActionMap("Player");

            PlayerInput.FindAction("Move").performed += Movement;
            PlayerInput.FindAction("Move").canceled += Movement;
            PlayerInput.FindAction("Ability").performed += PerformAbility;

            // Inputs.Player.Ability.canceled += PerformAbility;
            PlayerInput.FindAction("Jump").performed += Jump;

            PlayerInput.FindAction("HideControls").performed += ControlsVisibility;

            PlayerInput.FindAction("RotatePlayer").performed += RotatePlayer;

            // Inputs.Player.Jump.canceled += Jump;
            PlayerInput.Enable();

            Activate();
        }
    }

    public void RotatePlayer(InputAction.CallbackContext ctx)
    {
        GameEvents.RotatePlayer(PlayerIdSO.PlayerID, pInput);
    }

    public void DeactivateInput(int playerID)
    {
        if (playerID == PlayerIdSO.PlayerID && Inputs != null)
        {
            PlayerInput.FindAction("Move").performed -= Movement;
            PlayerInput.FindAction("Move").canceled -= Movement;
            PlayerInput.FindAction("Ability").performed -= PerformAbility;

            // Inputs.Player.Ability.canceled += PerformAbility;
            PlayerInput.FindAction("Jump").performed -= Jump;
            PlayerInput.FindAction("RotatePlayer").performed -= RotatePlayer;
            PlayerInput.FindAction("HideControls").performed -= ControlsVisibility;

            PlayerInput.Disable();
            Deactivate();

            activeIndicator.enabled = false;
        }
    }

    // use Vector2 direction = ctx.ReadValue<Vector2>(); to get the values for each direction of movement
    protected abstract void Movement(InputAction.CallbackContext ctx);

    protected abstract void Jump(InputAction.CallbackContext ctx);

    protected abstract void PerformAbility(InputAction.CallbackContext ctx);

    private void ControlsVisibility(InputAction.CallbackContext ctx)
    {
        controlObj.SetActive(!controlObj.activeSelf);
    }

    protected void AdjustFuelValue(float amount)
    {
        CurrentFuel += amount;
        // Debug.Log(CurrentFuel + " " + amount);
        fuelSlider.value = CurrentFuel;

        // UIEvents.OnFuelChanged(PlayerIdSO.PlayerID, CurrentFuel / DefaultPlayerData.MaxFuel);
        if (CurrentFuel <= 0)
        {
            Debug.LogError("Player Lost All fuel and Died");
            OnDeath();
        }
    }

    protected virtual void Update()
    {
        if (transform.position.y < 2.5f)
        {
            Debug.Log("PLayer got too low and died");
            OnDeath();
        }

        if (Active)
        {
            // AdjustFuelValue(-DefaultPlayerData.DecreaseFuelAmount.Evaluate(CurrentFuel / DefaultPlayerData.MaxFuel) * Time.deltaTime * DefaultPlayerData.FuelLoseMultiplier);
        }

        if (switchManager.GetActivePlayer() != null)
        {
            if (Vector3.Distance(this.gameObject.transform.position, switchManager.GetActivePlayer().transform.position) > 3.0f)
            {
                isFarEnoughAway = true;
            }
            else
            {
                isFarEnoughAway = false;
            }

            // if (!Active && isFarEnoughAway)
            // {
            //     Vector3 desiredPosition = switchManager.GetActivePlayer().transform.position;
            //     Vector3 smoothedPosition = Vector3.MoveTowards(this.transform.position, desiredPosition, FollowSpeed);
            //     Vector3 flattenedPosition = new Vector3(smoothedPosition.x, this.transform.position.y, smoothedPosition.z);
            //
            //     Rb.MovePosition(flattenedPosition);
            //     this.transform.LookAt(switchManager.GetActivePlayer().transform);
            // }
        }

        // AdjustFuelValue(-DefaultPlayerData.DecreaseFuelAmount.Evaluate(CurrentFuel / DefaultPlayerData.MaxFuel) * Time.deltaTime);
    }

    protected virtual void Start()
    {
        activeIndicator = GetComponentInChildren(typeof(SpriteRenderer)) as SpriteRenderer;
        Rb = GetComponent<Rigidbody>();
        switchManager = FindObjectOfType<SwitchManager>();
        Audio = GetComponent<AudioController>();
        Weight = Rb.mass;
        CurrentFuel = DefaultPlayerData.MaxFuel;

        startPosition = transform.position;
        startRotation = transform.rotation;

        GameEvents.OnAddPlayerSwitch(PlayerIdSO.PlayerID);
    }

    public virtual void OnDeath()
    {
        if (PlayerInput != null)
        {
            PlayerInput.Disable();
        }

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
        GameEvents.OnActivatePlayer += ActivateInput;
        GameEvents.OnDeactivatePlayer += DeactivateInput;
    }

    protected virtual void OnDisable()
    {
        DeactivateInput(PlayerIdSO.PlayerID);

        GameEvents.OnCollectFuel -= MaxFuel;
        GameEvents.OnDie -= Respawn;

        GameEvents.OnActivatePlayer -= ActivateInput;
        GameEvents.OnDeactivatePlayer -= DeactivateInput;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (!Rb)
        {
            Rb = GetComponent<Rigidbody>();
        }

        Gizmos.color = new Color(0, 1, 1, 1);
        Gizmos.DrawSphere(GetGroundCheckPosition(), GroundCheckRadius);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        bool bTakeFallDamage = ShouldTakeFallDamage(collision, out float relativeVelocity);

        // if (relativeVelocity > MovementSpeed + 1f)
        // {
        //     Debug.Log($"{name} collided with {collision.collider.name} at {relativeVelocity:F2}m/s");
        // }
        if (bTakeFallDamage)
        {
            TakeFallDamage(/*relativeVelocity*/);
        }

        if (!collision.gameObject.CompareTag("Player") && beforeCollideSpeed > DefaultPlayerData.dustParticlesCollisionSpeed)
        {
            Instantiate(DefaultPlayerData.DustParticles, collision.GetContact(0).point, Quaternion.identity);
        }

        if(collision.gameObject.CompareTag("Blueprint"))
        {
            SceneManager.LoadScene("WinScene");
        }
    }

    protected void TakeFallDamage(/*float impactVelocity*/ /* This might be needed if we want to decrease health at lower speeds, and kill at higher speeds. */)
    {
        // too sensitive at the moment
        // GameEvents.Die();
        OnDeath();
    }

    protected virtual void FixedUpdate()
    {
        beforeCollideSpeed = Rb.velocity.magnitude;
    }

    protected virtual bool ShouldTakeFallDamage(Collision collision, out float relativeVelocity)
    {
        relativeVelocity = collision.relativeVelocity.magnitude;

        return relativeVelocity > FallDamageThreshold;
    }

    protected virtual void Respawn()
    {
        // respawning code...
        Rb.velocity = Vector3.zero;
        Rb.transform.position = startPosition;
        transform.rotation = startRotation;
        if (PlayerInput != null)
        {
            PlayerInput.Enable();
        }
    }

    private void LoseInput(PlayerInput player)
    {
        DeactivateInput(PlayerIdSO.PlayerID);
    }

    private void AddBouyancy()
    {
        // apply bouyancy while in water
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
        // Debug.Log("Player Died");

#if !UNITY_EDITOR
        Dictionary<string, object> eventData = new Dictionary<string, object>();
        eventData.Add("PlayerID", PlayerIdSO.PlayerID);
        eventData.Add("Position", transform.position.ToString());
        eventData.Add("PlayerVelocity", Rb.velocity.magnitude);
        AnalyticsService.Instance.CustomData("PlayerDeath", eventData);
        AnalyticsService.Instance.Flush();
#endif

        yield return new WaitForSeconds(5);
        GameEvents.Die();
        DeathWaitTimer = null;
    }
}
