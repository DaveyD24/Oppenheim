using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : PlayerController
{
    [SerializeField] private List<AxleInfo> axleInfos; // the information about each individual axle

    [SerializeField] private float breakTorque; // maximum torque the motor can apply to wheel

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
        // abiliy
    }

    private void ApplyMovement()
    {
        float motor = MovementSpeed * inputAmount.y;
        float steering = RotationSpeed * inputAmount.x;

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
}