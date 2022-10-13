using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridPathfinding))]
public class GenericBot : MonoBehaviour
{
    private Node<GenericBot> topNode;

    [SerializeField] private GameObject[] pathList;

    [HideInInspector] public Transform NextPosTransform { get; set; }

    [HideInInspector] public List<Vector3> PathToNextPos { get; set; } = new List<Vector3>();

    [HideInInspector] public Vector3 NextPosVector { get; set; }

    [HideInInspector] public GridPathfinding GridPathfindingComp { get; private set; }

    [field: SerializeField] public float MovementSpeed { get; private set; }

    [field: SerializeField] public float RotationSpeed { get; private set; }

    [field: SerializeField] public float GroundOffset { get; private set; }

    [field: SerializeField] public float SightRadius { get; private set; }

    [field: SerializeField] public float SightAngle { get; private set; }

    [field: SerializeField] public LayerMask DetectibleLayers { get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        FindClosestNode();
        GridPathfindingComp = gameObject.GetComponent<GridPathfinding>();
        GridPathfindingComp.GenerateGrid();
        GenerateBehaviourTree();
    }

    // Update is called once per frame
    private void Update()
    {
        if (topNode != null)
        {
            topNode.Execute();
        }
    }

    private void GenerateBehaviourTree()
    {
        GetNextNode getNextNode = new GetNextNode(this);
        MoveTowards moveTowards = new MoveTowards(this);

        topNode = new Sequence<GenericBot>(new List<Node<GenericBot>> { getNextNode, moveTowards });
    }

    private void FindClosestNode()
    {
        if (pathList.Length > 0)
        {
            GameObject closestNode = pathList[0]; // find the next location to move towards
            foreach (GameObject node in pathList)
            {
                if (Vector3.Distance(node.transform.position, transform.position) < Vector3.Distance(closestNode.transform.position, transform.position))
                {
                    closestNode = node;
                }
            }

            NextPosTransform = closestNode.transform;
        }
        else
        {
            Debug.LogError("No valid path assigned to this bot, please assign one");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        foreach (GameObject n in pathList)
        {
            Gizmos.DrawLine(n.GetComponent<NextNode>().NextNodeObj.transform.position, n.transform.position);
            Gizmos.DrawSphere(n.transform.position, 1);
        }
    }
}
