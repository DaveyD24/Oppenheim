using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// simply either show an object or hide it, but do so in the correct sequence of the behaviour tree.
/// </summary>
public class ShowHideObject : Node<ActionSequence>
{
    private GameObject obj;
    private bool bIsActive;

    public ShowHideObject(ActionSequence blackboard, GameObject obj, bool bIsActive)
    {
        this.Blackboard = blackboard;
        this.obj = obj;
        this.bIsActive = bIsActive;
    }

    public override void Init()
    {
        base.Init();
    }

    public override ENodeState Evaluate()
    {
        obj.SetActive(bIsActive);
        return ENodeState.Success; // can be whatever return is nessesary
    }

    public override void End()
    {
        base.End();
    }
}
