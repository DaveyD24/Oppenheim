using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// simply either show an object or hide it, but do so in the correct sequence of the behaviour tree.
/// </summary>
public class ShowHideObject : Node<IntroTransition>
{
    private GameObject obj;
    private bool bIsActive;
    Tween fadeAnim;
    private Vector3 camStartPos;
    private Vector3 camStartRot;

    public ShowHideObject(IntroTransition blackboard, GameObject obj, bool bIsActive, Vector3 startCamPos, Vector3 startCamRot)
    {
        this.Blackboard = blackboard;
        this.obj = obj;
        this.bIsActive = bIsActive;

        camStartPos = startCamPos;
        camStartRot = startCamRot;
    }

    public override void Init()
    {
        base.Init();

        if (!bIsActive)
        {
            fadeAnim = new Tween(new Color(0, 0, 0, 0), new Color(0, 0, 0, 1), Time.time, 1);
        }
        else
        {
            fadeAnim = new Tween(new Color(0, 0, 0, 1), new Color(0, 0, 0, 0), Time.time, 1);

            obj.SetActive(bIsActive);
            Blackboard.Camera.transform.position = camStartPos;
            Blackboard.Camera.transform.rotation = Quaternion.Euler(camStartRot);
        }
    }

    public override ENodeState Evaluate()
    {
        if (fadeAnim.IsComplete())
        {
            obj.SetActive(bIsActive);
            if (obj.gameObject.name == "Blueprint")
            {
                Blackboard.CameraTopNode = null;
            }
            return ENodeState.Success; // can be whatever return is nessesary
        }

        Blackboard.Fade.color = fadeAnim.UpdateColour();
        return ENodeState.Running;
    }

    public override void End()
    {
        base.End();
    }
}
