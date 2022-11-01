using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : PlayerController
{
#if UNITY_EDITOR
    [field: Header("Start Reset")]
    [field: ContextMenuItem("Set Start Transform", "SetStartTransform")]
#pragma warning disable SA1202 // Elements should be ordered by access
    [field: SerializeField] public Vector3 StageStartPosition { get; set; }

    [field: ContextMenuItem("Move to Start", "MoveToStartTransform")]
    [field: SerializeField] public Quaternion StageStartRotation { get; set; }
#pragma warning restore SA1202 // Elements should be ordered by access

#endif

    [SerializeField] public List<AxleInfo> axleInfos; // the information about each individual axle
    private bool bIsGrounded = true;

    private Node<CarController> dashTopNode;

    [Header("Steering")]
    [Space(1)]
    [SerializeField] private Material indicatorMat;
    [SerializeField] private Material normalLightMat;
    [SerializeField] private float indicatorFrequency;

    [Header("Breaking")]
    [Space(1)]
    [SerializeField] private float breakTorque; // maximum break torque which can be applied
    [SerializeField] private Material tailLights;
    [SerializeField] private Material illumimatedTailLights;
    [SerializeField] private GameObject trail;

    [Header("Anti Roll Settings")]
    [Space(1)]
    [SerializeField] private float popUpForce = 300; // maximum torque the motor can apply to wheel
    [SerializeField] private float antiRollTorque; // maximum torque the motor can apply to wheel
    [SerializeField] private float rayCenterOffset;
    [SerializeField] private float distCheckForGround = 2.5f;

    [SerializeField] private float maxFlippedWait = 1.5f;
    private float flippedTime = 3;

    private AudioSource engineAudio;

    [field: Header("Dash Settings")]
    [field: Space(1)]
    [field: SerializeField] public LayerMask PlayerLayer { get; private set; }

    [field: SerializeField] public AnimationCurve DashSpeedCurve { get; private set; }

    [field: SerializeField] public AnimationCurve TransitionRotCurve { get; private set; }

    [field: SerializeField] public Vector3 DashOffset { get; private set; }

    [field: SerializeField] public Material DashBodyMaterial { get; private set; }

    [field: SerializeField] public float DashGroundCheckLength { get; private set; } = 2;

    [SerializeField] public bool BCancelDash { get; set; } = false;

    [Header("Wind Particles")]
    [Space(1)]
    [SerializeField]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Order would get messed up making it confusiong.")]
    private ParticleSystem windParticles;
    [Range(0, 1)]
    [SerializeField] private float particelSpeedMultiplier;

    private Vector2 inputAmount; // current movement and steering input applied

    public MeshRenderer BodyMeshRenderer { get; set; }

    public Material[] CarMaterials { get; set; }

    public Transform BodyTransform { get; private set; }

    public bool BIsDash { get; set; }

    public bool BAllowEndBreaking { get; set; } = false;

    public bool BAnyWheelGrounded { get; private set; } = true;

    public float Motor { get; set; } = 0; // the current force of the cars motor

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0 || !collider.gameObject.activeSelf)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        visualWheel.transform.SetPositionAndRotation(position, rotation);
    }

    // car movement is based on this https://docs.unity3d.com/2022.2/Documentation/Manual/WheelColliderTutorial.html
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Active)
        {
            inputAmount = Vector2.zero;
        }

        if (CurrentFuel > 0)
        {
            ApplyMovement();
            AntiFlip();
        }
    }

    /// <summary>
    /// checks if any part of the car is tounching the ground.
    /// </summary>
    /// <returns>if the car is tounching the ground or not.</returns>
    public override bool IsGrounded()
    {
        Vector3 centerOffset = transform.position - (transform.up * rayCenterOffset);
        Debug.DrawRay(transform.position - (transform.up * rayCenterOffset), Vector3.down * 5, Color.black);

        return Physics.Raycast(centerOffset, Vector3.down, distCheckForGround);
    }

    protected override void Start()
    {
        base.Start();
        BodyMeshRenderer = transform.GetChild(1).gameObject.GetComponent<MeshRenderer>();
        CarMaterials = BodyMeshRenderer.sharedMaterials;

        BodyTransform = transform.GetChild(1);

        foreach (AxleInfo axleInfo in axleInfos)
        {
            axleInfo.Trail = trail;
            axleInfo.InitSkidTrails(Instantiate(trail).GetComponent<TrailRenderer>(), Instantiate(trail).GetComponent<TrailRenderer>());
        }

        BuildDashTree();

        windParticles.Play();

        InvokeRepeating("ApplyIndicator", indicatorFrequency, indicatorFrequency);

        PlayEngineSounds();
    }

    protected override void Jump(InputAction.CallbackContext ctx)
    {
        // this car cannot jump
    }

    protected override void Movement(InputAction.CallbackContext ctx)
    {
        inputAmount = ctx.ReadValue<Vector2>();
        inputAmount.x = AjustMovementValue(inputAmount.x);
        inputAmount.y = AjustMovementValue(inputAmount.y);
        if (ctx.control.device.name != "Keyboard")
        {
            inputAmount.y /= 5;
            inputAmount.x /= 5;
        }

        PlayEngineSounds();
    }

    protected override void PerformAbility(InputAction.CallbackContext ctx)
    {
        // note buggs out and fails if the car's wheels currently are not moving at all, otherwise it is fine
        if (AbilityUses > 0 && !BIsDash && BAnyWheelGrounded && Active)
        {
            BIsDash = true;
            AdjustAbilityValue(-1);

            Audio.PlayUnique("Rev", EAudioPlayOptions.FollowEmitter | EAudioPlayOptions.DestroyOnEnd);
        }
    }

    protected override void Update()
    {
        base.Update();

        bIsGrounded = IsGrounded();
        SetWindParticles();
    }

    public override void OnDeath()
    {
        base.OnDeath();
        inputAmount = Vector2.zero;
        if (BIsDash)
        {
            BCancelDash = true;
        }

        foreach (AxleInfo axleInfo in axleInfos)
        {
            axleInfo.LeftWheel.gameObject.SetActive(false);
            axleInfo.RightWheel.gameObject.SetActive(false);

            axleInfo.LeftWheelDeath.SetActive(true);
            axleInfo.RightWheelDeath.SetActive(true);

            axleInfo.LeftWheelDeath.transform.parent = null;
            axleInfo.LeftWheelDeath.transform.position = axleInfo.LeftWheel.gameObject.transform.position;
            axleInfo.LeftWheelDeath.transform.rotation = axleInfo.LeftWheel.gameObject.transform.rotation;

            axleInfo.RightWheelDeath.transform.parent = null;
            axleInfo.RightWheelDeath.transform.position = axleInfo.RightWheel.gameObject.transform.position;
            axleInfo.RightWheelDeath.transform.rotation = axleInfo.RightWheel.gameObject.transform.rotation;

            // axleInfo.LeftWheelDeath.GetComponent<Rigidbody>().AddForce(100 * -transform.right);
            // axleInfo.RightWheelDeath.GetComponent<Rigidbody>().AddForce(100 * transform.right);
        }

        Audio.Play("Death", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(DashOffset + transform.position, Vector3.one * .25f);
        Gizmos.color = Color.red;

        Gizmos.DrawLine(transform.position, transform.position + (Vector3.down * DashGroundCheckLength));
    }

    protected override void Respawn()
    {
        base.Respawn();
        inputAmount = Vector2.zero;
        if (BIsDash)
        {
            BCancelDash = true;
        }

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.LeftWheel != null)
            {
                axleInfo.LeftWheel.gameObject.SetActive(true);
                axleInfo.RightWheel.gameObject.SetActive(true);

                axleInfo.LeftWheelDeath.SetActive(false);
                axleInfo.RightWheelDeath.SetActive(false);

                axleInfo.LeftWheelDeath.GetComponent<Rigidbody>().velocity = Vector3.zero;
                axleInfo.LeftWheelDeath.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

                axleInfo.RightWheelDeath.GetComponent<Rigidbody>().velocity = Vector3.zero;
                axleInfo.RightWheelDeath.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (collision.gameObject.TryGetComponent(out BreakableObj breakableObj) && BIsDash)
        {
            breakableObj.OnBreak();
        }

        if (collision.gameObject.CompareTag("Wall") && BIsDash)
        {
            Vector3 boxLevelPos = new Vector3(transform.position.x, collision.gameObject.transform.position.y, transform.position.z);
            Vector3 direction = (collision.gameObject.transform.position - boxLevelPos).normalized;
            collision.gameObject.GetComponent<PushableBox>().ApplyMovementForce(direction);
            BCancelDash = true;

            Audio.Play(RandomPushableSound(), EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
        }
    }

    protected override void PlayFuelCollectionSound()
    {
        Audio.Play("Rev", EAudioPlayOptions.Global | EAudioPlayOptions.DestroyOnEnd);
    }

    /// <summary>
    /// Set the values for the wind particles to be based on the cars speed.
    /// </summary>
    private void SetWindParticles()
    {
        ParticleSystem.EmissionModule particleEmission = windParticles.emission;
        ParticleSystem.MainModule mainSettings = windParticles.main;
        float velocityDamped = Rb.velocity.magnitude * particelSpeedMultiplier;
        if (BIsDash)
        {
            velocityDamped *= 5;
        }

        particleEmission.rateOverDistance = velocityDamped;
        mainSettings.startSpeed = Rb.velocity.magnitude;
    }

    private void ApplyMovement()
    {
        Motor = MovementSpeed * inputAmount.y;
        float steering = BIsDash ? 0 : RotationSpeed * inputAmount.x;

        if (Active)
        {
            PerformDash();
        }

        int numWheelGrounded = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.LeftWheel != null && axleInfo.RightWheel != null)
            {
                if (axleInfo.Steering && Active)
                {
                    axleInfo.LeftWheel.steerAngle = steering;
                    axleInfo.RightWheel.steerAngle = steering;
                }

                if (axleInfo.Motor && Active)
                {
                    axleInfo.LeftWheel.motorTorque = Motor;
                    axleInfo.RightWheel.motorTorque = Motor;
                }

                if (axleInfo.LeftWheel.isGrounded)
                {
                    numWheelGrounded++;
                }

                if (axleInfo.RightWheel.isGrounded)
                {
                    numWheelGrounded++;
                }

                ApplyBreaking(axleInfo);

                ApplyLocalPositionToVisuals(axleInfo.LeftWheel);
                ApplyLocalPositionToVisuals(axleInfo.RightWheel);
            }
        }

        BAnyWheelGrounded = numWheelGrounded > 0;
    }

    private void PerformDash()
    {
        if (BIsDash)
        {
            dashTopNode.Evaluate();
        }
    }

    /// <summary>
    /// Apply a breaking force to the wheels.
    /// </summary>
    /// <param name="axleInfo">the axle of the car recieving breaking.</param>
    private void ApplyBreaking(AxleInfo axleInfo)
    {
        // do breaking
        if ((!BIsDash || BAllowEndBreaking) && Motor == 0)
        {
            if (Rb.velocity.magnitude > 0.25f)
            {
                if (CarMaterials[4] != illumimatedTailLights)
                {
                    CarMaterials[4] = illumimatedTailLights;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }

                axleInfo.RightWheel.brakeTorque = breakTorque;
                axleInfo.LeftWheel.brakeTorque = breakTorque;
            }

            if (Rb.velocity.magnitude > 3.45f)
            {
                axleInfo.SetSkidTrails();
                if (BAnyWheelGrounded)
                {
                    Audio.PlayUnique("Skid", EAudioPlayOptions.FollowEmitter | EAudioPlayOptions.DestroyOnEnd);
                }
            }
        }
        else
        {
            if (CarMaterials[4] != tailLights)
            {
                CarMaterials[4] = tailLights;
                BodyMeshRenderer.sharedMaterials = CarMaterials;
            }

            axleInfo.ActivateSkidTrails(false);

            axleInfo.RightWheel.brakeTorque = 0;
            axleInfo.LeftWheel.brakeTorque = 0;
        }
    }

    private async void AntiFlip()
    {
        if (Vector3.Dot(transform.up, Vector3.up) < 0.75f && bIsGrounded && !IsCarMoving())
        {
            // push the car off the ground so that when it rotates back as it falls down it has no ground friction to worry about
            Rb.AddForce(Vector3.up * popUpForce, ForceMode.Acceleration);
            Rb.angularVelocity = Vector3.zero;

            // identify the up angle to orient the car towards
            Quaternion targetRotation = Quaternion.identity;
            targetRotation.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            // smoothly rotate back up while not being mostly aligned with the up axis
            while (this != null && Vector3.Dot(transform.up, Vector3.up) < 0.9f)
            {
                Rb.rotation = Quaternion.RotateTowards(Rb.rotation, targetRotation, antiRollTorque * Time.deltaTime);
                await System.Threading.Tasks.Task.Yield();
            }

            Rb.rotation = targetRotation;
        }
    }

    /// <summary>
    /// if the car is moving less than XXX amount for XXX flippedTime seconds, then the car is no longer moving.
    /// </summary>
    /// <returns>if the car is moving or not.</returns>
    private bool IsCarMoving()
    {
        if (Rb.velocity.magnitude <= 0.5f)
        {
            flippedTime -= Time.deltaTime;
            if (flippedTime <= 0)
            {
                flippedTime = maxFlippedWait;
                return false;
            }
        }
        else if (flippedTime < maxFlippedWait)
        {
            flippedTime = maxFlippedWait;
        }

        return true;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 250, 250), $"Speed: {CalculateSpeed():F1} km/h");
        GUI.Label(new Rect(10, 30, 250, 250), $"Current Up Direction Offset: {Vector3.Dot(transform.up, Vector3.up):F1}");
    }
