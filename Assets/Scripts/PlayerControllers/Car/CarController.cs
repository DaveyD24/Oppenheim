using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : PlayerController
{
    [SerializeField] private List<AxleInfo> axleInfos; // the information about each individual axle
    [SerializeField] private MeshRenderer meshRenderer;
    private Material[] carMaterials;

    private bool bIsGrounded = true;
    private bool bAnyWheelGrounded = true;

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

    [Header("Dash Settings")]
    [Space(1)]
    [SerializeField] private AnimationCurve dashSpeedCurve;
    [SerializeField] private Vector3 dashOffset;

    [Header("Wind Particles")]
    [Space(1)]
    [SerializeField] private ParticleSystem windParticles;
    [Range(0, 1)]
    [SerializeField] private float particelSpeedMultiplier;
    private float dashCurrentTime = 0;
    private float dashMaxTime = 0;
    private bool bIsDash;
    private bool bDoneDashImpulse = false;

    private Vector2 inputAmount; // current movement and steering input applied

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
        ApplyMovement();
        AntiFlip();
    }

    protected override void Start()
    {
        base.Start();
        dashMaxTime = dashSpeedCurve[dashSpeedCurve.length - 1].time;
        carMaterials = meshRenderer.sharedMaterials;

        foreach (AxleInfo axleInfo in axleInfos)
        {
            axleInfo.Trail = trail;
            axleInfo.InitSkidTrails(Instantiate(trail).GetComponent<TrailRenderer>(), Instantiate(trail).GetComponent<TrailRenderer>());
        }

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
        if (!bIsDash && bAnyWheelGrounded)
        {
            bDoneDashImpulse = false;
            Rb.mass = Weight / 2.5f;
            dashCurrentTime = 0;
            bIsDash = true;
        }
    }

    private void Update()
    {
        bIsGrounded = IsGrounded();
        SetWindParticles();
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
        // dashParticles.emission = particleEmission;
    }

    private void ApplyMovement()
    {
        float motor = MovementSpeed * inputAmount.y;
        float steering = RotationSpeed * inputAmount.x;

        PerformDash(ref motor);

        int numWheelGrounded = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.Steering)
            {
                axleInfo.LeftWheel.steerAngle = steering;
                axleInfo.RightWheel.steerAngle = steering;
            }

            if (axleInfo.Motor)
            {
                axleInfo.LeftWheel.motorTorque = motor;
                axleInfo.RightWheel.motorTorque = motor;
            }

            if (axleInfo.LeftWheel.isGrounded)
            {
                numWheelGrounded++;
            }

            if (axleInfo.RightWheel.isGrounded)
            {
                numWheelGrounded++;
            }

            ApplyBreaking(ref motor, axleInfo);

            ApplyLocalPositionToVisuals(axleInfo.LeftWheel);
            ApplyLocalPositionToVisuals(axleInfo.RightWheel);
        }

        bAnyWheelGrounded = numWheelGrounded > 0;
    }

    private void PerformDash(ref float motor)
    {
        // at least one wheel on the ground
        if (bIsDash)
        {
            motor = dashSpeedCurve.Evaluate(dashCurrentTime);

            if (bAnyWheelGrounded)
            {
                Rb.AddForceAtPosition(Rb.transform.forward * dashSpeedCurve.Evaluate(dashCurrentTime), CalculateDashOffset(), ForceMode.Acceleration);
            }

            dashCurrentTime += Time.fixedDeltaTime;
            if (dashCurrentTime >= dashMaxTime)
            {
                bIsDash = false;
                Rb.mass = Weight;
            }
        }
    }

    /// <summary>
    /// Apply a breaking force to the wheels.
    /// </summary>
    /// <param name="motor">the current power of the engine.</param>
    /// <param name="axleInfo">the axle of the car recieving breaking.</param>
    private void ApplyBreaking(ref float motor, AxleInfo axleInfo)
    {
        // do breaking
        if (!bIsDash && motor == 0 && Mathf.Round(Rb.velocity.magnitude) > 1)
        {
            if (carMaterials[4] != illumimatedTailLights)
            {
                carMaterials[4] = illumimatedTailLights;
                meshRenderer.sharedMaterials = carMaterials;
            }

            axleInfo.SetSkidTrails();
            axleInfo.RightWheel.brakeTorque = breakTorque;
            axleInfo.LeftWheel.brakeTorque = breakTorque;
        }
        else
        {
            if (carMaterials[4] != tailLights)
            {
                carMaterials[4] = tailLights;
                meshRenderer.sharedMaterials = carMaterials;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, Vector3.one);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(CalculateDashOffset(), Vector3.one);
   }

    /// <summary>
    /// offsets the position the dash force is applied to the player at using local space.
    /// </summary>
    /// <returns>the offset position.</returns>
    private Vector3 CalculateDashOffset()
    {
        Vector3 pos = transform.position;
        pos += transform.forward * dashOffset.z;
        pos += transform.right * dashOffset.x;
        pos += transform.up * dashOffset.y;

        return pos;
    }

    private void ApplyIndicator()
    {
        Debug.Log(Mathf.Sign(inputAmount.x) + "signed input value");
        if (inputAmount.x > 0.5f && Mathf.Sign(inputAmount.x) > 0.5f)
        {
            // flick on and off the right indicator
            if (carMaterials[5] != indicatorMat)
            {
                carMaterials[5] = indicatorMat;
                meshRenderer.sharedMaterials = carMaterials;
            }
            else if (carMaterials[5] != normalLightMat)
            {
                carMaterials[5] = normalLightMat;
                meshRenderer.sharedMaterials = carMaterials;
            }

            if (carMaterials[3] != normalLightMat)
            {
                carMaterials[3] = normalLightMat;
                meshRenderer.sharedMaterials = carMaterials;
            }
        }
        else if (Mathf.Sign(inputAmount.x) < -0.5f)
        {
            // flick on and off the left indicator
            if (carMaterials[3] != indicatorMat)
            {
                carMaterials[3] = indicatorMat;
                meshRenderer.sharedMaterials = carMaterials;
            }
            else if (carMaterials[3] != normalLightMat)
            {
                carMaterials[3] = normalLightMat;
                meshRenderer.sharedMaterials = carMaterials;
            }

            if (carMaterials[5] != normalLightMat)
            {
                carMaterials[5] = normalLightMat;
                meshRenderer.sharedMaterials = carMaterials;
            }
        }
        else
        {
            // no indicator pressed
            if (carMaterials[5] != normalLightMat)
            {
                carMaterials[5] = normalLightMat;
                meshRenderer.sharedMaterials = carMaterials;
            }

            if (carMaterials[3] != normalLightMat)
            {
                carMaterials[3] = normalLightMat;
                meshRenderer.sharedMaterials = carMaterials;
            }
        }
    }
}