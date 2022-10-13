using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScanForPlayer : Node<GenericBot>
{
    public ScanForPlayer(GenericBot blackboard)
    {
        this.Blackboard = blackboard;
    }

    public override ENodeState Evaluate()
    {
        Collider[] colliders = Physics.OverlapSphere(Blackboard.transform.position, Blackboard.SightRadius);

        foreach (Collider item in colliders)
        {
            Vector3 dirToItem = item.transform.position - Blackboard.transform.position;

            if (ValidAngle(dirToItem))
            {
                return ENodeState.Success;
            }
        }

        return ENodeState.Failure;
    }

    private bool ValidAngle(Vector3 dirToPoint)
    {
        // if x distance away and within x angle then seen player, or if a much closer distance and a much larger angle
        return Vector3.Angle(Blackboard.transform.forward, dirToPoint) < Blackboard.SightAngle / 2;
    }
}
