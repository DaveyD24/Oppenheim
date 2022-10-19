using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;

/// <summary>
/// waits until all active players have moved close enough to the object before continuing.
/// </summary>
public class MoveCloser : Node<ActionSequence>
{
    private float maxDistanceAway = 5;
    private GameObject goalObject;

    public MoveCloser(ActionSequence blackboard, GameObject goal)
    {
        this.Blackboard = blackboard;
        goalObject = goal;
    }

    public override void Init()
    {
        base.Init();
        Blackboard.oppenheimText.gameObject.SetActive(false);

        string[] inputDeviceNames = UIEvents.GetInputTypes();
        if (inputDeviceNames.Length >= 2)
        {
            UIEvents.ShowInputControls("Move", inputDeviceNames.Length, inputDeviceNames[0], inputDeviceNames[1]);
        }
        else if (inputDeviceNames.Length == 1)
        {
            UIEvents.ShowInputControls("Move", inputDeviceNames.Length, inputDeviceNames[0], string.Empty);
        }
    }

    public override ENodeState Evaluate()
    {
        return GameEvents.PlayerCompareDistance(Blackboard.MaxDistanceAway, goalObject.transform.position) ? ENodeState.Success : ENodeState.Running; // can be whatever return is nessesary
    }

    public override void End()
    {
        base.End();
        Blackboard.oppenheimText.gameObject.SetActive(true);
        UIEvents.HideInputControls();
    }
}
