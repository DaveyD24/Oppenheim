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
    [SerializeField] private MeshFilter[] meshFilters;
    [SerializeField] private MeshRenderer[] meshRenderers;
    private MeshRenderer textRenderer;

    // Start is called before the first frame update
    private void Start()
    {
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
    }
}