#endif

    private float CalculateSpeed()
    {
        // speed is in units/second and as 1 unit is 1 meter it comes to meters/second so need to multiply by 3.6 to get km/h
        return Rb.velocity.magnitude * 3.6f;
    }

    private void BuildDashTree()
    {
        DashTransition dashInit = new DashTransition(this, -1); // play the animation to transition to dashing
        DashPerform dashPerform = new DashPerform(this); // perform the dash ability
        DashTransition dashEnd = new DashTransition(this, 1, true); // play the animation to transition back to normal

        dashTopNode = new Sequence<CarController>(new List<Node<CarController>> { dashInit, dashPerform, dashEnd });
    }

    private void ApplyIndicator()
    {
        if (Active)
        {
            if (inputAmount.x > 0.5f)
            {
                // flick on and off the right indicator
                if (CarMaterials[5] != indicatorMat)
                {
                    CarMaterials[5] = indicatorMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }
                else if (CarMaterials[5] != normalLightMat)
                {
                    CarMaterials[5] = normalLightMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }

                if (CarMaterials[3] != normalLightMat)
                {
                    CarMaterials[3] = normalLightMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }
            }
            else if (inputAmount.x < -0.5f)
            {
                // flick on and off the left indicator
                if (CarMaterials[3] != indicatorMat)
                {
                    CarMaterials[3] = indicatorMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }
                else if (CarMaterials[3] != normalLightMat)
                {
                    CarMaterials[3] = normalLightMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }

                if (CarMaterials[5] != normalLightMat)
                {
                    CarMaterials[5] = normalLightMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }
            }
            else
            {
                // no indicator pressed
                if (CarMaterials[5] != normalLightMat)
                {
                    CarMaterials[5] = normalLightMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }

                if (CarMaterials[3] != normalLightMat)
                {
                    CarMaterials[3] = normalLightMat;
                    BodyMeshRenderer.sharedMaterials = CarMaterials;
                }
            }
        }
    }

    private void PlayEngineSounds()
    {
	    Audio.PlayUnique(RandomIdleSound(), EAudioPlayOptions.FollowEmitter);
    }

    private string RandomIdleSound()
    {
        bool bRandomBool = Random.Range(0f, 1f) < .5f;

        return bRandomBool ? "Engine Idle 2" : "Engine Idle 1";
    }

    private string RandomPushableSound()
    {
        bool bRandomBool = Random.Range(0f, 1f) < .5f;

        return bRandomBool ? "Hit Pushable 1" : "Hit Pushable 2";
    }
#if UNITY_EDITOR
    private void SetStartTransform()
    {
        StageStartPosition = transform.position;
        StageStartRotation = transform.rotation;
    }

    private void MoveToStartTransform()
    {
        transform.rotation = StageStartRotation;
        transform.position = StageStartPosition;
    }
#endif
}