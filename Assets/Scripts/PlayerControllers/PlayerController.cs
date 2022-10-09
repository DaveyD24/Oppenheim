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
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// A base class for handling the common functionality across all players.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(AudioController))]
public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject controlObj;
    [SerializeField] private TextMeshProUGUI abilityTxt;

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

    public int AbilityUses { get; private set; } = 3;

    public AudioController Audio { get; private set; }

    public float Weight { get; private set; }

    [ReadOnly] public EPlayer HumanPlayerIndex = EPlayer.None;

    [field: Header("Inherited from Player Controller")]

    // A unique object each scene object gets assigned, being largly used to store the players id
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }

    [field: SerializeField] protected DefaultPlayerDataObject DefaultPlayerData { get; private set; }

    [field: SerializeField] protected float Bouyancy { get; private set; }

    [field: SerializeField] protected float MovementSpeed { get; private set; }

    [field: SerializeField] protected float RotationSpeed { get; private set; }

    [field: SerializeField] protected float Health { get; private set; }

    [field: SerializeField, Min(25f)] protected float FallDamageThreshold { get; private set; }

    [field: SerializeField] protected Vector3 groundCheckPosition;

    [field: SerializeField] protected float GroundCheckRadius { get; private set; }

    [field: SerializeField] public SpringArm TrackingCamera { get; set; }

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
            bControlsHidden = false;
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
        bControlsHidden = !bControlsHidden;
    }

    public void AdjustAbilityValue(int amount)
    {
        AbilityUses += amount;
        AbilityUses = Mathf.Max(0, AbilityUses);
        abilityTxt.text = AbilityUses.ToString();
    }

    protected virtual void Update()
    {
        if (transform.position.y < 2.5f)
        {
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
        SaveData(null);
        activeIndicator = GetComponentInChildren(typeof(SpriteRenderer)) as SpriteRenderer;
        Rb = GetComponent<Rigidbody>();
        switchManager = FindObjectOfType<SwitchManager>();
        Audio = GetComponent<AudioController>();
        Weight = Rb.mass;
        CurrentFuel = DefaultPlayerData.MaxFuel;

        startPosition = transform.position;
        startRotation = transform.rotation;

        GameEvents.OnAddPlayerSwitch(PlayerIdSO.PlayerID);
        AdjustAbilityValue(0);
    }

    public virtual void OnDeath()
    {
        if (PlayerInput != null)
        {
            PlayerInput.Disable();
        }

        if (DeathWaitTimer == null)
        {
            // Debug.Log("Player Died");
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

        GameEvents.OnSavePlayerData += SaveData;
    }

    protected virtual void OnDisable()
    {
        DeactivateInput(PlayerIdSO.PlayerID);

        GameEvents.OnCollectFuel -= MaxFuel;
        GameEvents.OnDie -= Respawn;

        GameEvents.OnActivatePlayer -= ActivateInput;
        GameEvents.OnDeactivatePlayer -= DeactivateInput;

        GameEvents.OnSavePlayerData -= SaveData;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        if (!Rb)
        {
            Rb = GetComponent<Rigidbody>();
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(GetGroundCheckPosition(), GroundCheckRadius);
        Gizmos.DrawCube(Rb.worldCenterOfMass, Vector3.one * 0.5f);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        bool bTakeFallDamage = ShouldTakeFallDamage(collision, out _);

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
            OnDustParticles(collision.GetContact(0).point);
        }

        if (collision.gameObject.CompareTag("Blueprint"))
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
        Rb.angularVelocity = Vector3.zero;

        if (!Checkpoint.BUseCheckpointPos)
        {
            Rb.transform.position = startPosition;
            transform.rotation = startRotation;
        }
        else
        {
            // calculate the radius around the checkpoint at which the players are to spawn
            Vector3 centrePos = Checkpoint.RespawnPosition;
            float currentAngle = (90 * PlayerIdSO.PlayerID * Mathf.PI) / 180.0f;
            Vector3 playerPos = centrePos + new Vector3(Mathf.Cos(currentAngle) * DefaultPlayerData.RadiusFromCheckpiont, DefaultPlayerData.CheckpointYOffset, Mathf.Sin(currentAngle) * DefaultPlayerData.RadiusFromCheckpiont);

            Rb.transform.position = playerPos;
            transform.rotation = Quaternion.identity;
        }

        if (PlayerInput != null)
        {
            PlayerInput.Enable();
        }

        LoadData();
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
            AdjustAbilityValue(5); // for each fuel collected add 5 ability uses;
            PlayFuelCollectionSound();
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

#pragma warning disable SA1202 // Elements should be ordered by access
    public void LoadData()
#pragma warning restore SA1202 // Elements should be ordered by access
    {
        if (PersistentDataManager.SaveableData.PlayerDatas.Dictionary.ContainsKey(PlayerIdSO.PlayerID))
        {
            PlayerData pData = PersistentDataManager.SaveableData.PlayerDatas.Dictionary[PlayerIdSO.PlayerID];
            AbilityUses = pData.NumberAbilityLeft;
            AdjustAbilityValue(0);
        }
    }

    public void SaveData(int[] fuelDataReset)
    {
        PlayerData pData = new PlayerData();

        if (fuelDataReset != null)
        {
            // determine amount of fuel to ignore when saving, so that on a reset to this checkpoint no extra fuel gets added
            int numInvalidAbilities = fuelDataReset[PlayerIdSO.PlayerID];
            int saveAbilityAmount = AbilityUses - (numInvalidAbilities * 5);
            if (saveAbilityAmount < 3)
            {
                // as only save whenever reach a checkpoint, ensure the player gets enough ability uses
                saveAbilityAmount = 3;
                AbilityUses = 3;
                AdjustAbilityValue(0);
            }

            pData.NumberAbilityLeft = saveAbilityAmount;
        }
        else
        {
            pData.NumberAbilityLeft = AbilityUses;
        }

        pData.Position = transform.position;
        if (PersistentDataManager.SaveableData.PlayerDatas.Dictionary.ContainsKey(PlayerIdSO.PlayerID))
        {
            PersistentDataManager.SaveableData.PlayerDatas.Dictionary[PlayerIdSO.PlayerID] = pData;
        }
        else
        {
            PersistentDataManager.SaveableData.PlayerDatas.Dictionary.Add(PlayerIdSO.PlayerID, pData);
        }
    }

    protected virtual void OnDustParticles(Vector3 Position)
    {
        Instantiate(DefaultPlayerData.DustParticles, Position, Quaternion.identity);
    }

    protected virtual void PlayFuelCollectionSound() { }
}

public enum EPlayer
{
        None = 0,
        P1 = 1,
        P2 = 2,
        P3 = 3,
        P4 = 4,
}
