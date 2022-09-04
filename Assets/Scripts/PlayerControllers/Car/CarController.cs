using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : PlayerController
{
    [SerializeField] private List<AxleInfo> axleInfos; // the information about each individual axle
    private bool bIsGrounded = true;

    private Node dashTopNode;

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

    [field: Header("Dash Settings")]
    [field: Space(1)]
    [field: SerializeField] public AnimationCurve DashSpeedCurve { get; private set; }

    [field: SerializeField] public AnimationCurve TransitionRotCurve { get; private set; }

    [field: SerializeField] public Vector3 DashOffset { get; private set; }

    [field: SerializeField] public Material DashBodyMaterial { get; private set; }

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

    public bool BAnyWheelGrounded { get; private set; } = true;

    public float Motor { get; set; } = 0; // the current force of the cars motor

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);

        visualWheel.transform.SetPositionAndRotation(position, rotation);
    }

    // car movement is based on this https://docs.unity3d.com/2022.2/Documentation/Manual/WheelColliderTutorial.html
    public void FixedUpdate()
    {
        if (CurrentFuel > 0)
        {
            ApplyMovement();
            AntiFlip();
        }
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
    }

    protected override void Jump(InputAction.CallbackContext ctx)
    {
        // this car cannot jump
    }

    protected override void Movement(InputAction.CallbackContext ctx)
    {
        inputAmount = ctx.ReadValue<Vector2>();
    }

    protected override void PerformAbility(InputAction.CallbackContext ctx)
    {
        // note buggs out and fails if the car's wheels currently are not moving at all, otherwise it is fine
        if (!BIsDash && BAnyWheelGrounded)
        {
            BIsDash = true;
        }
    }

    protected override void Update()
    {
        base.Update();

        bIsGrounded = IsGrounded();
        SetWindParticles();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.LeftWheel != null)
            {
                axleInfo.LeftWheel.transform.parent = null;
                axleInfo.RightWheel.transform.parent = null;

                axleInfo.LeftWheel.gameObject.AddComponent<Rigidbody>().AddForce(250 * -transform.right);
                axleInfo.RightWheel.gameObject.AddComponent<Rigidbody>().AddForce(250 * transform.right);

                Destroy(axleInfo.LeftWheel.gameObject.GetComponent<WheelCollider>());
                Destroy(axleInfo.RightWheel.gameObject.GetComponent<WheelCollider>());
            }
        }
    }

    /// <summary>
    /// Set the values for the wind particles to be based on the cars speed.
    /// </summary>
    private void SetWindParticles()
    {
        ParticleSystem.EmissionModule particleEmission = windParticles.emission;
        ParticleSystem.MainModule mainSettings = windParticles.main;
        float velocityDamped = Rb.velocity.magnitude * particelSpeedMultiplier;

        particleEmission.rateOverDistance = velocityDamped;
        mainSettings.startSpeed = Rb.velocity.magnitude;
    }

    private void ApplyMovement()
    {
        Motor = MovementSpeed * inputAmount.y;
        float steering = RotationSpeed * inputAmount.x;

        if (active)
        {
            PerformDash();
        }

        int numWheelGrounded = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.Steering && active)
            {
                axleInfo.LeftWheel.steerAngle = steering;
                axleInfo.RightWheel.steerAngle = steering;
            }

            if (axleInfo.Motor && active)
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
        if (!BIsDash && Motor == 0 && Mathf.Round(Rb.velocity.magnitude) > 0.25f)
        {
            if (CarMaterials[4] != illumimatedTailLights)
            {
                CarMaterials[4] = illumimatedTailLights;
                BodyMeshRenderer.sharedMaterials = CarMaterials;
            }

            axleInfo.SetSkidTrails();
            axleInfo.RightWheel.brakeTorque = breakTorque;
            axleInfo.LeftWheel.brakeTorque = breakTorque;
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
        if (Vector3.Dot(transform.up, Vector3.up) < 0.9f && bIsGrounded && !IsCarMoving())
        {
            // push the car off the ground so that when it rotates back as it falls down it has no ground friction to worry about
            Rb.AddForce(Vector3.up * popUpForce, ForceMode.Acceleration);
            Rb.angularVelocity = Vector3.zero;

            // identify the up angle to orient the car towards
            Quaternion targetRotation = Quaternion.identity;
            targetRotation.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            // smoothly rotate back up while not being mostly aligned with the up axis
            while (Vector3.Dot(transform.up, Vector3.up) < 0.9f)
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

    /// <summary>
    /// checks if any part of the car is tounching the ground.
    /// </summary>
    /// <returns>if the car is tounching the ground or not.</returns>
    private bool IsGrounded()
    {
        Vector3 centerOffset = transform.position - (transform.up * rayCenterOffset);
        Debug.DrawRay(transform.position - (transform.up * rayCenterOffset), Vector3.down * 5, Color.black);

        return Physics.Raycast(centerOffset, Vector3.down, distCheckForGround);
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

   // private void OnDrawGizmosSelected()
   // {
   //     gizmos.color = color.green;
   //     Gizmos.DrawCube(transform.position, Vector3.one);
   //     Gizmos.color = Color.red;
   // }

    private void BuildDashTree()
    {
        DashTransition dashInit = new DashTransition(this, -1); // play the animation to transition to dashing
        DashPerform dashPerform = new DashPerform(this); // perform the dash ability
        DashTransition dashEnd = new DashTransition(this, 1, true); // play the animation to transition back to normal

        dashTopNode = new Sequence(new List<Node> { dashInit, dashPerform, dashEnd });
    }

    private void ApplyIndicator()
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Breakable") && BIsDash)
        {
            // convert this to the event system
            Destroy(collision.gameObject.GetComponent<MeshCollider>());
            Destroy(collision.gameObject.GetComponent<MeshRenderer>());
            Destroy(collision.gameObject.GetComponent<MeshFilter>());
            for (int i = collision.gameObject.transform.childCount - 1; i >= 0; i--)
            {
                collision.gameObject.transform.GetChild(i).gameObject.AddComponent<Rigidbody>().AddForce(1500 * transform.forward);
                collision.gameObject.transform.GetChild(i).gameObject.AddComponent<MeshCollider>().convex = true;
                collision.gameObject.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().enabled = true;
                collision.gameObject.transform.GetChild(i).parent = null;
            }
        }
    }
}