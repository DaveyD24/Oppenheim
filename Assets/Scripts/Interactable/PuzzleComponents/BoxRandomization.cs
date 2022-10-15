using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSystem;

/// <summary>
/// Randomly customizes the breakable and pushable boxes so they can be unique each playthrough.
/// </summary>
public class BoxRandomization : MonoBehaviour
{
    [SerializeField] private Mesh[] textMeshes;
    [SerializeField] private Material[] randomMat;
    [SerializeField] private bool bDoRandMat;
    [SerializeField] private bool bDoRandMesh;
    [SerializeField] private bool bDoRandRotation = false;
    [SerializeField] private MeshFilter[] meshFilters;
    [SerializeField] private MeshRenderer[] meshRenderers;
    private Rigidbody rb;
    [SerializeField] private bool bFreezeXPos = false;
    [SerializeField] private bool bFreezeYPos = false;
    [SerializeField] private bool bFreezeZPos = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (bDoRandMesh)
        {
            int randLetter = Random.Range(0, textMeshes.Length - 1);
            foreach (MeshFilter mesh in meshFilters)
            {
                mesh.mesh = textMeshes[randLetter];
            }
        }

        if (bDoRandMat)
        {
            int randColour = Random.Range(0, randomMat.Length - 1);
            foreach (MeshRenderer renderer in meshRenderers)
            {
                Material[] sharedMat = renderer.sharedMaterials;
                if (sharedMat.Length > 1)
                {
                    sharedMat[1] = randomMat[randColour];
                }
                else
                {
                    sharedMat[0] = randomMat[randColour];
                }

                renderer.sharedMaterials = sharedMat;
            }
        }

        if (bDoRandRotation)
        {
            Quaternion rot = Quaternion.Euler(Random.Range(0, 4) * 90, Random.Range(0, 4) * 90, Random.Range(0, 4) * 90);
            transform.rotation = rot;
        }

        // Physics.IgnoreCollision(GetComponent<Collider>(), solidColliderObj.GetComponent<Collider>());
    }

    /// <summary>
    /// limit the box to only move one direction when pushed.
    /// only limit on the x and z global directions.
    /// </summary>
    private void OneDirectionMove()
    {
        if (rb != null)
        {
            Vector3 velocity = rb.velocity;
            if (velocity.magnitude > 1)
            {
                float xVelocity = Mathf.Abs(rb.velocity.x);
                float zVelocity = Mathf.Abs(rb.velocity.z);

                // determine the direction with the most force and freeze the other
                if (xVelocity > zVelocity)
                {
                    velocity.z = 0;
                    UnsetAllValidPositionConstraints();
                    rb.constraints = RigidbodyConstraints.FreezePositionZ;
                }
                else
                {
                    UnsetAllValidPositionConstraints();
                    rb.constraints = RigidbodyConstraints.FreezePositionX;
                    velocity.x = 0;
                }

                rb.angularVelocity = new Vector3(0, 0, 0);
                rb.velocity = velocity;
            }
            else
            {
                UnsetAllValidPositionConstraints();
            }

            rb.freezeRotation = true;
        }
    }

    private void UnsetAllValidPositionConstraints()
    {
        if (bFreezeXPos)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX;
        }
        else
        {
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        }

        if (bFreezeYPos)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
        }

        if (bFreezeZPos)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ;
        }
        else
        {
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
        }

        if (bFreezeZPos && bFreezeXPos)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionX;
        }
    }

    private void Update()
    {
        OneDirectionMove();

        // Debug.Log(solidColliderObj);
        // solidColliderObj.transform.position = transform.position;
        // solidColliderObj.transform.rotation = transform.rotation;
    }

    // private void OnEnable()
    // {
    //    GameEvents.OnColliderIgnore += IgnoreParentCollisions;
    // }

    // private void OnDisable()
    // {
    //    GameEvents.OnColliderIgnore -= IgnoreParentCollisions;
    // }

    ///// <summary>
    ///// Set this object to ignore physics collisions with all players execpt the car, so that the car is the only one which can move it.
    ///// </summary>
    ///// <param name="colliderIgnoring">The player collider to ignore.</param>
    ///// <param name="bIgnoreChild">Does it ignore the parent(collider with rigidbody) or the child(collider without rigidbody).</param>
    // private void IgnoreParentCollisions(Collider colliderIgnoring, bool bIgnoreChild)
    // {
    //     Collider collider = bIgnoreChild ? GetComponent<Collider>() : solidColliderObj.GetComponent<Collider>();
    //     Physics.IgnoreCollision(collider, colliderIgnoring, true);
    //
    //     Debug.Log("Collision Ignored");
    // }
}
