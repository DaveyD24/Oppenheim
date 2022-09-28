using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PushableBoxData
{
    [SerializeField] public Vector3 Position { get; set; }

    [SerializeField] public Vector3 Velocity { get; set; }

    [SerializeField] public Vector3 AngularVelocity { get; set; }

    [SerializeField] public Quaternion Rotation { get; set; }
}
