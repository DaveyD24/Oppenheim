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
}
