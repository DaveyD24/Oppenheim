using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the actual data for a physics object which needs to be saved.
/// </summary>
[System.Serializable]
public class GeneralPhysicsObjectData
{
    [SerializeField] public Vector3 Position { get; set; }

    [SerializeField] public Vector3 Velocity { get; set; }

    [SerializeField] public Vector3 AngularVelocity { get; set; }

    [SerializeField] public Quaternion Rotation { get; set; }
}
