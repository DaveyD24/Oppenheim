using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPathfinding : MonoBehaviour
{
#if UNITY_EDITOR
    private NavigationNode[,,] grid;
    [SerializeField] private MeshFilter meshFilter;
    [Tooltip("The width of the grid which can be searched")]
    [SerializeField] private int width;
    [Tooltip("The height of the grid which can be searched")]
    [SerializeField] private int height;
    [Tooltip("The depth of the grid which can be searched")]
    [SerializeField] private int depth;
    [Tooltip("The size each tile in the grid")]
    [SerializeField] private int tileSize;
    [SerializeField] private bool bGlobalPosition = false;
    [Tooltip("The offset of the grid from the world origin")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private LayerMask groundLayer;
    [ContextMenuItem("Generate the Grid", "GenerateGrid")]
    [SerializeField] private bool bShowGrid = false;
    [SerializeField] private bool bPathValidCollide = true;

    [ContextMenuItem("Generate a path from this object to the goal", "GenerateWirePath")]
    public GameObject goalObject;

    private List<NavigationNode> openSet = new List<NavigationNode>(); // the list of nodes which have been seen, but not yet evaluated
    private List<NavigationNode> closedSet = new List<NavigationNode>(); // the list of nodes which have been seen, and evaluated

    // Start is called before the first frame update
    public void GenerateGrid()
    {
        grid = new NavigationNode[width, height, depth];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    bool bHit = Physics.CheckSphere(PointToWorld(i, j, k), tileSize * 0.5f, groundLayer);
                    if ((bHit && bPathValidCollide) || (!bHit && !bPathValidCollide))
                    {
                        grid[i, j, k] = new NavigationNode(i, j, k);
                    }
                }
            }
        }
    }

    private void GenerateWirePath()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        List<Vector3> path = APathfinding(transform.position, goalObject.transform.position);
        lineRenderer.positionCount = path.Count;

        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i]);
        }

        lineRenderer.SetPosition(0, goalObject.transform.position);
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, transform.position);

        // CreateMeshPath.MakeCube(meshFilter, path); // temporarily disable and fix up for sprint 4
    }

    private void OnDrawGizmos()
    {
        if (bShowGrid)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        Gizmos.color = Color.red;
                        if (grid.Length > 0 && grid[i, j, k] != null)
                        {
                            Gizmos.DrawCube(PointToWorld(i, j, k), new Vector3(1, 1, 1));
                        }
                    }
                }
            }
        }
    }

    public Vector3 PointToWorld(int x, int y, int z)
    {
        Vector3 pointOffset = TransformOffset();

        return new Vector3(x * tileSize, y * tileSize, z * tileSize) + pointOffset;
    }

    public void WorldToPoint(Vector3 worldPoint)
    {
        worldPoint -= TransformOffset();

        int x = Mathf.CeilToInt(worldPoint.x / tileSize);
        int y = Mathf.CeilToInt(worldPoint.y / tileSize);
        int z = Mathf.CeilToInt(worldPoint.z / tileSize);
    }

    /// <summary>
    /// find a valid node on the grid for the position, checking neighbours if the initially choosen one is not valid.
    /// </summary>
    /// <param name="position">the position checking, either being the paths start or end.</param>
    /// <returns>A position on the grid.</returns>
    public (int, int, int) FindValidCell(Vector3 position)
    {
        position -= TransformOffset();
        int gridX = Mathf.Clamp(Mathf.RoundToInt(position.x / tileSize), 0, width - 1);
        int gridY = Mathf.Clamp(Mathf.RoundToInt(position.y / tileSize), 0, height - 1);
        int gridZ = Mathf.Clamp(Mathf.RoundToInt(position.z / tileSize), 0, depth - 1);

        if (grid[gridX, gridY, gridZ] == null)
        {
            int currNeighbourOut = 1;

            // check within a 10 tile range of the current cell inside of
            while (currNeighbourOut <= 1)
            {// not below is quite inefficent as need to check all cells of the ones before it already checked
                // for every possible neighbour see if it would be valid instead
                for (int i = -currNeighbourOut; i <= currNeighbourOut; i++)
                {
                    for (int j = -currNeighbourOut; j <= currNeighbourOut; j++)
                    {
                        for (int k = -currNeighbourOut; k <= currNeighbourOut; k++)
                        {
                            NavigationNode neighbour;
                            if (gridX + i >= 0 && gridX + i < width && gridY + j >= 0 && gridY + j < height
                                && gridZ + k >= 0 && gridZ + k < depth)
                            {
                                neighbour = grid[gridX + i, gridY + j, gridZ + k];

                                // as long as this is a valid node, return it
                                if (neighbour != null)
                                {
                                    return (gridX + i, gridY + j, gridZ + k);
                                }
                            }
                        }
                    }
                }

                currNeighbourOut++;
            }

        }

        return (gridX, gridY, gridZ);
    }

    public List<Vector3> APathfinding(Vector3 startPosition, Vector3 targetPosition)
    {
        ResetPathFinding();

        // convert the world positions to the tiles on the grid
        (int targetGridX, int targetGridY, int targetGridZ) = FindValidCell(targetPosition);
        (int gridX, int gridY, int gridZ) = FindValidCell(startPosition);

        // Debug.Log(gridX + " " + gridY + " " + gridZ);
        if (grid[targetGridX, targetGridY, targetGridZ] != null && grid[gridX, gridY, gridZ] != null)
        {
            grid[gridX, gridY, gridZ].gCost = 0;
            grid[gridX, gridY, gridZ].hCost = DistToTarget(new Vector3(gridX, gridY, gridZ), new Vector3(targetGridX, targetGridY, targetGridZ));
            openSet.Add(grid[gridX, gridY, gridZ]); // add the start node to the open set

            while (openSet.Count > 0)
            {
                // find the node with the lowest fscore as its the one which is likly closest to the goal
                NavigationNode currentNode = LowestFCost(); // the current node at in the grid
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                currentNode.isClosed = true;
                currentNode.isOpen = false;

                if ((currentNode.x == targetGridX && currentNode.y == targetGridY && currentNode.z == targetGridZ)) // || closedSet.Count >= 5)
                {
                    return RecalculatePath(currentNode.x, currentNode.y, currentNode.z, gridX, gridY, gridZ); // break out as have reached the goal
                }

                List<NavigationNode> neighbourNodes = new List<NavigationNode>();// every valid neighbour of the current node
                for (int i = -1; i <= 1; i++) // for every possible neighbour add it
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        for (int k = -1; k <= 1; k++)
                        {
                            NavigationNode neighbour;
                            if (currentNode.x + i >= 0 && currentNode.x + i < width && currentNode.y + j >= 0 && currentNode.y + j < height
                                && currentNode.z + k >= 0 && currentNode.z + k < depth) // ensure checked node is inside the grid
                                                                                       // && currentNode.x + i != currentNode.x && currentNode.y + j != currentNode.y && currentNode.z + k != currentNode.z) //ensure that node checking is also not the current node itself
                            {
                                neighbour = grid[currentNode.x + i, currentNode.y + j, currentNode.z + k];

                                // as long as valid to actually move to that node
                                if (neighbour != null && !neighbour.isClosed)
                                {
                                    neighbourNodes.Add(neighbour);
                                }
                            }
                        }
                    }
                }

                // as now have a valid list of neighbour nodes can loop through them
                foreach (NavigationNode neighbour in neighbourNodes)
                {
                    float tentativeGCost = currentNode.gCost + DistToTarget(new Vector3(currentNode.x, currentNode.y, currentNode.z), new Vector3(neighbour.x, neighbour.y, neighbour.z)); // the new gCost if this node is used

                    // if it the new node is closer or this neighbour not yet seen
                    if (!neighbour.isOpen || tentativeGCost < currentNode.gCost) 
                    {
                        neighbour.gCost = tentativeGCost; // update nodes values
                        neighbour.hCost = DistToTarget(new Vector3(neighbour.x, neighbour.y, neighbour.z), new Vector3(targetGridX, targetGridY, targetGridZ));
                        neighbour.parent = currentNode;

                        // add it to the openset if it isn't already
                        if (!neighbour.isOpen)
                        {
                            openSet.Add(neighbour);
                            neighbour.isOpen = true;
                        }
                    }
                }
            }
        }

        // Debug.Log("No Valid Path found");
        return new List<Vector3>();
    }

    public NavigationNode LowestFCost()
    {
        NavigationNode current = openSet[0];

        // loop through all current nodes visited to find one with shortest path
        foreach (NavigationNode node in openSet)
        {
            if (node.fCostSet() < current.fCostSet())
            {
                current = node;
            }
        }

        return current; // the node with the current shortest path
    }

    public List<Vector3> RecalculatePath(int targetX, int targetY, int targetZ, int startX, int startY, int startZ)
    {
        List<Vector3> worldPoints = new List<Vector3>();
        NavigationNode current = grid[targetX, targetY, targetZ];
        worldPoints.Add(PointToWorld(current.x, current.y, current.z));

        // while not at the start position yet
        while (current.x != startX || current.y != startY || current.z != startZ) 
        {
            worldPoints.Add(PointToWorld(current.x, current.y, current.z));
            current = current.parent;

            // Debug.Log("generating the Path!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        return worldPoints;
    }

    private float DistToTarget(Vector3 currPos, Vector3 targetPos)
    {
        return Vector3.Distance(currPos, targetPos);
    }

    private void ResetPathFinding()
    {
        openSet.Clear();
        closedSet.Clear();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    // Gizmos.color = Color.red;
                    if (grid.Length > 0 && grid[i, j, k] != null)
                    {
                        grid[i, j, k].isClosed = false;
                        grid[i, j, k].isOpen = false;
                        grid[i, j, k].hCost = -1;
                        grid[i, j, k].gCost = -1;
                    }
                }
            }
        }
    }

    private Vector3 TransformOffset()
    {
        if (bGlobalPosition)
        {
            return offset;
        }
        else
        {
            return transform.position + offset;
        }
    }
#endif
}
