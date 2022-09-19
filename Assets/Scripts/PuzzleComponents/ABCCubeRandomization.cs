using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABCCubeRandomization : MonoBehaviour
{
    [SerializeField] private Mesh[] textMeshes;
    [SerializeField] private Material[] randomMat;
    private MeshFilter textMeshFilter;
    private MeshRenderer boxRenderer;
    private MeshRenderer textRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        textMeshFilter = transform.GetChild(0).gameObject.GetComponent<MeshFilter>();
        textRenderer = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
        boxRenderer = gameObject.GetComponent<MeshRenderer>();
        int randColour = Random.Range(0, randomMat.Length - 1);
        int randLetter = Random.Range(0, textMeshes.Length - 1);

        textMeshFilter.mesh = textMeshes[randLetter];
        textRenderer.material = randomMat[randColour];
        Material[] sharedMat = boxRenderer.sharedMaterials;
        sharedMat[1] = randomMat[randColour];
        boxRenderer.sharedMaterials = sharedMat;
    }
}
