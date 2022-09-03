using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the values for each axle of the car.
/// </summary>
[System.Serializable]
public class AxleInfo
{
    private TrailRenderer leftSkidTrails;
    private TrailRenderer rightSkidTrails;

    [field: SerializeField] public WheelCollider LeftWheel { get; private set; }

    [field: SerializeField] public WheelCollider RightWheel { get; private set; }

    [field: SerializeField] public bool Motor { get; private set; } // is this wheel attached to motor?

    [field: SerializeField] public bool Steering { get; private set; } // is this wheel attached to motor?

    public GameObject Trail { get; set; }

    public void InitSkidTrails(TrailRenderer leftTrail, TrailRenderer rightTrail)
    {
        leftSkidTrails = leftTrail;
        leftSkidTrails.emitting = false;
        leftSkidTrails.gameObject.transform.parent = null;
        leftSkidTrails.gameObject.transform.position = LeftWheel.transform.position;

        rightSkidTrails = rightTrail;
        rightSkidTrails.emitting = false;
        rightSkidTrails.gameObject.transform.parent = null;
        rightSkidTrails.gameObject.transform.position = RightWheel.transform.position;
    }

    /// <summary>
    /// Play the skid trails on the ground, facing the direction of the grounds normal at that point.
    /// </summary>
    public void SetSkidTrails()
    {
        if (LeftWheel.isGrounded)
        {
            Vector3 leftTrailPos = LeftWheel.transform.position - (LeftWheel.transform.parent.up * LeftWheel.radius * 0.99f);
            if (leftSkidTrails == null || leftSkidTrails.emitting == false)
            {
                leftSkidTrails = MonoBehaviour.Instantiate(Trail, leftTrailPos, Quaternion.identity).GetComponent<TrailRenderer>();
                leftSkidTrails.emitting = true;
            }

            WheelHit leftWheelHit;
            LeftWheel.GetGroundHit(out leftWheelHit);
            leftSkidTrails.gameObject.transform.position = leftTrailPos;
            leftSkidTrails.gameObject.transform.forward = -leftWheelHit.normal;
        }
        else if (leftSkidTrails != null)
        {
            leftSkidTrails.emitting = false;
        }

        if (RightWheel.isGrounded)
        {
            Vector3 rightTrailPos = RightWheel.transform.position - (RightWheel.transform.parent.up * RightWheel.radius * 0.99f);
            if (rightSkidTrails == null || rightSkidTrails.emitting == false)
            {
                rightSkidTrails = MonoBehaviour.Instantiate(Trail, rightTrailPos, Quaternion.identity).GetComponent<TrailRenderer>();
                rightSkidTrails.emitting = true;
            }

            WheelHit rightWheelHit;
            RightWheel.GetGroundHit(out rightWheelHit);
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
