using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterScript : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("xd");
            //CharacterController soldier = other.GetComponent<CharacterController>();
            SoldierMovement soldier = other.GetComponent<SoldierMovement>();
            soldier.isSwimming = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //CharacterController soldier = other.GetComponent<CharacterController>();
            SoldierMovement soldier = other.GetComponent<SoldierMovement>();
            soldier.isSwimming = false;
        }
    }
}
