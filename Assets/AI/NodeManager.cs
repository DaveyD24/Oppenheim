using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    [SerializeField] float nodeDistance = 2.0f;
    [SerializeField] GameObject nodePrefab;

    float WDistance;
    float HDistance;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] AllNodePlatforms = GameObject.FindGameObjectsWithTag("NodePlatform");

        foreach (GameObject G in AllNodePlatforms)
        {
            BoxCollider Collider = G.GetComponent<BoxCollider>();

            float Width = Collider.bounds.size.x;
            float Height = Collider.bounds.size.z;

            WDistance = Width / nodeDistance;
            WDistance = Mathf.Floor(WDistance);
            WDistance = Width / WDistance;

            HDistance = Height / nodeDistance;
            HDistance = Mathf.Floor(HDistance);
            HDistance = Height / HDistance;

            int HNodeCount = Mathf.CeilToInt(Width / WDistance);
            int VNodeCount = Mathf.CeilToInt(Height / HDistance);

            //Vector3 StartPoint = new Vector3(Width - (Width / 2), G.transform.position.y, Height - (Height / 2));
            Vector3 StartPoint = new Vector3(G.transform.position.x - (Width / 2), G.transform.position.y, G.transform.position.z - (Height / 2));

            Debug.Log(StartPoint);
            GenerateNodes(StartPoint, HNodeCount, VNodeCount);
        }

        MovementNode[] AllNodes = GameObject.FindObjectsOfType<MovementNode>();
        foreach (MovementNode Node in AllNodes)
        {
            Node.ConnectNodes();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateNodes(Vector3 StartPoint, int HorizontalCount, int VerticalCount)
    {
        for (int j = 0; j <= VerticalCount; j++)
        {
            for (int i = 0; i <= HorizontalCount; i++)
            {
                Vector3 Position = new Vector3(StartPoint.x + (i * WDistance), StartPoint.y, StartPoint.z + (j * HDistance));
                Instantiate(nodePrefab, Position, Quaternion.identity);
            }
        }
    }

    public float GetNodeDistance()
    {
        return nodeDistance;
    }


}
