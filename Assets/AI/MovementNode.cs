using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Who can travel on this node.
/// </summary>
public enum Access
{
    /// <summary>
    /// Enemy robots
    /// </summary>
    Enemy,

    /// <summary>
    /// Player characters
    /// </summary>
    Player,
}

public class MovementNode : MonoBehaviour
{
    [SerializeField] private List<MovementNode> connectedNodes = new List<MovementNode>();
    private GameObject owner;
    private float nodeDistance = 2.0f;
    private MovementNode nextNode;

    [SerializeField] public int platformIndex; 

    NodeManager nodeManager;

    // Start is called before the first frame update
    private void Awake()
    {
        nodeManager = GameObject.FindObjectOfType<NodeManager>();
        nodeDistance = nodeManager.GetNodeDistance() * 1.5f;
    }


    // Update is called once per frame
    private void Update()
    {
        
    }

    public void ConnectNodes()
    {
        MovementNode[] allNodes = GameObject.FindObjectsOfType<MovementNode>();
        foreach (MovementNode n in allNodes)
        {
            float distance = Vector3.Distance(this.transform.position, n.transform.position);
            if (distance <= nodeDistance && n != this)
            {

                Vector3 Midpoint = (this.transform.position + n.transform.position) / 2;

                Collider[] OverlappingColliders = Physics.OverlapSphere(Midpoint, nodeDistance / 10);

                if (OverlappingColliders.Length == 0)
                {
                    continue;
                }
                else
                {
                    bool doConnect = true;
                    foreach (Collider C in OverlappingColliders)
                    {
                        if (!C.gameObject.CompareTag("NodePlatform") && !C.gameObject.CompareTag("SkyPlatform"))
                        {
                            doConnect = false;
                        }
                    }
                    if (doConnect)
                    {
                        connectedNodes.Add(n);
                    }
                }
            }
        }
    }

    public MovementNode CalculateNextNode()
    {
        if (connectedNodes.Count > 0)
        {
            int nodeCount = connectedNodes.Count;
            int random = Random.Range(0, nodeCount);
            return connectedNodes[random];
        }
        return null;
    }

    public MovementNode GetClosestNodeTo(GameObject Target)
    {
        MovementNode ClosestNode = new MovementNode();
        float smallestDistance = 99999f;

        MovementNode[] allNodes = GameObject.FindObjectsOfType<MovementNode>();
        foreach (MovementNode M in allNodes)
        {
            if (Vector3.Distance(M.transform.position, Target.transform.position) < smallestDistance)
            {
                smallestDistance = Vector3.Distance(M.transform.position, Target.transform.position);
                ClosestNode = M;
            }
        }
        return ClosestNode;
    }

    public static MovementNode GetClosestNode(GameObject Target)
    {
        MovementNode ClosestNode = new MovementNode();
        float smallestDistance = 99999f;

        MovementNode[] allNodes = GameObject.FindObjectsOfType<MovementNode>();
        foreach (MovementNode M in allNodes)
        {
            if (Vector3.Distance(M.transform.position, Target.transform.position) < smallestDistance)
            {
                smallestDistance = Vector3.Distance(M.transform.position, Target.transform.position);
                ClosestNode = M;
            }
        }
        return ClosestNode;
    }

    public MovementNode CalculateNextNodeTowardsPoint(Vector3 Target)
    {
        MovementNode ClosestNode = null;
        if (connectedNodes.Count > 0)
        {
            
            float ShortestDistance = 9999999;
            foreach (MovementNode M in this.connectedNodes)
            {
                if (Vector3.Distance(M.transform.position, Target) < ShortestDistance)
                {
                    ShortestDistance = Vector3.Distance(M.transform.position, Target);
                    ClosestNode = M;
                }
            }
        }
        return ClosestNode;
    }

    public MovementNode GetNextNode()
    {
        CalculateNextNode();
        return nextNode;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (MovementNode n in connectedNodes)
        {
            Gizmos.DrawLine(this.transform.position, n.transform.position);
        }
    }
}
