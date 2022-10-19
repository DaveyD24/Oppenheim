using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitSpawnPlayers : Node<ActionSequence>
{
    private bool bIsDone;

    public WaitSpawnPlayers(ActionSequence blackboard)
    {
        this.Blackboard = blackboard;
    }

    public override void Init()
    {
        base.Init();
        bIsDone = false;
        Blackboard.StartCoroutine(WaitTime());
    }

    public override ENodeState Evaluate()
    {
        return bIsDone ? ENodeState.Success : ENodeState.Running;
    }

    private IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(4);
        foreach (GameObject player in Blackboard.DeadPlayers)
        {
            player.SetActive(false);
        }

        foreach (GameObject player in Blackboard.AlivePlayers)
        {
            player.SetActive(true);
        }

        yield return new WaitForSeconds(1);
        bIsDone = true;
    }

    public override void End()
    {
        base.End();
    }
}
