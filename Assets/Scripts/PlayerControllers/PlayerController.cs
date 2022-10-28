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
    [Tooltip("When on the tutorial level do not give three ability used so need to check when it is")]
    [SerializeField] private bool bIsTutorialLevel = false;

    // [SerializeField] private GameObject controlObj;
    [SerializeField] private GameObject abilityActiveObj;
    [SerializeField] private TextMeshProUGUI abilityTxt;
    [SerializeField] private Canvas playerCanvas;

    private bool bControlsHidden = false;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float fuel;
    private bool isFarEnoughAway = false;
    private float beforeCollideSpeed;

    private PlayerInput pInput;

    private Vector3 PreviousMouseDragPosition; // used when moving the camera with the mouse

    // the variable for handling moving the camera
    private bool bMouseHeld = false;
    private Vector2 mouseControlInput;
    private float camZoomValue;

    // input handleing things
    public static IEnumerator DeathWaitTimer { get; private set; }

    public InputActionMap PlayerInput { get; private set; }

    [HideInInspector] public bool Active { get; set; } = false;

    public Rigidbody Rb { get; private set; }

    [field: SerializeField] public int AbilityUses { get; private set; } = 3;

    public AudioController Audio { get; private set; }

    public float Weight { get; private set; }

    [ReadOnly] public EPlayer HumanPlayerIndex = EPlayer.None;

    [field: Header("Inherited from Player Controller")]

    // A unique object each scene object gets assigned, being largly used to store the players id
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }

    [field: SerializeField] public DefaultPlayerDataObject DefaultPlayerData { get; private set; }

    [field: SerializeField] protected float Bouyancy { get; private set; }

    [field: SerializeField] protected float MovementSpeed { get; private set; }

    [field: SerializeField] protected float RotationSpeed { get; private set; }

    [field: SerializeField] protected float Health { get; private set; }

    [field: SerializeField, Min(10)] protected float FallDamageThreshold { get; private set; }

    [field: SerializeField] protected Vector3 groundCheckPosition;

    [field: SerializeField] protected float GroundCheckRadius { get; private set; }

#pragma warning disable SA1201 // Elements should appear in the correct order
    [SerializeField] private SpringArm trackingCamera;
