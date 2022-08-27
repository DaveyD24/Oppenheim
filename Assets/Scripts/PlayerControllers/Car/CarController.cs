using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : PlayerController
{
    [SerializeField] private List<AxleInfo> axleInfos; // the information about each individual axle

    [SerializeField] private float breakTorque; // maximum torque the motor can apply to wheel

    [Header("Anti Roll Settings")]
    [SerializeField] private float popUpForce = 300; // maximum torque the motor can apply to wheel
    [SerializeField] private float antiRollTorque; // maximum torque the motor can apply to wheel
    [SerializeField] private float baseOffset;
    [SerializeField] private float distCheckForGround = 2.5f;

    [SerializeField] private float maxFlippedWait = 1.5f;
    private float flippedTime = 3;

    private bool bIsDash;

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

    private void ApplyMovement()
    {
        float motor = MovementSpeed * inputAmount.y;
        float steering = RotationSpeed * inputAmount.x;

        // if (bIsDash)
        // {
        //    motor = MovementSpeed * 5000;

        // Rb.AddForce(Rb.transform.forward * 1000, ForceMode.Acceleration);
        //    bIsDash = false;
        // }
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

            // do breaking
            if (motor == 0)
            {
                axleInfo.RightWheel.brakeTorque = breakTorque;
                axleInfo.LeftWheel.brakeTorque = breakTorque;
            }
            else
            {
                axleInfo.RightWheel.brakeTorque = 0;
                axleInfo.LeftWheel.brakeTorque = 0;
            }

            ApplyLocalPositionToVisuals(axleInfo.LeftWheel);
            ApplyLocalPositionToVisuals(axleInfo.RightWheel);
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
}