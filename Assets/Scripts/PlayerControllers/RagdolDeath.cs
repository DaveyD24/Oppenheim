using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdolDeath : MonoBehaviour
{
    private Dictionary<Rigidbody, Vector3> rigidbodies = new Dictionary<Rigidbody, Vector3>();
    private bool bAddForce = false;

    private void Awake()
    {
        GetRigidbody(transform);
    }

    /// <summary>
    /// get all the components of the ragdoll.
    /// </summary>
    /// <param name="parent">the top gameobject in the higherachy, so can recursivly go to the others.</param>
    private void GetRigidbody(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.gameObject.TryGetComponent(out Rigidbody childRB))
            {
                rigidbodies.Add(childRB, child.localPosition);
            }

            GetRigidbody(child);
        }
    }

    private void OnEnable()
    {
        bAddForce = true;
    }

    private void OnDisable()
    {
        foreach (KeyValuePair<Rigidbody, Vector3> item in rigidbodies)
        {
            // when death is complete and the player is reseting reset the ragdoll as well
            item.Key.gameObject.transform.localPosition = item.Value;
            item.Key.velocity = Vector3.zero;
            item.Key.angularVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (bAddForce)
        {
            foreach (KeyValuePair<Rigidbody, Vector3> item in rigidbodies)
            {
                // for each section of the ragdoll add an upwards force to it
                item.Key.AddForce(Vector3.up * 200, ForceMode.Acceleration);
            }

            bAddForce = false;
        }
    }
}
