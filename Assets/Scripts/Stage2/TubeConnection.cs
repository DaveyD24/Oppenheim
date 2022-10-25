#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TubeConnection : MonoBehaviour
{
    [SerializeField] private float gizmoSize = 10;
    [SerializeField] private float scaleMultiplier = 25;
    [ContextMenuItem("Add in a new tube", "AddTube")]
    [SerializeField] public TubeToAdd add;
    [SerializeField] private int currChildHole;
    [SerializeField] private int addChildHole;
    [SerializeField] private bool bUseSpecifiedRotation = false;
    [SerializeField] private Vector3 addChildRotation;

    [SerializeField] private GameObject[] tubes;
    [SerializeField] private GameObject[] holeTubes; // the tubes which are currently over each hole

    public enum TubeToAdd
    {
        Long,
        Short,
        Corner,
        ThreeWay,
        UpDown,
        FourWay,
    }

    private void OnDrawGizmosSelected()
    {
        if (holeTubes.Length < transform.childCount)
        {
            holeTubes = new GameObject[transform.childCount];
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(transform.GetChild(i).position, Vector3.one * gizmoSize);
            Handles.color = Color.blue;
            Handles.Label(transform.GetChild(i).position, i.ToString());
        }
    }

    private void AddTube()
    {
        if (holeTubes[currChildHole] != null)
        {
            DestroyImmediate(holeTubes[currChildHole]);
        }

        GameObject addedTube = Instantiate(tubes[(int)add]);
        addedTube.transform.localScale = Vector3.one * scaleMultiplier;
        Transform child = addedTube.transform.GetChild(addChildHole);
        child.parent = null;
        addedTube.transform.parent = child;

        if (!bUseSpecifiedRotation)
        {
            child.transform.forward = -transform.GetChild(currChildHole).forward;
        }
        else
        {
            child.transform.rotation = Quaternion.Euler(addChildRotation);
        }

        child.transform.position = transform.GetChild(currChildHole).position;
        // child.transform.position += child.transform.forward * 5.25f;

        addedTube.transform.parent = null;
        child.parent = addedTube.transform;
        child.SetSiblingIndex(addChildHole);
        addedTube.transform.parent = transform.parent;

        holeTubes[currChildHole] = addedTube;
    }
}
#endif