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

    public ShowHideObject(IntroTransition blackboard, GameObject obj, bool bIsActive)
    {
        this.Blackboard = blackboard;
        this.obj = obj;
        this.bIsActive = bIsActive;
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
        }
    }

    public override ENodeState Evaluate()
    {
        if (fadeAnim.IsComplete())
        {
            obj.SetActive(bIsActive);
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
