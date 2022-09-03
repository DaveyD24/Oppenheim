using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player ID Scriptable Object", fileName = "New Player ID SO")]
public class PlayerIdObject : ScriptableObject
{
    [field: SerializeField] public int PlayerID { get; private set; }
}
