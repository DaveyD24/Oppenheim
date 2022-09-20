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
    [SerializeField] private bool bRandomColour = false;

    [Header("Wall Size")]
    [SerializeField] private int numSegmentsX;
    [SerializeField] private int numSegmentsY;
    [SerializeField] private int numSegmentsZ;

    [SerializeField] private float xoffsetAmount;
    [SerializeField] private float yoffsetAmount;
    [SerializeField] private float zoffsetAmount;

    [SerializeField] private float segmentsOffsetX = 0.25f;
    [SerializeField] private float segmentsOffsetY = 0.25f;
    [SerializeField] private float segmentsOffsetZ = 0.25f;
    [SerializeField] private bool bIterateX = false;
    [SerializeField] private bool bIterateZ = false;

    [ContextMenuItem("Generate Wall", "GenerateWall")]
    [SerializeField] private GameObject wallSegment;
    [ContextMenuItem("Generate Wall", "GenerateWall")]
    [SerializeField] private List<GameObject> spawnedObjs = new List<GameObject>();

    [Space(30)]
    [Header("ShelveItems")]
    [SerializeField] private List<GameObject> randomItems = new List<GameObject>();
    [SerializeField] private List<GameObject> spawnedItems = new List<GameObject>();
    [ContextMenuItem("Add Shelve Items", "GenerateShelveItems")]
    [SerializeField] private int numItemsX;
    [ContextMenuItem("Add Shelve Items", "GenerateShelveItems")]
    [SerializeField] private int numItemsZ;
    [SerializeField] private float shelveDepth;
    [SerializeField] private Vector3 itemScale;
    [SerializeField] private Vector3 shelveItemOffset;

    // Start is called before the first frame update
    private void Start()
    {
    }

    private void GenerateShelveItems()
    {
        for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnedItems[i]);
        }

        spawnedItems.Clear();

        for (int i = 0; i < spawnedObjs.Count; i++)
        {
            // as each rack has 5 shelves & they start from child 4 of the parent shelve object
            for (int j = 4; j <= 8; j++)
            {
                Debug.Log(spawnedObjs[i].transform.childCount + "  " + j);
                if (j <= spawnedObjs[i].transform.childCount - 1)
                {
                    Vector3 shelvePos = spawnedObjs[i].transform.GetChild(j).position;
                    for (int x = 0; x < numItemsX; x++)
                    {
                        for (int z = 0; z < numItemsZ; z++)
                        {
                            GameObject randomItem = randomItems[Random.Range(0, randomItems.Count)];
                            Vector3 itemPos = new Vector3(shelvePos.x + (((float)width / (float)numItemsX * x) + shelveItemOffset.x), shelvePos.y + shelveItemOffset.y, shelvePos.z + (((float)shelveDepth / (float)numItemsZ * z) + shelveItemOffset.z));
                            GameObject segment = Instantiate(randomItem, itemPos, randomItem.transform.rotation, spawnedObjs[i].transform.GetChild(j));
                            if (segment.name.Contains("Container"))
                            {
                                segment.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                            }
                            else
                            {
                                segment.transform.localScale = itemScale;
                            }

                            spawnedItems.Add(segment);
                        }
                    }
                }
            }
        }
    }

    private void GenerateWall()
    {
        for (int i = spawnedObjs.Count - 1; i >= 0;  i--)
        {
            DestroyImmediate(spawnedObjs[i]);
        }

        spawnedObjs.Clear();
        wallSegment.transform.localScale = new Vector3(width, height, depth);
        // wallSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // wallSegment.transform.localScale = new Vector3(width, height, depth);
        // Rigidbody rb = wallSegment.AddComponent<Rigidbody>();
        // rb.mass = mass;
        // rb.angularDrag = angularDrag;
        // rb.drag = drag;

        Vector3 pos = transform.position;
        float offsetX = 0; // width * numSegmentsX / 2;
        float offsetY = 0; // height * numSegmentsY / 2;
        float offsetZ = 0; // depth * numSegmentsZ / 2;
        for (int y = 0; y < numSegmentsY; y++)
        {
            for (int z = 0; z < numSegmentsZ; z++)
            {
                float zoffset;
                if (y % 2 == 0 && bIterateZ)
                {
                    zoffset = zoffsetAmount;
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
                        xoffset = xoffsetAmount;
                    }
                    else
                    {
                        xoffset = 0;
                    }

                    Vector3 segmentPos = new Vector3(xoffset + (x * width) + pos.x + (segmentsOffsetX * x) - offsetX, (y * height) + pos.y + (segmentsOffsetY * y) - offsetY, zoffset + (z * depth) + pos.z + (segmentsOffsetZ * z) - offsetZ);
                    GameObject segment = Instantiate(wallSegment, segmentPos, Quaternion.identity, transform);
                    if (bRandomColour)
                    {
                        segment.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
                    }

                    segment.transform.localScale = new Vector3(width, height, depth);
                    spawnedObjs.Add(segment);
                }
            }
        }
    }
}
