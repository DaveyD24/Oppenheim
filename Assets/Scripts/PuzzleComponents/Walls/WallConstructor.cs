using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallConstructor : MonoBehaviour
{
    [Header("Individual Segment")]
    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private float depth;
    [SerializeField] private float mass;
    [SerializeField] private float drag;
    [SerializeField] private float angularDrag;

    [Header("Wall Size")]
    [SerializeField] private int numSegmentsX;
    [SerializeField] private int numSegmentsY;
    [SerializeField] private int numSegmentsZ;
    [SerializeField] private float segmentsOffsetX = 0.25f;
    [SerializeField] private float segmentsOffsetY = 0.25f;
    [SerializeField] private float segmentsOffsetZ = 0.25f;
    [SerializeField] private bool bIterateX = false;
    [SerializeField] private bool bIterateZ = false;

    [ContextMenuItem("Generate Wall", "GenerateWall")]
    [SerializeField] private GameObject wallSegment;
    [ContextMenuItem("Generate Wall", "GenerateWall")]
    [SerializeField] private List<GameObject> spawnedObjs = new List<GameObject>();

    // Start is called before the first frame update
    private void Start()
    {
    }

    private void GenerateWall()
    {
        for (int i = spawnedObjs.Count - 1; i >= 0;  i--)
        {
            DestroyImmediate(spawnedObjs[i]);
        }

        spawnedObjs.Clear();

        wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallSegment.transform.localScale = new Vector3(width, height, depth);
        Rigidbody rb = wallSegment.AddComponent<Rigidbody>();
        rb.mass = mass;
        rb.angularDrag = angularDrag;
        rb.drag = drag;

        Vector3 pos = transform.position;
        float offsetX = width * numSegmentsX / 2;
        float offsetY = height * numSegmentsY / 2;
        float offsetZ = depth * numSegmentsZ / 2;
        for (int y = 0; y < numSegmentsY; y++)
        {
            for (int z = 0; z < numSegmentsZ; z++)
            {
                float zoffset;
                if (y % 2 == 0 && bIterateZ)
                {
                    zoffset = depth / 2.0f;
                }
                else
                {
                    zoffset = 0;
                }

                for (int x = 0; x < numSegmentsX; x++)
                {
                    float xoffset = 0;
                    if ((z + y) % 2 == 0 && bIterateX)
                    {
                        xoffset = width / 2.0f;
                    }
                    else
                    {
                        xoffset = 0;
                    }

                    Vector3 segmentPos = new Vector3(xoffset + (x * width) + pos.x + (segmentsOffsetX * x) - offsetX, (y * height) + pos.y + (segmentsOffsetY * y) - offsetY, zoffset + (z * depth) + pos.z + (segmentsOffsetZ * z) - offsetZ);
                    GameObject segment = Instantiate(wallSegment, segmentPos, Quaternion.identity, transform);
                    segment.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                    spawnedObjs.Add(segment);
                }
            }
        }
    }
}
