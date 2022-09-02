using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player ID Scriptable Object", fileName = "New Player ID SO")]
public class PlayerIdObject : ScriptableObject
{
    [SerializeField] private int playerID;

    public int PlayerID { get => playerID; private set => playerID = value; }
}
