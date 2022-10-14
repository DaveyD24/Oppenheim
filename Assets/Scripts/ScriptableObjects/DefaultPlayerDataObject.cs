using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Scriptable object holding the editor adjustable paramater values which are consistent across all players.
/// </summary>
[CreateAssetMenu(menuName = "Default Player Data Scriptable Object", fileName = "Default Player Data SO")]
public class DefaultPlayerDataObject : ScriptableObject
{
    [field: Tooltip("X Axis: current fuel left(normalized), Y Axis: amount of fuel to lose at this point(per second)")]
    [field: SerializeField] public AnimationCurve DecreaseFuelAmount { get; private set; }

    [field: Range(0, 1)]
    [field: SerializeField] public float InputDeadZone { get; private set; }

    [field: SerializeField] public GameObject DustParticles { get; private set; }

    [field: SerializeField] public float dustParticlesCollisionSpeed { get; private set; }

    [field: SerializeField] public float CheckpointYOffset { get; private set; }

    [field: SerializeField] public float RadiusFromCheckpiont { get; private set; }

    public float MaxFuel { get; private set; } = 100;
}
