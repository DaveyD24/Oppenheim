using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNextNode : Node<GenericBot>
{
    // find next position on the map to move towards
    public GetNextNode(GenericBot blackboard, bool isFleeing = false)
    {
        this.Blackboard = blackboard;
    }

    public override ENodeState Evaluate()
    {
        if (Blackboard.NextPosTransform)
        {
            if (Blackboard.PathToNextPos.Count <= 0)
            {
                if (Blackboard.NextPosTransform.gameObject.TryGetComponent(out NextNode nextNode))
                {
                    Blackboard.NextPosTransform = nextNode.NextNodeObj.transform;
                }

                Blackboard.PathToNextPos = Blackboard.GridPathfindingComp.APathfinding(Blackboard.transform.position, Blackboard.NextPosTransform.position); // generate the new path
            }

            if (Blackboard.PathToNextPos.Count > 0)
            {
                Blackboard.NextPosVector = Blackboard.PathToNextPos[Blackboard.PathToNextPos.Count - 1];
                Blackboard.PathToNextPos.RemoveAt(Blackboard.PathToNextPos.Count - 1);

                // as no new tiles to move towards can safely say move towards the final goal
                if (Blackboard.PathToNextPos.Count <= 0)
                {
                    Blackboard.NextPosVector = Blackboard.NextPosTransform.position;
                }
                    return ENodeState.Success;
            }
        }

        return ENodeState.Failure; //no node or path could be found
    }
}