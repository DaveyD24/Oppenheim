using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// turns a supplied path into a 3D mesh.
/// </summary>
public static class CreateMeshPath
{
    public static List<Vector3> newVertices;
    public static Vector2[] newUV;

    public static List<int> newTriangles;

    ///// <summary>
    ///// Remember the quads need all triangles to be in the exact same order.
    ///// </summary>
    //public static Vector3[] testVertices =
    //{
    //    // the front face vertices of the cube
    //    new Vector3(1, 1, 1) *2,
    //    new Vector3(-1, 1, 1)*2,
    //    new Vector3(-1, -1, 1)*2,
    //    new Vector3(1, -1, 1)*2,

    //    // the back face vertices of the cube
    //    new Vector3(-1, 1, -1)*2,
    //    new Vector3(1, 1, -1)*2,
    //    new Vector3(1, -1, -1)*2,
    //    new Vector3(-1, -1, -1)*2,
    //};

    /// <summary>
    /// references to the vertices each triangle uses.
    /// each index gets the appropriate references to the triangles to use.
    /// </summary>
    public static int[][] faceTriangles =
    {
        new int[] { 0, 1, 2, 3 }, // all 4 vertices which will make up the two triangles of the cube
        new int[] { 5, 0, 3, 6 },
        new int[] { 4, 5, 6, 7 },
        new int[] { 1, 4, 7, 2 },
        new int[] { 5, 4, 1, 0 },
        new int[] { 3, 2, 7, 6 },
    };

    public static Vector3[] faceVertices(int dir, Vector3[] currentVertices)
    {
        Vector3[] fv = new Vector3[4];
        for (int i = 0; i < fv.Length; i++)
        {
            fv[i] = currentVertices[faceTriangles[dir][i]];
        }

        return fv;
    }

    public static void MakeCube(MeshFilter meshFilter, List<Vector3> trailPath, float wireSize = 0.1f)
    {
    //    testVertices
    //    {
    //        // the front face vertices of the cube
    //        new Vector3(1, 1, 1),
    //    new Vector3(-1, 1, 1),
    //    new Vector3(-1, -1, 1),
    //    new Vector3(1, -1, 1),

    //    // the back face vertices of the cube
    //    new Vector3(-1, 1, -1),
    //    new Vector3(1, 1, -1),
    //    new Vector3(1, -1, -1),
    //    new Vector3(-1, -1, -1),
    //};

        ////////////////////////for (int i = 0; i < testVertices.Length; i++)
        ////////////////////////{
        ////////////////////////    testVertices[i] += transform.position;
        ////////////////////////}

        newTriangles = new List<int>();
        newVertices = new List<Vector3>();

        for (int p = 0; p < trailPath.Count - 1; p++)
        {

            Vector3 startFacePos = trailPath[p];
            Vector3 otherFacePos = trailPath[p + 1];

            Debug.Log("Cube is made");

            Debug.Log(trailPath[p]);
            Vector3[] testVertices =
        {
        // the front face vertices of the cube
        (new Vector3(1,  1, 1)* wireSize + startFacePos),
        (new Vector3(-1, 1, 1)* wireSize + startFacePos),
        (new Vector3(-1, -1, 1)* wireSize + startFacePos),
        (new Vector3(1, -1, 1)* wireSize + startFacePos),

        // the back face vertices of the cube
        (new Vector3(-1, 1, -1) * wireSize+ otherFacePos),
        (new Vector3(1, 1, -1) * wireSize+ otherFacePos),
        (new Vector3(1, -1, -1)* wireSize + otherFacePos),
        (new Vector3(-1, -1, -1) * wireSize+ otherFacePos),
        };

            // for each of the six faces of a cube
            for (int i = 0; i < 6; i++)
            {
                MakeCubeFace(i, testVertices);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = newVertices.ToArray(); // turns lists into arrays

            // mesh.uv = newUV;
            mesh.triangles = newTriangles.ToArray();

            meshFilter.mesh = mesh;
        }
    }

    public static void MakeCubeFace(int faceIndex, Vector3[] currvertices)
    {
        newVertices.AddRange(faceVertices(faceIndex, currvertices)); // pass in the four vertices.

        newTriangles.Add(newVertices.Count - 4); // dynamically keep track of the vertices count.
        newTriangles.Add(newVertices.Count - 4 + 1);
        newTriangles.Add(newVertices.Count - 4 + 2);

        newTriangles.Add(newVertices.Count - 4);
        newTriangles.Add(newVertices.Count - 4 + 2);
        newTriangles.Add(newVertices.Count - 4 + 3);

    }
}
