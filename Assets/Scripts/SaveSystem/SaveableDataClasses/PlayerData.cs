using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [SerializeField] public int NumberAbilityLeft { get; set; }

    [SerializeField] public Vector3 Position { get; set; }

    // also solider number of shots fired for the round
}
