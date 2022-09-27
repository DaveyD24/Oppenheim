using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// not the ideal way of doing it, just done due to lack of time.
/// </summary>
public class IDStuff : MonoBehaviour
{
    [field: SerializeField] public PlayerIdObject PlayerIdSO { get; private set; }
}
