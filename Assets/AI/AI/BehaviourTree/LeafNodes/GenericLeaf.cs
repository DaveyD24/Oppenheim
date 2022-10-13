using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    this is just a generic function showcasing the base things each leaf node is required to contain to operate properly
    can simply copy this code into any leaf node and modify as needed
 */
public class GenericLeaf : Node<GenericBot>
{
    public GenericLeaf(GenericBot blackboard)
    {
        this.Blackboard = blackboard;
    }

    // run whenever this node gets called, and it was not the node running the previous frame
    public override void Init()
    {
        base.Init();
    }

    // called each update frame that this node returns running fall
    public override ENodeState Evaluate()
    {
        return ENodeState.Running; // can be whatever retrun is nessesary
    }

    // when this node ends its execution, by either returning a failed or success node state
    public override void End()
    {
        base.End();
    }
}