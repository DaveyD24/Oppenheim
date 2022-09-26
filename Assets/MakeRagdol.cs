using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Run in editor to turn a player into a ragdol.
/// </summary>
public class MakeRagdol : MonoBehaviour
{
    [SerializeField] private Vector3 colliderSize = new Vector3(0.001f, 0.001f, 0.001f);

    private void Start()
    {
        MakeRigidbody(transform, false);
    }

    private void MakeRigidbody(Transform parent, bool bAddHindge = true)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.AddComponent<Rigidbody>();
            child.gameObject.AddComponent<BoxCollider>().size = colliderSize;
            if (bAddHindge)
            {
                HingeJoint hinge = child.gameObject.AddComponent<HingeJoint>();
                hinge.connectedBody = parent.gameObject.GetComponent<Rigidbody>();

                JointLimits limits = hinge.limits;
                limits.min = 0;
                limits.bounciness = 0;
                limits.bounceMinVelocity = 0;
                limits.max = 90;
                hinge.limits = limits;
                hinge.useLimits = true;
            }

            MakeRigidbody(child);
        }
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
