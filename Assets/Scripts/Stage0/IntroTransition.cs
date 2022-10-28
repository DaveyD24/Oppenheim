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

    [field: SerializeField] public Vector3[] List { get; set; }
}

public class IntroTransition : MonoBehaviour
{
    private Node<IntroTransition> cameraTopNode;

    [SerializeField] private List<VectorList> cameraPos;

    [field: SerializeField] public GameObject Camera { get; private set; }

    [field: SerializeField] public Image Fade { get; private set; }

    private void Start()
    {
        BuildSequenceTree();
    }

    private void BuildSequenceTree()
    {
        MoveCameraTrans camMoveOutside = new MoveCameraTrans(this, cameraPos[0].List, cameraPos[0].Duration);
        ShowHideObject showHideFactory = new ShowHideObject(this, cameraPos[0].StageObject, false); // play scene transition thing here as well
        ShowHideObject showHideStage01 = new ShowHideObject(this, cameraPos[1].StageObject, true); // play scene transition thing here as well
        MoveCameraTrans camMoveStage01 = new MoveCameraTrans(this, cameraPos[1].List, cameraPos[1].Duration);
        ShowHideObject showHide01Hide = new ShowHideObject(this, cameraPos[1].StageObject, false); // play scene transition thing here as well
        ShowHideObject showHide2 = new ShowHideObject(this, cameraPos[2].StageObject, true); // play scene transition thing here as well
        MoveCameraTrans camMoveStage02 = new MoveCameraTrans(this, cameraPos[2].List, cameraPos[2].Duration);
        ShowHideObject showHide2Hide = new ShowHideObject(this, cameraPos[2].StageObject, false); // play scene transition thing here as well
        ShowHideObject showHideBlueprint = new ShowHideObject(this, cameraPos[3].StageObject, true); // play scene transition thing here as well
        // black transition

        // stage 0-1
        // black transition
        // stage 2

        // blueprints or head office or something
        // oppenheim shows up for a brief period

        // wait for input to be recieved
        cameraTopNode = new Sequence<IntroTransition>(new List<Node<IntroTransition>> { camMoveOutside, showHideFactory, showHideStage01, camMoveStage01, showHide01Hide, showHide2, 
            camMoveStage02, showHide2Hide, showHideBlueprint });
    }

    // Update is called once per frame
    private void Update()
    {
        if (cameraTopNode != null)
        {
            cameraTopNode.Execute();
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var item in cameraPos)
        {
            foreach (Vector3 pos in item.List)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(pos, Vector3.one * 20);
            }
        }
    }
}
