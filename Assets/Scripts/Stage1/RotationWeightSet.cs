using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// instead of relying on arbitrary physics, define the rotation to apply to an object based on the weight a player provides to it.
/// </summary>
public class RotationWeightSet : MonoBehaviour
{
    private float currentWeight;
    private Dictionary<string, float> objDistAway = new Dictionary<string, float>();

    private void OnTriggerStay(Collider other)
    {
        GameObject obj = other.gameObject.name == "CarBody" ? other.transform.parent.gameObject : other.gameObject;

        if (obj.TryGetComponent(out Rigidbody physics))
        {
            currentWeight += physics.mass;
        }
    }
}