#pragma warning restore SA1201 // Elements should appear in the correct order

    [SerializeField] public SpringArm TrackingCamera
    {
        get => trackingCamera;
        set
        {
            trackingCamera = value;
            playerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            playerCanvas.worldCamera = trackingCamera.gameObject.GetComponent<Camera>();
            playerCanvas.planeDistance = 1;
        }
    }

    protected float CurrentFuel { get => fuel; set => fuel = Mathf.Clamp(value, 0, DefaultPlayerData.MaxFuel); }

    SwitchManager switchManager;
    float FollowSpeed = 0.001f;

    MovementNode currentNode;
    MovementNode nextNode;

    float StartTime;
    float EndTime;

    float HeightOffset;

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
            // Debug.Log(groundOverlap[0].gameObject.name + "  " + numberOfValidContacts);
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

        abilityActiveObj.SetActive(true);
    }

    public void Deactivate()
    {
        Active = false;
        UIEvents.CanvasStateChanged(PlayerIdSO.PlayerID, false);

        if (abilityActiveObj != null)
        {
            abilityActiveObj.SetActive(false);
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
            // setup the inputs to use
            pInput = playerInput;
            Inputs = playerInput.actions;

            PlayerInput = Inputs.FindActionMap("Player");

            PlayerInput.FindAction("Move").performed += Movement;
            PlayerInput.FindAction("Move").canceled += Movement;
            PlayerInput.FindAction("Ability").performed += PerformAbility;

            // Inputs.Player.Ability.canceled += PerformAbility;
            PlayerInput.FindAction("Jump").performed += Jump;

            PlayerInput.FindAction("RotatePlayer").performed += RotatePlayer;

            PlayerInput.FindAction("Pause").performed += GamePause;

            // camera inputs
            PlayerInput.FindAction("CamMove").performed += CameraMove;
            PlayerInput.FindAction("CamMove").canceled += CameraMove;
            PlayerInput.FindAction("CamZoom").performed += CameraZoom;
            PlayerInput.FindAction("CamZoom").canceled += CameraZoom;
            // PlayerInput.FindAction("CamFollowRotation").performed += CameraFollowRotation;

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
            PlayerInput.FindAction("Pause").performed -= GamePause;

            // camera inputs
            PlayerInput.FindAction("CamMove").performed -= CameraMove;
            PlayerInput.FindAction("CamMove").canceled -= CameraMove;
            PlayerInput.FindAction("CamZoom").performed -= CameraZoom;
            PlayerInput.FindAction("CamZoom").canceled -= CameraZoom;
            // PlayerInput.FindAction("CamFollowRotation").performed -= CameraFollowRotation;

            PlayerInput.Disable();
            Deactivate();
        }
    }

    // use Vector2 direction = ctx.ReadValue<Vector2>(); to get the values for each direction of movement
    protected abstract void Movement(InputAction.CallbackContext ctx);

    protected abstract void Jump(InputAction.CallbackContext ctx);

    protected abstract void PerformAbility(InputAction.CallbackContext ctx);

    private void GamePause(InputAction.CallbackContext ctx)
    {
        UIEvents.PauseGame();
    }

    private void CameraMove(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.control.name);

        // mouse control
        if (ctx.control.name == "rightButton")
        {
            bMouseHeld = ctx.control.IsPressed();
            Debug.Log(bMouseHeld);

            // when initially pressed set it appropriatly
            PreviousMouseDragPosition = Input.mousePosition;
        }
        else
        {
            // must be using gamepad input
            Vector2 inputAmount = ctx.ReadValue<Vector2>();
            print(inputAmount);
            inputAmount.x = AjustMovementValue(inputAmount.x);
            inputAmount.y = AjustMovementValue(inputAmount.y);

            mouseControlInput = inputAmount;
        }

        if (ctx.canceled)
        {
            GameEvents.CameraMove(gameObject.transform, Vector3.zero, true);
        }
    }

    void LateUpdate()
    {
        PreviousMouseDragPosition = Input.mousePosition;
    }

    private void CamMoveMouse()
    {
        if (bMouseHeld)
        {
            Vector3 inputAmount = Vector3.zero;
            Vector3 mousePosition = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
            inputAmount.x = mousePosition.x - PreviousMouseDragPosition.x;
            inputAmount.y = mousePosition.y - PreviousMouseDragPosition.y;

            GameEvents.CameraMove(gameObject.transform, mousePosition * 60);
        }
        else if (Mathf.Abs(mouseControlInput.x) > 0 || Mathf.Abs(mouseControlInput.y) > 0)
        {
            // if input has been recieved for the controller apply movement to it
            GameEvents.CameraMove(gameObject.transform, mouseControlInput * 3);
        }
    }

    private void CameraZoom(InputAction.CallbackContext ctx)
    {
        camZoomValue = ctx.ReadValue<float>();
    }

    private void CameraFollowRotation(InputAction.CallbackContext ctx)
    {
        GameEvents.CameraFollowRotation(gameObject.transform);
    }

    public void AdjustAbilityValue(int amount)
    {
        AbilityUses += amount;
        AbilityUses = Mathf.Max(0, AbilityUses);
        abilityTxt.text = AbilityUses.ToString();
    }

    protected virtual void Update()
    {
        CamMoveMouse();

        if (!BatMathematics.IsZero(camZoomValue))
        {
            GameEvents.CameraZoom(gameObject.transform, camZoomValue);
        }

        if (transform.position.y < 2.5f)
        {
            OnDeath();
        }

        if (Active)
        {
            // AdjustFuelValue(-DefaultPlayerData.DecreaseFuelAmount.Evaluate(CurrentFuel / DefaultPlayerData.MaxFuel) * Time.deltaTime * DefaultPlayerData.FuelLoseMultiplier);
            //Rb.isKinematic = false;
        }
        else
        {
            if (isFarEnoughAway)
            {
                //Rb.isKinematic = true;
                //MoveToNextNode();
            }
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

    void MoveToNextNode()
    {
        if (currentNode != null)
        {
            //this.transform.position = currentNode.transform.position;
            //Debug.Log("hmmm");


            // this.transform.position = new Vector3(Vector3.Lerp(this.transform.position, nextNode.transform.position, 0.2f).x, 0, Vector3.Lerp(this.transform.position, nextNode.transform.position, 0.2f).z);




            if (Time.time < EndTime)
            {
                //Debug.Log("inwhile");
                if (currentNode != null && nextNode != null)
                {
                    float TimeProgressd = (Time.time - StartTime) / 0.4f;
                    this.transform.position = new Vector3(Vector3.Lerp(currentNode.transform.position, nextNode.transform.position, TimeProgressd).x, currentNode.transform.position.y + HeightOffset, Vector3.Lerp(currentNode.transform.position, nextNode.transform.position, TimeProgressd).z);
                }
                    //Debug.Log(TimeProgressd);
            }
            else
            {
                if (switchManager.GetActivePlayer() != null)
                {
                    currentNode = nextNode;
                    do
                    {
                        nextNode = currentNode.CalculateNextNodeTowardsPoint(switchManager.GetActivePlayer().transform.position);
                    }
                    while (currentNode.CalculateNextNodeTowardsPoint(switchManager.GetActivePlayer().transform.position).platformIndex != currentNode.platformIndex);
                    StartTime = Time.time;
                    EndTime = StartTime + 0.4f;
                }
            }



            //this.transform.position = Vector3.Lerp(currentNode.transform.position, nextNode.transform.position, 0.2f);
            //this.transform.position = Vector3.move

            //currentNode = nextNode;
            //Debug.Log("Part2");
        }
        //yield return new WaitForFixedUpdate();
    }

    protected virtual void Start()
    {
        playerCanvas.gameObject.transform.SetParent(null, false);
        SaveData(null);
        Rb = GetComponent<Rigidbody>();
        switchManager = FindObjectOfType<SwitchManager>();
        Audio = GetComponent<AudioController>();
        Weight = Rb.mass;
        CurrentFuel = DefaultPlayerData.MaxFuel;

        startPosition = transform.position;
        startRotation = transform.rotation;

        GameEvents.OnAddPlayerSwitch(PlayerIdSO.PlayerID);

        if (bIsTutorialLevel)
        {
            AbilityUses = 0;
        }

        AdjustAbilityValue(0);

#if false
                StartTime = Time.time;
        EndTime = StartTime + 2.0f;
        if (currentNode == null)
        {
            currentNode = MovementNode.GetClosestNode(this.gameObject);
        }
        Debug.LogError(currentNode.gameObject.name);
        do
        {
            nextNode = currentNode.CalculateNextNode();
        }    
        while (currentNode.CalculateNextNode().platformIndex != currentNode.platformIndex);
        Debug.LogError(nextNode.gameObject.name);

        if (this as CarController)
        {
            GameObject CarBody = transform.GetChild(1).gameObject;
            HeightOffset = CarBody.GetComponent<MeshCollider>().bounds.size.y / 2.0f;
        }
        else
        {
            HeightOffset = this.GetComponent<BoxCollider>().bounds.size.y / 2.0f;
        }

#endif
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

        GameEvents.OnRespawnPlayersOnly += RespawnPositionSet;
    }

    protected virtual void OnDisable()
    {
        DeactivateInput(PlayerIdSO.PlayerID);

        GameEvents.OnCollectFuel -= MaxFuel;
        GameEvents.OnDie -= Respawn;

        GameEvents.OnActivatePlayer -= ActivateInput;
        GameEvents.OnDeactivatePlayer -= DeactivateInput;

        GameEvents.OnSavePlayerData -= SaveData;
        GameEvents.OnRespawnPlayersOnly -= RespawnPositionSet;
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
        if (bTakeFallDamage && IsActive())
        {
            TakeFallDamage(/*relativeVelocity*/);
        }

        if (!collision.gameObject.CompareTag("Player") && beforeCollideSpeed > DefaultPlayerData.dustParticlesCollisionSpeed)
        {
            OnDustParticles(collision.GetContact(0).point);
        }

        if (collision.gameObject.CompareTag("Blueprint"))
        {
            UIEvents.SceneChange(collision.gameObject.GetComponent<Blueprint>().NextScene);
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
        RespawnPositionSet();

        if (PlayerInput != null)
        {
            PlayerInput.Enable();
        }

        LoadData();
    }

    /// <summary>
    /// When respawning set the players position to be at the latest checkpoint.
    /// </summary>
    /// <param name="bInactiveOnly">do only players which are inactive and not being controlled respawn?.</param>
    private void RespawnPositionSet(bool bInactiveOnly = false)
    {
        if (!bInactiveOnly || !Active)
        {
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
                float currentAngle = 90 * PlayerIdSO.PlayerID * Mathf.Deg2Rad;
                Vector3 playerPos = centrePos + new Vector3(Mathf.Cos(currentAngle) * DefaultPlayerData.RadiusFromCheckpiont, DefaultPlayerData.CheckpointYOffset, Mathf.Sin(currentAngle) * DefaultPlayerData.RadiusFromCheckpiont);

                Rb.transform.position = playerPos;
                transform.rotation = Quaternion.identity;
            }

            Instantiate(DefaultPlayerData.RespawnParticles, transform.position + Vector3.down, Quaternion.identity);

            if (!bInactiveOnly)
            {
                Audio.Play("Respawn", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
            }
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
            AdjustAbilityValue(5); // for each fuel collected add 5 ability uses;
            PlayFuelCollectionSound();
        }
    }

    private void SetCanvasCamera()
    {

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

        yield return new WaitForSeconds(2);
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
            if (saveAbilityAmount < 3 && !bIsTutorialLevel)
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

    /// <summary>
    /// take an input axis and limit it by a dead zone so that it can have better controllability on a controller.
    /// </summary>
    /// <param name="value">The current amount the specified control has has moved.</param>
    /// <returns>the value one adjusted by the deadzone amount.</returns>
    public float AjustMovementValue(float value)
    {
        if (Mathf.Abs(value) < DefaultPlayerData.InputDeadZone)
        {
            value = 0;
        }
        else
        {
            // below uses the formula new_value = ( (old_value - old_min) / (old_max - old_min) ) * (new_max - new_min) + new_min
            if (value > 0)
            {
                // map the ratio from InputDeadZone - 1 to 0 - 1 for further controllability
                value = (((value - DefaultPlayerData.InputDeadZone) * (1 - 0)) / (1 - DefaultPlayerData.InputDeadZone)) + 0;
                value = Mathf.Clamp(value, 0, 1);
            }
            else
            {
                value = (((value - -1) * (0 - -1)) / (-DefaultPlayerData.InputDeadZone - 1)) + -1;
                value = Mathf.Clamp(value, -1, 0);
            }
        }

        // clamp the value as for some reason the negative direction gives a larger value than the positive one
        //value = Mathf.Clamp(value, -1, 1);

        return value;
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
