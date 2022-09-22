using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                    rb.constraints = RigidbodyConstraints.None;
                    rb.constraints = RigidbodyConstraints.FreezePositionZ;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;
                    rb.constraints = RigidbodyConstraints.FreezePositionX;
                    velocity.x = 0;
                }

                rb.angularVelocity = new Vector3(0, 0, 0);
                rb.velocity = velocity;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }

            rb.freezeRotation = true;
        }
    }

    private void Update()
    {
        OneDirectionMove();
    }
}
