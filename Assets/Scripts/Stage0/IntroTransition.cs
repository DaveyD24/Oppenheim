using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
public class VectorList
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
{
    [field: SerializeField] public float Duration { get; set; }

    [field: SerializeField] public GameObject StageObject { get; set; }

    [field: SerializeField] public ListItems[] List2 { get; set; }
}

[System.Serializable]
#pragma warning disable SA1402 // File may only contain a single type
public class ListItems
#pragma warning restore SA1402 // File may only contain a single type
{
    [field: SerializeField] public Vector3 Position { get; set; }

    [field: SerializeField] public Vector3 Rotation { get; set; }

    [field: SerializeField] public bool bUseSpecificRotation { get; set; } = true;
}

public class IntroTransition : MonoBehaviour
{
    [SerializeField] private List<VectorList> cameraPos;

    public Node<IntroTransition> CameraTopNode { get; set; }

    [field: SerializeField] public GameObject Camera { get; private set; }

    [field: SerializeField] public Image Fade { get; private set; }

    private void Start()
    {
        BuildSequenceTree();
    }

    /// <summary>
    /// move the camera around the various elements, activating each scene as they go.
    /// </summary>
    private void BuildSequenceTree()
    {
        MoveCameraTrans camMoveOutside = new MoveCameraTrans(this, cameraPos[0].List2, cameraPos[0].Duration);
        ShowHideObject showHideFactory = new ShowHideObject(this, cameraPos[0].StageObject, false, Vector3.zero, Vector3.zero); // play scene transition thing here as well
        ShowHideObject showHideStage01 = new ShowHideObject(this, cameraPos[1].StageObject, true, cameraPos[1].List2[0].Position, cameraPos[1].List2[0].Rotation); // play scene transition thing here as well
        MoveCameraTrans camMoveStage01 = new MoveCameraTrans(this, cameraPos[1].List2, cameraPos[1].Duration);
        ShowHideObject showHide01Hide = new ShowHideObject(this, cameraPos[1].StageObject, false, Vector3.zero, Vector3.zero); // play scene transition thing here as well
        ShowHideObject showHide2 = new ShowHideObject(this, cameraPos[2].StageObject, true, cameraPos[2].List2[0].Position, cameraPos[2].List2[0].Rotation); // play scene transition thing here as well
        MoveCameraTrans camMoveStage02 = new MoveCameraTrans(this, cameraPos[2].List2, cameraPos[2].Duration);
        ShowHideObject showHide2Hide = new ShowHideObject(this, cameraPos[2].StageObject, false, Vector3.zero, Vector3.zero); // play scene transition thing here as well
        ShowHideObject showHideBlueprint = new ShowHideObject(this, cameraPos[3].StageObject, true, cameraPos[3].List2[0].Position, cameraPos[3].List2[0].Rotation); // play scene transition thing here as well
        MoveCameraTrans blueprintMove = new MoveCameraTrans(this, cameraPos[3].List2, cameraPos[3].Duration);

        // stage 0-1
        // black transition
        // stage 2

        // blueprints or head office or something
        // oppenheim shows up for a brief period

        // wait for input to be recieved
        CameraTopNode = new Sequence<IntroTransition>(new List<Node<IntroTransition>>
        {
            camMoveOutside, showHideFactory, showHideStage01, camMoveStage01, showHide01Hide, showHide2,
            camMoveStage02, showHide2Hide, showHideBlueprint, blueprintMove,
        });
    }

    // Update is called once per frame
    private void Update()
    {
        if (CameraTopNode != null)
        {
            CameraTopNode.Execute();
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var item in cameraPos)
        {
            foreach (ListItems pos in item.List2)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(pos.Position, Vector3.one * 20);
            }
        }
    }
}
