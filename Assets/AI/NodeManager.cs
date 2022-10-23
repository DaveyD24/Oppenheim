using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    [SerializeField] float nodeDistance = 2.0f;
    [SerializeField] GameObject nodePrefab;
    [SerializeField] GameObject botPrefab;
    [SerializeField] public GameObject armedBotPrefab;
    [SerializeField] public GameObject skyBotPrefab;
    MovementNode NodeToSpawnAt;
    MovementNode SkyNodeToSpawnAt;

    float WDistance;
    float HDistance;

    public List<MovementNode> SpawnPoints = new List<MovementNode>();

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] AllNodePlatforms = GameObject.FindGameObjectsWithTag("NodePlatform");

        int counter = 0;
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

            Vector2 RandomxD = GetRandomNodePoint(HNodeCount, VNodeCount);
            List<MovementNode> GeneratedNodes =  GenerateNodes(StartPoint, HNodeCount, VNodeCount, counter, RandomxD);

            foreach (MovementNode M in GeneratedNodes)
            {
                M.ConnectNodes();
            }

            for (int j = 0; j <= VNodeCount; j++)
            {
                for (int i = 0; i <= HNodeCount; i++)
                {
                    if (i == RandomxD.x && j == RandomxD.y)
                    {
                        int choice = Random.Range(0, 2);
                        if (choice == 1)
                        {
                            GameObject Enemy = Instantiate(botPrefab, NodeToSpawnAt.transform.position, Quaternion.identity);
                            Enemy.GetComponent<EnemyBot>().currentNode = NodeToSpawnAt;
                            SpawnPoints.Add(NodeToSpawnAt);
                        }
                        else
                        {
                            GameObject Enemy = Instantiate(armedBotPrefab, NodeToSpawnAt.transform.position, Quaternion.identity);
                            Enemy.GetComponent<ArmedBot>().currentNode = NodeToSpawnAt;
                            SpawnPoints.Add(NodeToSpawnAt);
                        }
                        
                        
                    }
                }
            }


            counter++;
        }

        GameObject[] AllSkyPlatforms = GameObject.FindGameObjectsWithTag("SkyPlatform");

        int counter2 = 0;
        foreach (GameObject G in AllSkyPlatforms)
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

            Vector2 RandomxD = GetRandomNodePoint(HNodeCount, VNodeCount);
            List<MovementNode> GeneratedNodes = GenerateSkyNodes(StartPoint, HNodeCount, VNodeCount, counter2, RandomxD);

            foreach (MovementNode M in GeneratedNodes)
            {
                M.ConnectNodes();
            }

            for (int j = 0; j <= VNodeCount; j++)
            {
                for (int i = 0; i <= HNodeCount; i++)
                {
                    if (i == RandomxD.x && j == RandomxD.y)
                    {

                        GameObject Enemy = Instantiate(skyBotPrefab, SkyNodeToSpawnAt.transform.position, Quaternion.identity);
                        Enemy.GetComponent<SkyBot>().currentNode = SkyNodeToSpawnAt;
                    }
                }
            }


            counter++;
        }



    }

    // Update is called once per frame
    void Update()
    {

    }

    List<MovementNode> GenerateNodes(Vector3 StartPoint, int HorizontalCount, int VerticalCount, int PlatformIndex, Vector2 RandomXD)
    {
        List<MovementNode> NodesGenerated = new List<MovementNode>();
        int nextNameNumber = 0;
        for (int j = 0; j <= VerticalCount; j++)
        {
            for (int i = 0; i <= HorizontalCount; i++)
            {
                Vector3 Position = new Vector3(StartPoint.x + (i * WDistance), StartPoint.y, StartPoint.z + (j * HDistance));
                GameObject NewNode = Instantiate(nodePrefab, Position, Quaternion.identity);
                NewNode.name = "Node" + nextNameNumber;
                MovementNode Mn = NewNode.GetComponent<MovementNode>();
                Mn.platformIndex = PlatformIndex;

                if (i == RandomXD.x && j == RandomXD.y)
                {
                    NodeToSpawnAt = Mn;
                }

                NodesGenerated.Add(Mn);

                nextNameNumber++;
            }
        }
        return NodesGenerated;
    }

    List<MovementNode> GenerateSkyNodes(Vector3 StartPoint, int HorizontalCount, int VerticalCount, int PlatformIndex, Vector2 RandomXD)
    {
        List<MovementNode> NodesGenerated = new List<MovementNode>();
        int nextNameNumber = 0;
        for (int j = 0; j <= VerticalCount; j++)
        {
            for (int i = 0; i <= HorizontalCount; i++)
            {
                Vector3 Position = new Vector3(StartPoint.x + (i * WDistance), StartPoint.y, StartPoint.z + (j * HDistance));
                GameObject NewNode = Instantiate(nodePrefab, Position, Quaternion.identity);
                NewNode.name = "Node" + nextNameNumber;
                MovementNode Mn = NewNode.GetComponent<MovementNode>();
                Mn.platformIndex = PlatformIndex;

                if (i == RandomXD.x && j == RandomXD.y)
                {
                    SkyNodeToSpawnAt = Mn;
                }

                NodesGenerated.Add(Mn);

                nextNameNumber++;
            }
        }
        return NodesGenerated;
    }

    Vector2 GetRandomNodePoint(int HorizontalCount, int VerticalCount)
    {
        int RandH = Mathf.FloorToInt(Random.Range(0, HorizontalCount - 1));
        int RandV = Mathf.FloorToInt(Random.Range(0, VerticalCount - 1));

        return new Vector2(RandH, RandV);
    }

    public float GetNodeDistance()
    {
        return nodeDistance;
    }


}
