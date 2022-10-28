using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventSystem;

public class SwitchScene : Node<ActionSequence>
{
    private string sceneName;

    public SwitchScene(ActionSequence blackboard, string sceneName)
    {
        this.Blackboard = blackboard;
        this.sceneName = sceneName;
    }

    // run whenever this node gets called, and it was not the node running the previous frame
    public override void Init()
    {
        base.Init();
    }

    // called each update frame that this node returns running fall
    public override ENodeState Evaluate()
    {
        UIEvents.SceneChange(sceneName);
        return ENodeState.Success; // can be whatever return is nessesary
    }

    // when this node ends its execution, by either returning a failed or success node state
    public override void End()
    {
        base.End();
    }
}
