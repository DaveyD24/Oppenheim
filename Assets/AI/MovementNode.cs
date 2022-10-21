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
            if (distance <= nodeDistance)
            {

                Vector3 Midpoint = (this.transform.position + n.transform.position) / 2;

                if (Physics.CheckSphere(Midpoint, nodeDistance / 10))
                {
                    connectedNodes.Add(n);
                }
            }
        }
    }

    public void CalculateNextNode()
    {
        int nodeCount = connectedNodes.Count;
        int random = Random.Range(0, nodeCount + 1);
        nextNode = connectedNodes[random];
    }

    public MovementNode GetNextNode()
    {
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
