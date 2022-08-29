using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController2 : PlayerController
{
    [SerializeField] private List<AxleInfoCustom> axleInfos; // the information about each individual axle

    private Vector2 inputAmount;

    [Header("Anti Roll Settings")]
    [SerializeField] private float popUpForce = 300; // maximum torque the motor can apply to wheel
    [SerializeField] private float antiRollTorque; // maximum torque the motor can apply to wheel
    [SerializeField] private float baseOffset;
    [SerializeField] private float distCheckForGround = 2.5f;

    [SerializeField] private float maxFlippedWait = 1.5f;
    private float flippedTime = 3;

    public void FixedUpdate()
    {
        ApplyMovement();
        AntiFlip();
    }

    protected override void Start()
    {
        base.Start();
        Rb.ResetCenterOfMass();
        Rb.centerOfMass = Rb.centerOfMass - Rb.centerOfMass; // Rb.transform.forward * 0.15f;
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
        // Rb.AddForce(Rb.transform.forward * 10000, ForceMode.Impulse);
        // bIsDash = true;
    }

    private float motor = 0;

    private void ApplyMovement()
    {
        float steering = RotationSpeed * inputAmount.x;
        float motor = inputAmount.y * MovementSpeed;

        foreach (AxleInfoCustom axleInfo in axleInfos)
        {
            if (axleInfo.Steering)
            {
                axleInfo.LeftWheel.transform.localRotation = Quaternion.Euler(0, steering, 0);
                axleInfo.RightWheel.transform.localRotation = Quaternion.Euler(0, steering, 0);
            }

            Debug.Log(motor);

            if (axleInfo.Motor)
            {
                axleInfo.LeftWheel.ApplyAcceleration(motor);
                axleInfo.RightWheel.ApplyAcceleration(motor);
            }

            // do breaking
            if (motor == 0)
            {
                axleInfo.RightWheel.ApplySteeringForce(axleInfo.RightWheel.transform.forward);
                axleInfo.LeftWheel.ApplySteeringForce(axleInfo.RightWheel.transform.forward);
            }
        }
    }

    private async void AntiFlip()
    {
        // checks if the car is on the ground or not
        Vector3 centerOffset = transform.position - (transform.up * baseOffset);
        bool bIsGrounded = Physics.Raycast(centerOffset, Vector3.down, distCheckForGround);
        Debug.DrawRay(transform.position - (transform.up * baseOffset), Vector3.down * 5, Color.black);

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
}