using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the values for each axle of the car.
/// </summary>
[System.Serializable]
public class AxleInfo
{
    [SerializeField] private WheelCollider leftWheel;
    [SerializeField] private WheelCollider rightWheel;
    [SerializeField] private bool motor; // is this wheel attached to motor?
    [SerializeField] private bool steering; // is this wheel attached to motor?

    private TrailRenderer leftSkidTrails;
    private TrailRenderer rightSkidTrails;

    public GameObject Trail { get; set; }

    public void InitSkidTrails(TrailRenderer leftTrail, TrailRenderer rightTrail)
    {
        leftSkidTrails = leftTrail;
        leftSkidTrails.emitting = false;
        leftSkidTrails.gameObject.transform.parent = null;
        leftSkidTrails.gameObject.transform.position = leftWheel.transform.position;

        rightSkidTrails = rightTrail;
        rightSkidTrails.emitting = false;
        rightSkidTrails.gameObject.transform.parent = null;
        rightSkidTrails.gameObject.transform.position = rightWheel.transform.position;
    }

    public WheelCollider LeftWheel
    {
        get => leftWheel; private set => leftWheel = value;
    }

    public WheelCollider RightWheel
    {
        get => rightWheel; private set => rightWheel = value;
    }

    public bool Motor // is this wheel attached to motor?
    {
        get => motor; private set => motor = value;
    }

    public bool Steering // does this wheel apply steer angle?
    {
        get => steering; private set => steering = value;
    }

    /// <summary>
    /// Play the skid trails on the ground, facing the direction of the grounds normal at that point.
    /// </summary>
    public void SetSkidTrails()
    {
        if (leftWheel.isGrounded)
        {
            Vector3 leftTrailPos = leftWheel.transform.position - (leftWheel.transform.parent.up * leftWheel.radius * 0.99f);
            if (leftSkidTrails == null || leftSkidTrails.emitting == false)
            {
                leftSkidTrails = MonoBehaviour.Instantiate(Trail, leftTrailPos, Quaternion.identity).GetComponent<TrailRenderer>();
                leftSkidTrails.emitting = true;
            }

            WheelHit leftWheelHit;
            leftWheel.GetGroundHit(out leftWheelHit);
            leftSkidTrails.gameObject.transform.position = leftTrailPos;
            Debug.Log(leftWheelHit.point);
            leftSkidTrails.gameObject.transform.forward = -leftWheelHit.normal;
        }
        else if (leftSkidTrails != null)
        {
            leftSkidTrails.emitting = false;
        }

        if (rightWheel.isGrounded)
        {
            Vector3 rightTrailPos = rightWheel.transform.position - (rightWheel.transform.parent.up * leftWheel.radius * 0.99f);
            if (rightSkidTrails == null || rightSkidTrails.emitting == false)
            {
                rightSkidTrails = MonoBehaviour.Instantiate(Trail, rightTrailPos, Quaternion.identity).GetComponent<TrailRenderer>();
                rightSkidTrails.emitting = true;
            }

            WheelHit rightWheelHit;
            rightWheel.GetGroundHit(out rightWheelHit);
            rightSkidTrails.gameObject.transform.position = rightTrailPos;
            rightSkidTrails.gameObject.transform.forward = -rightWheelHit.normal;
        }
        else if (rightSkidTrails != null)
        {
            rightSkidTrails.emitting = false;
        }
    }

    public void ActivateSkidTrails(bool isActive)
    {
        if (leftSkidTrails != null && rightSkidTrails != null)
        {
            leftSkidTrails.emitting = isActive;
            rightSkidTrails.emitting = isActive;
        }
    }
}
